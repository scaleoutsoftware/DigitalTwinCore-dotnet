using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench.UnitTests
{
    public class SimSharedData
    {
        class SharedDataProcessor : SimulationProcessor<SimulatedCarModel>
        {

            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Modify global shared data:
                var res = context.SharedGlobalData.Get("foo");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    context.SharedGlobalData.Put("foo", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    context.SharedGlobalData.Put("foo", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                // Modify model-specific shared data:
                res = context.SharedModelData.Get("bar");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    context.SharedModelData.Put("bar", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    context.SharedModelData.Put("bar", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                return ProcessingResult.DoUpdate;
            }

        }

        [Fact]
        public void ModifySharedData()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel("SimulatedCar", new SharedDataProcessor());

            var sc1 = new SimulatedCarModel { Speed = 2 };
            env.AddInstance("Car1", "SimulatedCar", sc1);

            env.InitializeSimulation(new DateTime(2023, 1, 1),
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));

            for (int i = 0; i < 5; i++)
            {
                var stepRes = env.Step();
                Assert.Equal(SimulationStatus.Running, stepRes.SimulationStatus);
            }

            // Check that shared global data was set:
            var res = env.SharedGlobalData.Get("foo");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            int value = BitConverter.ToInt32(res.Value);
            Assert.Equal(5, value);


            // Check that shared model data was set:
            var sharedModelData = env.GetSharedModelData("SimulatedCar");
            res = sharedModelData.Get("bar");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(5, value);
        }
    }
}

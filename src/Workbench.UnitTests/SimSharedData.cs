using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.Modules.DigitalTwin.Abstractions;
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

            public async override Task<ProcessingResult> ProcessModelAsync(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Modify global shared data:
                var res = await context.SharedGlobalData.GetAsync("foo");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    await context.SharedGlobalData.PutAsync("foo", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    await context.SharedGlobalData.PutAsync("foo", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                // Modify model-specific shared data:
                res = await context.SharedModelData.GetAsync("bar");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    await context.SharedModelData.PutAsync("bar", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    await context.SharedModelData.PutAsync("bar", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                return ProcessingResult.DoUpdate;
            }

        }

        [Fact]
        public async Task ModifySharedData()
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
                var stepRes = await env.StepAsync();
                Assert.Equal(SimulationStatus.Running, stepRes.SimulationStatus);
            }

            // Check that shared global data was set:
            var res = await env.SharedGlobalData.GetAsync("foo");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            int value = BitConverter.ToInt32(res.Value);
            Assert.Equal(5, value);


            // Check that shared model data was set:
            var sharedModelData = env.GetSharedModelData("SimulatedCar");
            res = await sharedModelData.GetAsync("bar");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(5, value);
        }
    }
}

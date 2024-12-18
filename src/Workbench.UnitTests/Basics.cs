#region Copyright notice and license

// Copyright 2023-2024 ScaleOut Software, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Scaleout.DigitalTwin.Workbench;
using Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar;
using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.Streaming.DigitalTwin.Core;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class Basics
    {
        [Fact]
        public void TwoSimObjScheduling()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new SimulatedCar.CarSimulationProcessor1());

            var car1 = new SimulatedCarModel { DelayTime = TimeSpan.FromSeconds(10) };
            env.AddInstance("Car1", nameof(SimulatedCar), car1);
            var car2 = new SimulatedCarModel { DelayTime = TimeSpan.FromSeconds(15) };
            env.AddInstance("Car2", nameof(SimulatedCar), car2);

            DateTime startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime, 
                      endTime: DateTimeOffset.MaxValue, 
                      simulationIterationInterval: TimeSpan.FromSeconds(1));

            var result = env.Step();
            Assert.Equal(startTime, env.CurrentTime);
            Assert.Equal(startTime + TimeSpan.FromSeconds(10), result.NextSimulationTime);
            Assert.Equal(startTime + TimeSpan.FromSeconds(10), env.PeekNextTimeStep());

            result = env.Step();
            Assert.Equal(startTime + TimeSpan.FromSeconds(10), env.CurrentTime);
            Assert.Equal(startTime + TimeSpan.FromSeconds(15), result.NextSimulationTime);
        }

        [Fact]
        public void AddRealTimeViaSendMsg()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new SimulatedCar.CarSimulationProcessor2());
            env.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCar.RealTimeCarMessageProcessor1());
            
            var sc = new SimulatedCarModel { Speed = 99 };
            env.AddInstance("Car1", nameof(SimulatedCar), sc);

            var startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime,
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));

            var res = env.Step(); // should cause Car1 to emit.
            Assert.Equal(SimulationStatus.Running, res.SimulationStatus);

            // Check that the RT obj got created.
            var newRealTimeCar = env.GetInstance<RealTimeCar.RealTimeCarModel>(nameof(RealTimeCar), "Car1");
            Assert.NotNull(newRealTimeCar);
            Assert.Equal(sc.Speed, newRealTimeCar!.Speed);
        }

        [Fact]
        public void SendToDataSource()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new CarSimulationProcessor3(), new SimulatedCarMessageProcessor());
            env.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor2());

            var sc = new SimulatedCarModel { Speed = 45 };
            env.AddInstance("Car1", nameof(SimulatedCar), sc);
            //var rtc = new RealTimeCarModel { Speed = 45 };
            //env.AddInstance("Car1", nameof(RealTimeCar), rtc);

            DateTime startTime = new DateTime(2023, 1, 1);

            env.InitializeSimulation(startTime,
                     endTime: DateTimeOffset.MaxValue,
                     simulationIterationInterval: TimeSpan.FromSeconds(1));

            // Should cause sim to send a message and then rt to send back to datasource:
            var res = env.Step();
            Assert.Equal(SimulationStatus.Running, res.SimulationStatus);

            var simCar = env.GetInstance<SimulatedCarModel>(nameof(SimulatedCar), "Car1");
            Assert.Equal("Too Fast", simCar!.Status);
        }

        [Fact]
        public void DeleteThisTwin()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new CarSimulationProcessor4(), new SimulatedCarMessageProcessor());
            
            var sc = new SimulatedCarModel { Speed = 45 };
            env.AddInstance("Car1", nameof(SimulatedCar), sc);

            env.InitializeSimulation(new DateTime(2023, 1, 1),
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));

            var res = env.Step(); // should call DeleteThisTwin()

            // Make sure it was removed.
            var simCar = env.GetInstance<SimulatedCarModel>(nameof(SimulatedCar), "Car1");
            Assert.Null(simCar);

            Assert.Equal(SimulationStatus.NoRemainingWork, res.SimulationStatus);
        }

        class DeleteOtherTwinProcessor : SimulationProcessor<SimulatedCarModel>
        {
            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Delete another twin when speed hits zero.
                digitalTwin.Speed = digitalTwin.Speed - 1;
                if (digitalTwin.Speed == 0)
                {
                    context.SimulationController.DeleteTwin(nameof(SimulatedCar), "Car2");
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void DeleteAnotherTwin()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new DeleteOtherTwinProcessor());

            var sc1 = new SimulatedCarModel { Speed = 2 }; // will delete Car2
            env.AddInstance("Car1", nameof(SimulatedCar), sc1);
            var sc2 = new SimulatedCarModel { Speed = 10 };
            env.AddInstance("Car2", nameof(SimulatedCar), sc2);

            env.InitializeSimulation(new DateTime(2023, 1, 1),
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));

            for (int i = 0; i < 50; i++)
            {
                var res = env.Step();
                Assert.Equal(SimulationStatus.Running, res.SimulationStatus);
            }

            // Car1 got to zero first, so it should have deleted Car2.
            var instances = env.GetInstances<SimulatedCarModel>(nameof(SimulatedCar));
            Assert.Single(instances);
            bool deletedCarFound = instances.TryGetValue("Car2", out _);
            Assert.False(deletedCarFound);

            var car1 = instances["Car1"];
            // speed should be negative after 50 steps:
            Assert.True(car1.Speed < 0);
        }

        [Fact]
        public void RealtimeToRealtimeUnderSimulation()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new CarSimulationProcessor6());
            env.AddRealTimeModel("RealtimeMessageSender", new RealTimeCarMessageProcessor3());
            env.AddRealTimeModel("RealtimeDatasourceSender", new RealTimeCarMessageProcessor4());

            var sc = new SimulatedCarModel { Speed = 45 };
            env.AddInstance("Car1", nameof(SimulatedCar), sc);
            var rtc = new RealTimeCarModel { Speed = 45 };
            env.AddInstance("Car1", "RealtimeMessageSender", rtc);

            DateTime startTime = new DateTime(2023, 1, 1);

            env.InitializeSimulation(startTime,
                     endTime: DateTimeOffset.MaxValue,
                     simulationIterationInterval: TimeSpan.FromSeconds(1));

            // Should cause sim to send a message and then rt to send another message:
            var res = env.Step();
            Assert.Equal(SimulationStatus.Running, res.SimulationStatus);

            //var simCar = env.GetInstance<SimulatedCarModel>(nameof(SimulatedCar), "Car1");
            //Assert.Equal("Too Fast", simCar!.Status);
        }
    }
}
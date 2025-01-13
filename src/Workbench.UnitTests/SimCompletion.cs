#region Copyright notice and license

// Copyright 2023-2025 ScaleOut Software, Inc.
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

using Scaleout.DigitalTwin.DevEnv.Tests.Messages;
using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.DigitalTwin.Workbench;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class SimCompletion
    {
        [Fact]
        public void NoMoreWork()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new CarSimulationProcessor5());

            var car1 = new SimulatedCarModel { Speed = 5 };
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime,
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));


            int stepCount = 0;
            for (; stepCount < 100; stepCount++) // should only take 5 steps, but don't want to loop forever if there's a bug
            {
                var stepResult = env.Step();
                if (stepResult.SimulationStatus != SimulationStatus.Running)
                {
                    Assert.Equal(SimulationStatus.NoRemainingWork, stepResult.SimulationStatus);
                    break;
                }
            }
            Assert.Equal(4, stepCount);
        }

        [Fact]
        public void NoInstances()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new CarSimulationProcessor5());

            DateTime startTime = new DateTime(2023, 1, 1);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                env.InitializeSimulation(startTime,
                          endTime: DateTimeOffset.MaxValue,
                          simulationIterationInterval: TimeSpan.FromSeconds(1));
            });

            Assert.Equal("No simulation instances were registered, no work to do.", ex.Message);
        }




        [Fact]
        public void EndTimeReached()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new DoNothingProcessor());

            var car1 = new SimulatedCarModel { Speed = 5 };
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            DateTime endTime = startTime.AddSeconds(60);
            env.InitializeSimulation(startTime, endTime,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));


            int stepCount = 0;
            for (; stepCount < 100; stepCount++) // should only take 60 steps, but don't want to loop forever if there's a bug
            {
                var stepResult = env.Step();
                if (stepResult.SimulationStatus != SimulationStatus.Running)
                {
                    Assert.Equal(SimulationStatus.EndTimeReached, stepResult.SimulationStatus);
                    break;
                }
            }
            Assert.Equal(59, stepCount);
        }

        class DelayInfiniteProcessor : SimulationProcessor<SimulatedCarModel>
        {
            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Delay forever when speed hits zero.
                digitalTwin.Speed = digitalTwin.Speed - 1;
                if (digitalTwin.Speed == 0)
                    context.SimulationController.Delay(TimeSpan.MaxValue);

                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void AllDelayed()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new DelayInfiniteProcessor());

            var car1 = new SimulatedCarModel { Speed = 5 };
            env.AddInstance("Car1", nameof(SimulatedCar), car1);
            var car2 = new SimulatedCarModel { Speed = 50 };
            env.AddInstance("Car2", nameof(SimulatedCar), car2);

            DateTime startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime,
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));


            int stepCount = 0;
            for (; stepCount < 100; stepCount++) // should only take 50 steps, but don't want to loop forever if there's a bug
            {
                var stepResult = env.Step();
                if (stepResult.SimulationStatus != SimulationStatus.Running)
                {
                    Assert.Equal(SimulationStatus.NoRemainingWork, stepResult.SimulationStatus);
                    break;
                }
            }
            Assert.Equal(49, stepCount);
        }

        class RequestStopProcessor : SimulationProcessor<SimulatedCarModel>
        {
            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Delay forever when speed hits zero.
                digitalTwin.Speed = digitalTwin.Speed - 1;
                if (digitalTwin.Speed == 0)
                    context.SimulationController.StopSimulation();

                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void StopRequested()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new RequestStopProcessor());

            var car1 = new SimulatedCarModel { Speed = 5 };
            env.AddInstance("Car1", nameof(SimulatedCar), car1);
            var car2 = new SimulatedCarModel { Speed = 50 };
            env.AddInstance("Car2", nameof(SimulatedCar), car2);

            DateTime startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime,
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));


            int stepCount = 0;
            for (; stepCount < 100; stepCount++) // should only take 5 steps, but don't want to loop forever if there's a bug
            {
                var stepResult = env.Step();
                if (stepResult.SimulationStatus != SimulationStatus.Running)
                {
                    Assert.Equal(SimulationStatus.StopRequested, stepResult.SimulationStatus);
                    break;
                }
            }
            Assert.Equal(4, stepCount);
        }

        class RunThisTwinSimProcessor : SimulationProcessor<SimulatedCarModel>
        {
            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                // Delay forever when speed hits zero.
                digitalTwin.Speed = digitalTwin.Speed - 1;

                if (digitalTwin.Speed == 10 && digitalTwin.Id == "Car_Sleeper")
                    context.SimulationController.DelayIndefinitely();
                else if (digitalTwin.Speed == 20 && digitalTwin.Id == "Car_Waker")
                    context.SendToTwin(nameof(SimulatedCar), "Car_Sleeper", new StatusMessage() { Payload = "WakeUp!" });
                else if (digitalTwin.Speed == 0)
                    context.SimulationController.DelayIndefinitely();

                return ProcessingResult.DoUpdate;
            }
        }

        class RunThisTwinMsgProcessor : MessageProcessor<SimulatedCarModel, StatusMessage>
        {
            public override ProcessingResult ProcessMessages(ProcessingContext context, SimulatedCarModel digitalTwin, IEnumerable<StatusMessage> newMessages)
            {
                foreach (var message in newMessages)
                {
                    digitalTwin.Status = message.Payload;
                    if (message.Payload == "WakeUp!")
                        context.SimulationController.RunThisTwin();
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void RunThisTwinTest()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            env.AddSimulationModel(nameof(SimulatedCar), new RunThisTwinSimProcessor(), new RunThisTwinMsgProcessor());

            var car1 = new SimulatedCarModel { Speed = 30 };
            env.AddInstance("Car_Waker", nameof(SimulatedCar), car1);
            var car2 = new SimulatedCarModel { Speed = 15 };
            env.AddInstance("Car_Sleeper", nameof(SimulatedCar), car2);

            DateTime startTime = new DateTime(2023, 1, 1);
            env.InitializeSimulation(startTime,
                      endTime: DateTimeOffset.MaxValue,
                      simulationIterationInterval: TimeSpan.FromSeconds(1));


            int stepCount = 0;
            for (; stepCount < 100; stepCount++) // should only take 29 steps
            {
                var stepResult = env.Step();
                if (stepResult.SimulationStatus != SimulationStatus.Running)
                {
                    Assert.Equal(SimulationStatus.NoRemainingWork, stepResult.SimulationStatus);
                    break;
                }
            }
            Assert.Equal(29, stepCount);
        }
    }
}

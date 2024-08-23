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

using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.DigitalTwin.Workbench;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class SimTimers
    {
        
        class StartSingleTimerProcessor : SimulationProcessor<SimulatedCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;

            public override ProcessingResult InitSimulation(InitSimulationContext context, SimulatedCarModel digitalTwin, DateTimeOffset startTime)
            {
                return ProcessingResult.NoUpdate;
            }

            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(10), TimerType.OneTime, TimerFiredHandler);
                    _timerStarted = true;
                }
                return ProcessingResult.DoUpdate;
            }

            private ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                Interlocked.Increment(ref _timerFiredCount);
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void FiresOnce()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StartSingleTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = env.RunSimulation(startTime,
                                           endTime: startTime.AddSeconds(30),
                                           simulationIterationInterval: TimeSpan.FromSeconds(1),
                                           delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(1, msgProcessor._timerFiredCount);
        }


        class StartRecurringTimerProcessor : SimulationProcessor<SimulatedCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;

            public override ProcessingResult InitSimulation(InitSimulationContext context, SimulatedCarModel digitalTwin, DateTimeOffset startTime)
            {
                return ProcessingResult.NoUpdate;
            }

            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(5), TimerType.Recurring, TimerFiredHandler);
                    _timerStarted = true;
                }
                return ProcessingResult.DoUpdate;
            }

            private ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                Assert.Equal("foo", timerName);
                Assert.Equal("Car1", instance.Id);
                Assert.NotNull(context);

                Interlocked.Increment(ref _timerFiredCount);
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void RecurringFires()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StartRecurringTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = env.RunSimulation(startTime,
                                           endTime: startTime.AddSeconds(31),
                                           simulationIterationInterval: TimeSpan.FromSeconds(1),
                                           delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(6, msgProcessor._timerFiredCount);
        }

        class StopRecurringTimerProcessor : SimulationProcessor<SimulatedCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;
            public int _processModelCount = 0;

            public override ProcessingResult InitSimulation(InitSimulationContext context, SimulatedCarModel digitalTwin, DateTimeOffset startTime)
            {
                return ProcessingResult.NoUpdate;
            }

            public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(5), TimerType.Recurring, TimerFiredHandler);
                    _timerStarted = true;
                }
                _processModelCount++;
                return ProcessingResult.DoUpdate;
            }

            private ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                _timerFiredCount++;

                if (_timerFiredCount > 2)
                {
                    context.StopTimer("foo");
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void StopRecurring()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StopRecurringTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = env.RunSimulation(startTime,
                                           endTime: startTime.AddSeconds(30),
                                           simulationIterationInterval: TimeSpan.FromSeconds(1),
                                           delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(3, msgProcessor._timerFiredCount);
            Assert.Equal(30, msgProcessor._processModelCount);

            // Make sure instance is still present
            var instances = env.GetInstances<SimulatedCarModel>(nameof(SimulatedCar));
            Assert.Equal(1, instances.Count);
        }

        class TimerTwin : DigitalTwinBase
        {
            public int TimerFiredCount; 
            public override void Init(string id, string model, InitContext initContext)
            {
                base.Init(id, model, initContext);
                initContext.StartTimer("foo", TimeSpan.FromSeconds(5), TimerType.OneTime, TimerFiredHandler);
            }

            public static ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                var twin = (TimerTwin)instance;
                twin.TimerFiredCount++;
                return ProcessingResult.DoUpdate;
            }
        }

        class InitTimerProcessor : SimulationProcessor<TimerTwin>
        {
            public override ProcessingResult InitSimulation(InitSimulationContext context, TimerTwin digitalTwin, DateTimeOffset startTime)
            {
                return ProcessingResult.NoUpdate;
            }

            public override ProcessingResult ProcessModel(ProcessingContext context, TimerTwin digitalTwin, DateTimeOffset currentTime)
            {
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void StartTimerFromInit()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new InitTimerProcessor();
            env.AddSimulationModel(nameof(TimerTwin), msgProcessor);

            var twin1 = new TimerTwin();
            env.AddInstance("Car1", nameof(TimerTwin), twin1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = env.RunSimulation(startTime,
                                           endTime: startTime.AddSeconds(30),
                                           simulationIterationInterval: TimeSpan.FromSeconds(1),
                                           delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(1, twin1.TimerFiredCount);
        }
    }
}

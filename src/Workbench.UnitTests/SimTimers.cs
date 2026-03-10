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

using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.DigitalTwin.Workbench;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class SimTimers
    {
        
        class StartSingleTimerProcessor : SimulationProcessor<SimulatedCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;
            

            public override async Task<ProcessingResult> ProcessModelAsync(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(10), TimerType.OneTime, TimerFiredAsyncHandler);
                    _timerStarted = true;
                }
                return ProcessingResult.DoUpdate;
            }

            private Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                Interlocked.Increment(ref _timerFiredCount);
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task FiresOnce()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StartSingleTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = await env.RunSimulationAsync(startTime,
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


            public override async Task<ProcessingResult> ProcessModelAsync(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(5), TimerType.Recurring, TimerFiredAsyncHandler);
                    _timerStarted = true;
                }
                return ProcessingResult.DoUpdate;
            }

            private Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                Assert.Equal("foo", timerName);
                Assert.Equal("Car1", instance.Id);
                Assert.NotNull(context);

                Interlocked.Increment(ref _timerFiredCount);
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task RecurringFires()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StartRecurringTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = await env.RunSimulationAsync(startTime,
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


            public override async Task<ProcessingResult> ProcessModelAsync(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(5), TimerType.Recurring, TimerFiredAsyncHandler);
                    _timerStarted = true;
                }
                _processModelCount++;
                return ProcessingResult.DoUpdate;
            }

            private async Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                _timerFiredCount++;

                if (_timerFiredCount > 2)
                {
                    await context.StopTimerAsync("foo");
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public async Task StopRecurring()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new StopRecurringTimerProcessor();
            env.AddSimulationModel(nameof(SimulatedCar), msgProcessor);

            var car1 = new SimulatedCarModel();
            env.AddInstance("Car1", nameof(SimulatedCar), car1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = await env.RunSimulationAsync(startTime,
                                                      endTime: startTime.AddSeconds(30),
                                                      simulationIterationInterval: TimeSpan.FromSeconds(1),
                                                      delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(3, msgProcessor._timerFiredCount);
            Assert.Equal(30, msgProcessor._processModelCount);

            // Make sure instance is still present
            var instances = env.GetInstances<SimulatedCarModel>(nameof(SimulatedCar));
            Assert.Single(instances);
        }

        class TimerTwin : DigitalTwinBase
        {
            public int TimerFiredCount; 
            public override async Task InitAsync(string id, string model, InitContext initContext)
            {
                await base.InitAsync(id, model, initContext);
                await initContext.StartTimerAsync("foo", TimeSpan.FromSeconds(5), TimerType.OneTime, TimerFiredAsyncHandler);
            }

            public static Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                var twin = (TimerTwin)instance;
                twin.TimerFiredCount++;
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        class InitTimerProcessor : SimulationProcessor<TimerTwin>
        {
            public override Task<ProcessingResult> ProcessModelAsync(ProcessingContext context, TimerTwin digitalTwin, DateTimeOffset currentTime)
            {
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task StartTimerFromInit()
        {
            SimulationWorkbench env = new SimulationWorkbench(logger: null);
            var msgProcessor = new InitTimerProcessor();
            env.AddSimulationModel(nameof(TimerTwin), msgProcessor);

            var twin1 = new TimerTwin();
            env.AddInstance("Car1", nameof(TimerTwin), twin1);

            DateTime startTime = new DateTime(2023, 1, 1);
            var result = await env.RunSimulationAsync(startTime,
                                                      endTime: startTime.AddSeconds(30),
                                                      simulationIterationInterval: TimeSpan.FromSeconds(1),
                                                      delayBetweenTimesteps: TimeSpan.Zero);

            Assert.Equal(SimulationStatus.EndTimeReached, result.SimulationStatus);
            Assert.Equal(1, twin1.TimerFiredCount);
        }
    }
}

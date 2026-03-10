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
using Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar;
using Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar;
using Scaleout.DigitalTwin.Workbench;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class RealTimeTimers
    {
        class StartSingleTimerProcessor : MessageProcessor<RealTimeCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;


            public override async Task<ProcessingResult> ProcessMessageAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(1), TimerType.OneTime, TimerFiredAsyncHandler);
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
        public async Task FiresOnceAsync()
        {
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StartSingleTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var car1 = new RealTimeCarModel();
            await endpoint.CreateTwinAsync("Car1", car1);

            var msg = new CarMessage { Speed = 55 };
            // serialize to JSON
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await endpoint.SendAsync("Car1", msgBytes); // should start timer
            await Task.Delay(2500);

            Assert.Equal(1, msgProcessor._timerFiredCount);
        }

        class StartRecurringTimerProcessor : MessageProcessor<RealTimeCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;


            public override async Task<ProcessingResult> ProcessMessageAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] messages)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(1), TimerType.Recurring, TimerFiredAsyncHandler);
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
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StartRecurringTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var msg = new CarMessage { Speed = 55 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await endpoint.SendAsync("Car1", msgBytes); // should start timer
            await Task.Delay(2500);

            Assert.Equal(2, msgProcessor._timerFiredCount);
        }

        class StopRecurringTimerProcessor : MessageProcessor<RealTimeCarModel>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;
            public int _processModelCount = 0;


            public override async Task<ProcessingResult> ProcessMessageAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
            {
                if (!_timerStarted)
                {
                    await context.StartTimerAsync("foo", TimeSpan.FromSeconds(1), TimerType.Recurring, TimerFiredAsyncHandler);
                    _timerStarted = true;
                }
                _processModelCount++;
                return ProcessingResult.DoUpdate;
            }

            private async Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                _timerFiredCount++;

                if (_timerFiredCount == 2)
                {
                    await context.StopTimerAsync("foo");
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public async Task StopRecurring()
        {
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StopRecurringTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var msg = new CarMessage { Speed = 55 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await endpoint.SendAsync("Car1", msgBytes); // should start timer
            await Task.Delay(5000); // timer should stop itself after 2 seconds

            Assert.Equal(2, msgProcessor._timerFiredCount);


            // Make sure instance is still present
            var instances = env.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.Single(instances);
        }

        [Fact]
        public async Task TimersGetDisposed()
        {
            var msgProcessor = new StartRecurringTimerProcessor();
            using (RealTimeWorkbench env = new RealTimeWorkbench(logger: null))
            {
                var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

                var msg = new CarMessage { Speed = 55 };
                byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
                await endpoint.SendAsync("Car1", msgBytes); // should start timer
                await Task.Delay(2000);

                Assert.True(msgProcessor._timerFiredCount > 0);
            }
            // Now that the environment has been disposed, make sure the timer isn't
            // firing any more.
            int fireCount = msgProcessor._timerFiredCount;
            Thread.Sleep(2000);
            Assert.Equal(fireCount, msgProcessor._timerFiredCount);
        }

        class TimerTwin : DigitalTwinBase
        {
            public int TimerFiredCount;
            public override void Init(string id, string model, InitContext initContext)
            {
                base.Init(id, model, initContext);
                initContext.StartTimer("foo", TimeSpan.FromSeconds(2), TimerType.OneTime, TimerFiredAsyncHandler);
            }

            public static Task<ProcessingResult> TimerFiredAsyncHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                var twin = (TimerTwin)instance;
                twin.TimerFiredCount++;
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        class InitTimerProcessor : MessageProcessor<TimerTwin>
        {
            public override Task<ProcessingResult> ProcessMessageAsync(ProcessingContext context, TimerTwin digitalTwin, byte[] msgBytes)
            {
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task StartTimerFromInit()
        {
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new InitTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(TimerTwin), msgProcessor);

            var twin1 = new TimerTwin();
            await endpoint.CreateTwinAsync("Twin1", twin1);

            await Task.Delay(5000);

            Assert.Equal(1, twin1.TimerFiredCount);
        }
    }
}

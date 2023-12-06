#region Copyright notice and license

// Copyright 2023 ScaleOut Software, Inc.
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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class RealTimeTimers
    {
        class StartSingleTimerProcessor : MessageProcessor<RealTimeCarModel, CarMessage>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;


            public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(1), TimerType.OneTime, TimerFiredHandler);
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
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StartSingleTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var car1 = new RealTimeCarModel();
            endpoint.CreateTwin("Car1", car1);

            var msg = new CarMessage { Speed = 55 };
            endpoint.Send("Car1", msg); // should start timer
            Thread.Sleep(2500);

            Assert.Equal(1, msgProcessor._timerFiredCount);
        }

        class StartRecurringTimerProcessor : MessageProcessor<RealTimeCarModel, CarMessage>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;


            public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(1), TimerType.Recurring, TimerFiredHandler);
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
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StartRecurringTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var msg = new CarMessage { Speed = 55 };
            endpoint.Send("Car1", msg); // should start timer
            Thread.Sleep(2500);

            Assert.Equal(2, msgProcessor._timerFiredCount);
        }

        class StopRecurringTimerProcessor : MessageProcessor<RealTimeCarModel, CarMessage>
        {
            private bool _timerStarted = false;
            public int _timerFiredCount = 0;
            public int _processModelCount = 0;


            public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
            {
                if (!_timerStarted)
                {
                    context.StartTimer("foo", TimeSpan.FromSeconds(1), TimerType.Recurring, TimerFiredHandler);
                    _timerStarted = true;
                }
                _processModelCount++;
                return ProcessingResult.DoUpdate;
            }

            private ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                _timerFiredCount++;

                if (_timerFiredCount == 2)
                {
                    context.StopTimer("foo");
                }
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void StopRecurring()
        {
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new StopRecurringTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

            var msg = new CarMessage { Speed = 55 };
            endpoint.Send("Car1", msg); // should start timer
            Thread.Sleep(5000); // timer should stop itself after 2 seconds

            Assert.Equal(2, msgProcessor._timerFiredCount);


            // Make sure instance is still present
            var instances = env.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.Equal(1, instances.Count);
        }

        [Fact]
        public void TimersGetDisposed()
        {
            var msgProcessor = new StartRecurringTimerProcessor();
            using (RealTimeWorkbench env = new RealTimeWorkbench(logger: null))
            {
                var endpoint = env.AddRealTimeModel(nameof(RealTimeCar), msgProcessor);

                var msg = new CarMessage { Speed = 55 };
                endpoint.Send("Car1", msg); // should start timer
                Thread.Sleep(2000);

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
                initContext.StartTimer("foo", TimeSpan.FromSeconds(2), TimerType.OneTime, TimerFiredHandler);
            }

            public static ProcessingResult TimerFiredHandler(string timerName, DigitalTwinBase instance, ProcessingContext context)
            {
                var twin = (TimerTwin)instance;
                twin.TimerFiredCount++;
                return ProcessingResult.DoUpdate;
            }
        }

        class InitTimerProcessor : MessageProcessor<TimerTwin, CarMessage>
        {
            public override ProcessingResult ProcessMessages(ProcessingContext context, TimerTwin digitalTwin, IEnumerable<CarMessage> newMessages)
            {
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void StartTimerFromInit()
        {
            using RealTimeWorkbench env = new RealTimeWorkbench(logger: null);
            var msgProcessor = new InitTimerProcessor();
            var endpoint = env.AddRealTimeModel(nameof(TimerTwin), msgProcessor);

            var twin1 = new TimerTwin();
            endpoint.CreateTwin("Twin1", twin1);

            Thread.Sleep(5000);

            Assert.Equal(1, twin1.TimerFiredCount);
        }
    }
}

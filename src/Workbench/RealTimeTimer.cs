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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class RealTimeTimer
    {
        public string TimerName { get; }

        public TimerType TimerType { get; }

        public TimeSpan Interval { get; }

        public InstanceRegistration InstanceRegistration { get; }

        private ILogger _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private RealTimeWorkbench _env;

        private Task _timerTask;

        public RealTimeTimer(InstanceRegistration instanceRegistration,
                                 string timerName,
                                 TimerType timerType,
                                 TimeSpan interval,
                                 TimerHandler callback,
                                 RealTimeWorkbench env,
                                 ILogger? logger = null)
        {
            TimerName = timerName;
            TimerType = timerType;
            Interval = interval;
            InstanceRegistration = instanceRegistration;
            _cancellationTokenSource = new CancellationTokenSource();
            _env = env;
            _logger = logger ?? NullLogger.Instance;

            var ct = _cancellationTokenSource.Token;
            _timerTask = new Task(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(Interval, ct);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    if (ct.IsCancellationRequested)
                        break;
                    
                    try
                    {
                        ProcessingContext processingContext = new RealTimeProcessingContext(
                            messageSourceContext: null, // no data source when just firing a timer
                            instanceRegistration,
                            env,
                            0,
                            _logger);

                        _ = callback(timerName, instanceRegistration.DigitalTwinInstance, processingContext);
                        if (timerType == TimerType.OneTime)
                        {
                            _env.Timers.Remove(timerName);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception thrown from timer {TimerName} for instance {ModelName}\\{InstanceId}", timerName, instanceRegistration.ModelRegistration.ModelName, instanceRegistration.DigitalTwinInstance.Id);
                    }
                }
            });
        }


        public void Start()
        {
            _timerTask.Start();
        }

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            return _timerTask;
        }

       

    }
}

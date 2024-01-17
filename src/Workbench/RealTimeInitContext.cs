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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class RealTimeInitContext : InitContext
    {
        InstanceRegistration _instanceRegistration;
        RealTimeWorkbench _env;
        ILogger _logger;

        public RealTimeInitContext(InstanceRegistration instanceRegistration,
                                   RealTimeWorkbench env,
                                   ILogger logger)
        {
            _instanceRegistration = instanceRegistration;
            _env = env;
            _logger = logger;                
        }
        public override TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerHandler timerCallback)
        {
            RealTimeTimer timer = new RealTimeTimer(
                                                _instanceRegistration,
                                                timerName,
                                                type,
                                                interval,
                                                timerCallback,
                                                _env,
                                                _logger);
            bool added = _env.Timers.TryAdd(timerName, timer);
            if (added)
            {
                timer.Start();
                return TimerActionResult.Success;
            }
            else
            {
                return TimerActionResult.FailedTimerAlreadyExists;
            }
        }

        public override ISharedData SharedModelData => _instanceRegistration.ModelRegistration.SharedModelData;

        public override ISharedData SharedGlobalData => _env.SharedGlobalData;
    }
}

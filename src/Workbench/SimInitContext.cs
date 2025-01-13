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

using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimInitContext : InitContext
    {
        InstanceRegistration _instanceRegistration;
        SimulationWorkbench _env;

        public SimInitContext(InstanceRegistration instanceRegistration, SimulationWorkbench env)
        {
            _instanceRegistration = instanceRegistration;
            _env = env;
        }
        public override TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerHandler timerCallback)
        {
            if (timerName == null) throw new ArgumentNullException(nameof(timerName));
            if (timerCallback == null) throw new ArgumentNullException(nameof(timerCallback));
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

            SimulationTimer timerRegistration = new SimulationTimer(_instanceRegistration,
                                                                    timerName,
                                                                    type,
                                                                    interval,
                                                                    timerCallback);
            bool added = _env.Timers.TryAdd(timerName, timerRegistration);
            if (added)
            {
                if (_env.EventGenerator == null)
                {
                    // This means that a timer is being added prior to the simulation
                    // being added--probably via a model's Init() implementation.
                    // So take no action--the SimulationWorkbench's InitializeSimulation
                    // method will set up the timer later.
                }
                else
                {
                    _env.EventGenerator.EnqueueEvent(timerRegistration, _env.CurrentTime + timerRegistration.Interval);
                }
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

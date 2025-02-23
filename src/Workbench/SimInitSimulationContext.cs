﻿#region Copyright notice and license

// Copyright 2024-2025 ScaleOut Software, Inc.
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

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimInitSimulationContext : InitSimulationContext
    {
        InstanceRegistration _instanceRegistration;
        SimulationWorkbench _env;

        public SimInitSimulationContext(InstanceRegistration instanceRegistration, SimulationWorkbench env)
        {
            _instanceRegistration = instanceRegistration;
            _env = env;
        }

        public override ISharedData SharedModelData => _instanceRegistration.ModelRegistration.SharedModelData;

        public override ISharedData SharedGlobalData => _env.SharedGlobalData;
    }
}

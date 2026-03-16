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

using Microsoft.Extensions.Logging;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal abstract class ModelRegistration
    {
        public ModelRegistration(string modelName, ISharedData sharedModelData, RealTimeWorkbench? realTimeWorkbench, SimulationWorkbench? simulationWorkbench)
        {
            ModelName = modelName;
            SharedModelData = sharedModelData;
            SimulationWorkbench = simulationWorkbench;
            RealTimeWorkbench = realTimeWorkbench;
        }

        public SimulationWorkbench? SimulationWorkbench { get; }

        public RealTimeWorkbench? RealTimeWorkbench { get; }

        public string ModelName { get; }

        public SimulationProcessor? SimulationProcessor { get; set; }
        public MessageProcessor? MessageProcessor { get; set; }

        public ISharedData SharedModelData { get; set; }

        public abstract ProcessingResult InitSimulation(InitSimulationContext initSimulationContext, InstanceRegistration instanceRegistration, DateTimeOffset simulationTime);
        public abstract Task<SimProcessingResult> ProcessModelAsync(InstanceRegistration instanceRegistration, DateTimeOffset simulationTime, int messageDepth, ILogger? logger = null);
        public abstract Task<ProcessingResult> ProcessMessageAsync(InstanceRegistration instanceRegistration, byte[] msgBytes, int messageDepth, ILogger? logger = null);
        public abstract Task<InstanceRegistration> CreateNewInitializedInstanceAsync(string instanceId, InstanceRegistration? dataSource, ILogger logger);
    }
}

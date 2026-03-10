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

using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal delegate ProcessingResult InitSimulationInvoker(InitSimulationContext initSimulationContext, DigitalTwinBase dtInstance, DateTimeOffset simulationTime);

    internal delegate Task<ProcessingResult> ProcessModelAsyncInvoker(ProcessingContext processingContext, DigitalTwinBase dtInstance, DateTimeOffset simulationTime);

    internal delegate Task<ProcessingResult> ProcessMessagesAsyncInvoker(ProcessingContext processingContext, DigitalTwinBase dtInstance, byte[] msgBytes);

    internal delegate object? MessageDeserializer(byte[] message);

    internal delegate DigitalTwinBase CreateNew();

    internal class ModelRegistration
    {
        public ModelRegistration(string modelName, ISharedData sharedModelData)
        {
            ModelName = modelName;
            SharedModelData = sharedModelData;
        }
        public string ModelName { get; }

        public SimulationProcessor? SimulationProcessor { get; set; }

        public ISharedData SharedModelData { get; set; }

        public InitSimulationInvoker? InvokeInitSimulation;

        public ProcessModelAsyncInvoker? InvokeProcessModelAsync { get; set; }

        public MessageProcessor? MessageProcessor { get; set; }

        public ProcessMessagesAsyncInvoker? InvokeProcessMessagesAsync { get; set; }

        public CreateNew? CreateNew { get; set; }
    }
}

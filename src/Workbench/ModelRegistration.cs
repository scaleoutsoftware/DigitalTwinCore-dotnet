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
    internal delegate ProcessingResult InitSimulationInvoker(InitSimulationContext initSimulationContext, DigitalTwinBase dtInstance, DateTimeOffset simulationTime);

    internal delegate ProcessingResult ProcessModelInvoker(ProcessingContext processingContext, DigitalTwinBase dtInstance, DateTimeOffset simulationTime);

    internal delegate ProcessingResult ProcessMessagesInvoker(ProcessingContext processingContext, DigitalTwinBase dtInstance, IEnumerable<object> messages);

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

        public ProcessModelInvoker? InvokeProcessModel;

        public MessageProcessor? MessageProcessor { get; set; }

        public ProcessMessagesInvoker? InvokeProcessMessages { get; set; }

        public MessageDeserializer? DeserializeMessage { get; set; }

        public CreateNew? CreateNew { get; set; }
    }
}

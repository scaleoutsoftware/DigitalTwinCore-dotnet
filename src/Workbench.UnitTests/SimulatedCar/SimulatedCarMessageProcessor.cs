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
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar
{
    internal class SimulatedCarMessageProcessor : MessageProcessor<SimulatedCarModel>
    {
        public override Task<ProcessingResult> ProcessMessageAsync(ProcessingContext context, SimulatedCarModel digitalTwin, byte[] msgBytes)
        {
            StatusMessage? statusMsg = System.Text.Json.JsonSerializer.Deserialize<StatusMessage>(msgBytes);
            Assert.NotNull(statusMsg);
            digitalTwin.Status = statusMsg.Payload;
            return Task.FromResult(ProcessingResult.DoUpdate);
        }
    }
}

﻿#region Copyright notice and license

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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar
{
    internal class SimulatedCarMessageProcessor : MessageProcessor<SimulatedCarModel, StatusMessage>
    {
        public override ProcessingResult ProcessMessages(ProcessingContext context, SimulatedCarModel digitalTwin, IEnumerable<StatusMessage> newMessages)
        {
            foreach (var message in newMessages)
            {
                digitalTwin.Status = message.Payload;
            }
            return ProcessingResult.DoUpdate;
        }
    }
}

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

namespace Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar
{
    public class RealTimeCarMessageProcessor1 : MessageProcessor<RealTimeCarModel>
    {
        public override Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            digitalTwin.Speed = carMsg.Speed;
            return Task.FromResult(ProcessingResult.DoUpdate);
        }
    }

    public class RealTimeCarMessageProcessor2 : MessageProcessor<RealTimeCarModel>
    {
        public override async Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            digitalTwin.Speed = carMsg.Speed;
            var msg = new StatusMessage() { Payload = "Too Fast" };
            byte[] msgBytesOut = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await context.SendToDataSourceAsync(msgBytesOut);
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor3 : MessageProcessor<RealTimeCarModel>
    {
        public int MessagesReceived = 0;

        public override async Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            if (MessagesReceived > 0)
            {
                throw new InvalidOperationException("Only one message should be received");
            }
            MessagesReceived++;

            // Send to another RT model. Since we're already in a
            // RT model, the target should not have a datasource, therefore
            // it should not send a message back to us.
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            digitalTwin.Speed = carMsg.Speed;
            await context.SendToTwinAsync("RealtimeDatasourceSender", "Car2", msgBytes);
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor4 : MessageProcessor<RealTimeCarModel>
    {
        public override async Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            digitalTwin.Speed = carMsg.Speed;
            var msg = new StatusMessage() { Payload = "Too Fast" };
            // this call should fail because we shouldn't have a data source--this 
            // twin was created by another RT twin.
            byte[] msgBytesOut = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SendToDataSourceAsync(msgBytesOut));
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor5 : MessageProcessor<RealTimeCarModel>
    {
        public override async Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            if (carMsg.Speed == 42)
            {
                // delete Car1
                await context.RemoveRealTimeTwinAsync(null, "Car1");
            }
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor6 : MessageProcessor<RealTimeCarModel>
    {
        public override Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, RealTimeCarModel digitalTwin, byte[] msgBytes)
        {
            CarMessage? carMsg = System.Text.Json.JsonSerializer.Deserialize<CarMessage>(msgBytes);
            Assert.NotNull(carMsg);
            if (carMsg.Speed == 42)
                return Task.FromResult(ProcessingResult.Remove);
            else
                return Task.FromResult(ProcessingResult.DoUpdate);
        }
    }
}

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

using Scaleout.DigitalTwin.DevEnv.Tests.Messages;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar
{
    public class RealTimeCarMessageProcessor1 : MessageProcessor<RealTimeCarModel, CarMessage>
    {
        public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
        {
            foreach (var message in newMessages)
            {
                digitalTwin.Speed = message.Speed;
            }
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor2 : MessageProcessor<RealTimeCarModel, CarMessage>
    {
        public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
        {
            foreach (var message in newMessages)
            {
                digitalTwin.Speed = message.Speed;
                var msg = new StatusMessage() { Payload = "Too Fast" };
                context.SendToDataSource(msg);
            }
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor3 : MessageProcessor<RealTimeCarModel, CarMessage>
    {
        public int MessagesReceived = 0;

        public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
        {
            foreach (var message in newMessages)
            {
                if (MessagesReceived > 0)
                {
                    throw new InvalidOperationException("Only one message should be received");
                }
                MessagesReceived++;

                // Send to another RT model. Since we're already in a
                // RT model, the target should not have a datasource, therefore
                // it should not send a message back to us.
                digitalTwin.Speed = message.Speed;
                context.SendToTwin("RealtimeDatasourceSender", "Car2", message);
            }
            return ProcessingResult.DoUpdate;
        }
    }

    public class RealTimeCarMessageProcessor4 : MessageProcessor<RealTimeCarModel, CarMessage>
    {
        public override ProcessingResult ProcessMessages(ProcessingContext context, RealTimeCarModel digitalTwin, IEnumerable<CarMessage> newMessages)
        {
            foreach (var message in newMessages)
            {
                digitalTwin.Speed = message.Speed;
                var msg = new StatusMessage() { Payload = "Too Fast" };
                // this call should fail because we shouldn't have a data source--this 
                // twin was created by another RT twin.
                Assert.Throws<InvalidOperationException>(() => context.SendToDataSource(msg));
            }
            return ProcessingResult.DoUpdate;
        }
    }
}

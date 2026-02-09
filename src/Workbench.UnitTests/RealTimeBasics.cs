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
using Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar;
using Scaleout.DigitalTwin.Workbench;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests
{
    public class RealTimeBasics
    {
        [Fact]
        public async Task SendMessageAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor1());

            var msg = new CarMessage { Speed = 22 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await endpoint.SendAsync("Car1", msgBytes);

            var instances = wb.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.True(instances.ContainsKey("Car1"));
        }

        [Fact]
        public async Task SendToDataSourceAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            bool msgReceived = false;
            wb.DataSourceMessageReceived += (sender, e) =>
            {
                msgReceived = true;
                StatusMessage? statusMessage = System.Text.Json.JsonSerializer.Deserialize<StatusMessage>(e.Message);
                Assert.NotNull(statusMessage);
                Assert.Equal("Too Fast", statusMessage.Payload);
                Assert.Equal("Car1", e.DigitalTwinId);
                Assert.Equal(nameof(RealTimeCar), e.ModelName); 
            };

            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor2());
            var msg = new CarMessage { Speed = 22 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            await endpoint.SendAsync("Car1", msgBytes);

            
            Assert.True(msgReceived);
        }

        [Fact]
        public async Task RemoveInstanceAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor5());

            var msg22 = new CarMessage { Speed = 22 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg22);
            await endpoint.SendAsync("Car1", msgBytes);

            var msg42 = new CarMessage { Speed = 42 };
            msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg42);
            await endpoint.SendAsync("Car2", msgBytes); // should delete car1

            var instances = wb.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.False(instances.ContainsKey("Car1"));
            Assert.True(instances.ContainsKey("Car2"));
        }

        [Fact]
        public async Task SelfDestructAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor6());

            var msg22 = new CarMessage { Speed = 22 };
            byte[] msgBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg22);
            await endpoint.SendAsync("Car1", msgBytes);

            var msg42 = new CarMessage { Speed = 42 };
            byte[] msg42Bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg42);
            await endpoint.SendAsync("Car2", msg42Bytes); // should delete itself

            var instances = wb.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.True(instances.ContainsKey("Car1"));
            Assert.False(instances.ContainsKey("Car2"));
        }
    }
}

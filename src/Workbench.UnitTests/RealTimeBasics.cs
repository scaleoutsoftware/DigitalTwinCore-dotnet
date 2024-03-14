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
        public void SendMessage()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor1());

            endpoint.Send("Car1", new CarMessage { Speed = 22 });

            var instances = wb.GetInstances<RealTimeCarModel>(nameof(RealTimeCar));
            Assert.True(instances.ContainsKey("Car1"));
        }

        [Fact]
        public void SendToDataSource()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            bool msgReceived = false;
            wb.DataSourceMessageReceived += (sender, e) =>
            {
                msgReceived = true;
                Assert.Equal("Too Fast", ((StatusMessage)e.Message).Payload);
                Assert.Equal("Car1", e.DigitalTwinId);
                Assert.Equal(nameof(RealTimeCar), e.ModelName); 
            };

            var endpoint = wb.AddRealTimeModel(nameof(RealTimeCar), new RealTimeCarMessageProcessor2());

            endpoint.Send("Car1", new CarMessage { Speed = 22 });

            
            Assert.True(msgReceived);
        }
    }
}

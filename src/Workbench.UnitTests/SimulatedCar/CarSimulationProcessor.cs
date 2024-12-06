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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.DevEnv.Tests.SimulatedCar
{

    class DoNothingProcessor : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor1 : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            digitalTwin.Speed = RandomNumberGenerator.GetInt32(45, 65);
            context.SimulationController.Delay(digitalTwin.DelayTime);
            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor2 : SimulationProcessor<SimulatedCarModel>
    {
        
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            CarMessage msg = new CarMessage() { Speed = digitalTwin.Speed };
            context.SimulationController.EmitTelemetry(nameof(RealTimeCar), msg);

            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor3 : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            digitalTwin.Speed = 75;
            CarMessage msg = new CarMessage() { Speed = digitalTwin.Speed };
            context.SimulationController.EmitTelemetry(nameof(RealTimeCar), msg);
            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor4 : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            context.SimulationController.DeleteThisTwin();
            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor5 : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            // use speed as a countdown to zero, then delete the DT instance.
            digitalTwin.Speed = digitalTwin.Speed - 1;
            if (digitalTwin.Speed == 0) 
            {
                context.SimulationController.DeleteThisTwin();
            }
            return ProcessingResult.DoUpdate;
        }
    }

    public class CarSimulationProcessor6 : SimulationProcessor<SimulatedCarModel>
    {
        public override ProcessingResult ProcessModel(ProcessingContext context, SimulatedCarModel digitalTwin, DateTimeOffset currentTime)
        {
            digitalTwin.Speed = 75;
            CarMessage msg = new CarMessage() { Speed = digitalTwin.Speed };
            context.SimulationController.EmitTelemetry("RealtimeMessageSender", msg);
            return ProcessingResult.DoUpdate;
        }
    }
}

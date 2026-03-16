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
    internal interface ISimulationTimer
    {
        string TimerName { get; }
        TimerType TimerType { get; }
        TimeSpan Interval { get; }
        Task<SimProcessingResult> InvokeTimerAsync();
    }

    internal class SimulationTimer<TDigitalTwin> : InstanceRegistration<TDigitalTwin>, ISimulationTimer
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>, new()
    {
        public string TimerName { get; set; }

        public TimerType TimerType { get; set; }

        public TimeSpan Interval { get; set; }

        public TimerAsyncHandler<TDigitalTwin> TimerCallback { get; set; }

        public SimulationTimer(InstanceRegistration<TDigitalTwin> instanceRegistration,
                               string timerName,
                               TimerType timerType,
                               TimeSpan interval,
                               TimerAsyncHandler<TDigitalTwin> callback) 
            : base(instanceRegistration.InstanceId, instanceRegistration.DigitalTwinInstance, instanceRegistration.ModelRegistration, null)
        {
            TimerName = timerName;
            TimerType = timerType;
            Interval = interval;
            TimerCallback = callback;
        }

        public async Task<SimProcessingResult> InvokeTimerAsync()
        {
            if (ModelRegistration.SimulationWorkbench == null)
                throw new InvalidOperationException("Model was not registered in SimulationWorkbench.");

            var processingContext = new SimProcessingContext<TDigitalTwin>(this, ModelRegistration.SimulationWorkbench, messageDepth: 0, logger: null);
            ProcessingResult processingResult = await TimerCallback(TimerName, DigitalTwinInstance, processingContext);

            return new SimProcessingResult
            {
                ProcessingResult = processingResult,
                DeleteRequested = processingContext.DeleteRequested,
                StopRequested = processingContext.StopRequested,
                RequestedSimulationCycleDelay = processingContext.RequestedSimulationCycleDelay
            };  
        }
    }
}

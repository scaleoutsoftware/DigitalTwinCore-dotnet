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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class RealTimeProcessingContext : ProcessingContext
    {
        private RealTimeWorkbench _env;
        private ILogger _logger;
        internal string InstanceId { get; }
        internal InstanceRegistration InstanceRegistration { get; }

        const int MAX_MESSAGE_DEPTH = 100;
        private int _messageDepth;

        internal RealTimeProcessingContext? MessageSourceContext;


        public RealTimeProcessingContext(RealTimeProcessingContext? messageSourceContext,
                                      InstanceRegistration instanceRegistration,
                                      RealTimeWorkbench env,
                                      int messageDepth,
                                      ILogger? logger = null)
        {
            MessageSourceContext = messageSourceContext;
            DigitalTwinModel = instanceRegistration.ModelRegistration.ModelName;
            InstanceId = instanceRegistration.DigitalTwinInstance.Id;
            InstanceRegistration = instanceRegistration;
            _env = env;
            _messageDepth = messageDepth;
            AnomalyDetectionProviders = _env.AnomalyDetectionProviders?.ContainsKey(DigitalTwinModel) == true ? 
                _env.AnomalyDetectionProviders[DigitalTwinModel] : new Dictionary<string, IAnomalyDetectionProvider>();

            _logger = logger ?? NullLogger.Instance;
        }


        public override string DigitalTwinModel { get; }

        public override IDigitalTwinModelProvider AzureDigitalTwinsProvider => throw new NotSupportedException();

        public override Dictionary<string, IAnomalyDetectionProvider> AnomalyDetectionProviders { get; }

        public override ISimulationController? SimulationController => null;

        public override ISharedData SharedModelData => InstanceRegistration.ModelRegistration.SharedModelData;

        public override ISharedData SharedGlobalData => _env.SharedGlobalData;

        public override DateTimeOffset GetCurrentTime()
        {
            return DateTimeOffset.UtcNow;
        }

        public override Task LogMessageAsync(LogSeverity severity, string message)
        {
            _logger.Log(severity.ToLogLevel(), message);
            return Task.CompletedTask;
        }

        public override Task SendAlertAsync(string providerName, AlertMessage alertMessage)
        {
            _env.RecordAlert(providerName, alertMessage);
            return Task.CompletedTask;
        }

        public override Task SendAlertAsync(AlertMessage alertMessage)
        {
            return SendAlertAsync("default", alertMessage);
        }

        public override Task SendToDataSourceAsync(byte[] message)
        {
            _env.SendToDataSouce(InstanceId, DigitalTwinModel, message);
            return Task.CompletedTask;
        }

        public override Task SendToDataSourceAsync(IEnumerable<byte[]> messages)
        {
            foreach (var msg in messages)
            {
                SendToDataSourceAsync(msg);
            }
            return Task.CompletedTask;
        }

        public override Task SendToTwinAsync(string targetTwinModel, string targetTwinId, byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return SendToTwinAsync(targetTwinModel, targetTwinId, messages);
        }

        public async override Task SendToTwinAsync(string targetTwinModel, string targetTwinId, IEnumerable<byte[]> messages)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            // Undocumented feature of the real ProcessingContextInternal class: If the targetTwinModel is null/empty,
            // assuming we're sending a message to another instanceRegistration in the same model.
            if (string.IsNullOrEmpty(targetTwinModel))
                targetTwinModel = this.DigitalTwinModel;

            bool foundModel = _env.Instances.TryGetValue(targetTwinModel, out var instances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {targetTwinModel} not found. Register it first with the {nameof(RealTimeWorkbench)} before sending message to it.");

            var instanceRegistration = instances.GetOrAdd(targetTwinId, key =>
            {
                // Create a new one:
                bool foundModelRegistration = _env.Models.TryGetValue(targetTwinModel, out var modelRegistration);
                if (!foundModel)
                    throw new KeyNotFoundException($"Model {targetTwinModel} not found when trying to create new instanceRegistration.");
                if (modelRegistration.CreateNew == null)
                    throw new InvalidOperationException($"Model {targetTwinModel} is not able to create new instances.");

                DigitalTwinBase newInstance = modelRegistration.CreateNew();
                var registration = new InstanceRegistration(newInstance, modelRegistration, dataSource: null);

                var initContext = new RealTimeInitContext(registration, _env, _logger);
                newInstance.InitInternalAsync(targetTwinId, targetTwinModel, initContext).GetAwaiter().GetResult();
                return registration;
            });

            if (instanceRegistration.ModelRegistration.InvokeProcessMessagesAsync == null)
                throw new InvalidOperationException("Model was not configured to process messages.");


            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {targetTwinModel}\\{targetTwinId}");

            var processingContext = new RealTimeProcessingContext(null, // no data source when just sending a message.
                                                                  instanceRegistration,
                                                                  _env,
                                                                  nextMessageDepth,
                                                                  _logger);

            foreach (var message in messages)
            {
                await instanceRegistration.ModelRegistration.InvokeProcessMessagesAsync(processingContext,
                                                                          instanceRegistration.DigitalTwinInstance,
                                                                          message);
            }

        }

        public override TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerAsyncHandler timerCallback)
        {
            RealTimeTimer timer = new RealTimeTimer(
                                                InstanceRegistration,
                                                timerName,
                                                type,
                                                interval,
                                                timerCallback,
                                                _env,
                                                _logger);
            bool added = _env.Timers.TryAdd(timerName, timer);
            if (added)
            {
                timer.Start();
                return TimerActionResult.Success;
            }
            else
            {
                return TimerActionResult.FailedTimerAlreadyExists;
            }
            
        }

        public override TimerActionResult StopTimer(string timerName)
        {
            bool found = _env.Timers.TryGetValue(timerName, out var timerReg);
            if (!found)
                return TimerActionResult.FailedNoSuchTimer;
            else
            {
                timerReg.Stop().GetAwaiter().GetResult();
                _env.Timers.Remove(timerName);
                return TimerActionResult.Success;
            }
        }

        public override Task RemoveRealTimeTwinAsync(string targetTwinModel, string targetTwinId)
        {
            // Undocumented feature of the real ProcessingContextInternal class: If the targetTwinModel is null/empty,
            // assuming we're working on another instanceRegistration in the same model.
            if (string.IsNullOrEmpty(targetTwinModel))
                targetTwinModel = this.DigitalTwinModel;

            bool foundModel = _env.Instances.TryGetValue(targetTwinModel, out var instances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {targetTwinModel} not found. Register it first with the {nameof(RealTimeWorkbench)} before sending message to it.");

            instances.TryRemove(targetTwinId, out _);

            return Task.CompletedTask;
        }
    }
}

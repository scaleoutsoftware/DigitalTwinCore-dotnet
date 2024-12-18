#region Copyright notice and license

// Copyright 2023-2024 ScaleOut Software, Inc.
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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimProcessingContext : ProcessingContext, ISimulationController
    {
        private SimulationWorkbench _env;
        private ILogger _logger;

        internal string InstanceId { get; }

        const int MAX_MESSAGE_DEPTH = 100;
        private int _messageDepth;

        internal SimProcessingContext(InstanceRegistration instanceRegistration,
                                      SimulationWorkbench env,
                                      int messageDepth,
                                      ILogger? logger = null)
        {
            DigitalTwinModel = instanceRegistration.ModelRegistration.ModelName;
            InstanceId = instanceRegistration.DigitalTwinInstance.Id;
            _env = env;
            _messageDepth = messageDepth;
            InstanceRegistration = instanceRegistration;

            _logger = logger ?? NullLogger.Instance;
        }

        public override string? DataSourceId {
            get => InstanceRegistration.DataSource?.DigitalTwinInstance.Id;
        }

        public override string DigitalTwinModel { get; }

        internal TimeSpan RequestedSimulationCycleDelay { get; set; } = TimeSpan.Zero;

        internal bool StopRequested { get; set; } = false;

        public bool DeleteRequested { get; set; } = false;

        public override IPersistenceProvider PersistenceProvider => throw new NotImplementedException();

        public override Dictionary<string, IAnomalyDetectionProvider> AnomalyDetectionProviders => throw new NotImplementedException();


        public override ISimulationController SimulationController => this as ISimulationController;

        public override ISharedData SharedModelData => InstanceRegistration.ModelRegistration.SharedModelData;

        public override ISharedData SharedGlobalData => _env.SharedGlobalData;

        public InstanceRegistration InstanceRegistration { get; }

        public SendingResult CreateTwin(string modelName, string twinId, object newInstance)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("modelName cannot be null or whitespace");
            if (string.IsNullOrWhiteSpace(twinId))
                throw new ArgumentException("twinId cannot be null or whitespace");
            if (newInstance == null)
                throw new ArgumentNullException(nameof(newInstance));

            DigitalTwinBase? dtInstance = newInstance as DigitalTwinBase;
            if (dtInstance == null)
                throw new ArgumentException("newInstance must be a model that derives from DigitalTwinBase");

            _env.AddInstance(modelName, twinId, dtInstance);

            ModelRegistration modelRegistration = _env.Models[modelName];
            if (modelRegistration.SimulationProcessor != null)
            {
                // TODO: enqueue for immediate processing of this time step
                throw new NotImplementedException();
            }
            return SendingResult.Handled;
        }

        public SendingResult CreateTwinFromPersistenceStore(string modelName, string twinId, object defaultInstance)
        {
            throw new NotSupportedException();
        }

        public SendingResult CreateTwinFromPersistenceStore(string modelName, string twinId)
        {
            throw new NotSupportedException();
        }

        public SendingResult Delay(TimeSpan delay)
        {
            this.RequestedSimulationCycleDelay = delay;
            return SendingResult.Handled;
        }

        public SendingResult DelayIndefinitely()
        {
            this.RequestedSimulationCycleDelay = TimeSpan.MaxValue;
            return SendingResult.Handled;
        }

        public SendingResult DeleteThisTwin()
        {
            DeleteRequested = true; // prevents simulation instance from being re-enqueud in the scheduler.

            _env.RemoveInstance(this.DigitalTwinModel, this.InstanceId);
            return SendingResult.Handled;
        }

        public void RunThisTwin()
        {
            _env.EnqueueImmediate(this.InstanceRegistration);
        }

        public SendingResult DeleteTwin(string modelName, string twinId)
        {
            _env.RemoveInstance(modelName, twinId);
            return SendingResult.Handled;
        }

        public SendingResult EmitTelemetry(string modelName, byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            bool foundModel = _env.Models.TryGetValue(modelName, out var model);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {modelName} not found. Register it first with the {nameof(SimulationWorkbench)} before sending telemetry to it.");

            if (model.DeserializeMessage == null)
                throw new InvalidOperationException("Model was not configured to process messages.");

            object? deserializedMsg = model.DeserializeMessage(message);
            if (deserializedMsg == null)
                throw new ArgumentException("message deserializes to null", nameof(message));

            return EmitTelemetry(modelName, deserializedMsg);
        }

        public SendingResult EmitTelemetry(string modelName, object message)
        {
            bool foundModel = _env.Instances.TryGetValue(modelName, out var instances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {modelName} not found. Register it first with the {nameof(SimulationWorkbench)} before sending telemetry to it.");

            //bool foundInstance = instances.TryGetValue(InstanceId, out var targetInstanceRegistration);

            var instanceRegistration = instances.GetOrAdd(InstanceId, key =>
            {
                // Create a new one:
                bool foundModelRegistration = _env.Models.TryGetValue(modelName, out var modelRegistration);
                if (!foundModel)
                    throw new KeyNotFoundException($"Model {modelName} not found when trying to create new instance.");
                if (modelRegistration.CreateNew == null)
                    throw new InvalidOperationException($"Model {modelName} is not able to create new instances.");

                DigitalTwinBase newInstance = modelRegistration.CreateNew();
                var registration = new InstanceRegistration(newInstance, modelRegistration, dataSource: this.InstanceRegistration);
                newInstance.InitInternal(InstanceId, modelName, new SimInitContext(registration, _env));
                return registration;
            });


            if (instanceRegistration.ModelRegistration.InvokeProcessMessages == null)
                throw new InvalidOperationException("Model is not configured to process messages.");


            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {modelName}\\{InstanceId}");

            var processingContext = new SimProcessingContext(instanceRegistration,
                                                             _env,
                                                             nextMessageDepth,
                                                             _logger);

            object[] messages = new object[] { message };
            instanceRegistration.ModelRegistration.InvokeProcessMessages(processingContext,
                                                                          instanceRegistration.DigitalTwinInstance,
                                                                          messages);

            return SendingResult.Handled;
        }

        /// <inheritdoc/>
        public override DateTimeOffset GetCurrentTime()
        {
            if (_env == null || _env.EventGenerator == null)
                return DateTimeOffset.UtcNow;
            else
                return _env.CurrentTime.ToUniversalTime();

        }

        /// <inheritdoc/>
        public TimeSpan GetSimulationTimeIncrement()
        {
            if (_env == null || _env.EventGenerator == null)
                throw new InvalidOperationException("Not running a simulation.");

            return _env.EventGenerator.SimulationIterationInterval;
        }

        /// <inheritdoc/>
        public DateTimeOffset SimulationStartTime
        {
            get
            {
                if (_env == null || _env.EventGenerator == null)
                    throw new InvalidOperationException("Not running a simulation.");

                return _env.EventGenerator.StartTime;
            }
        }

        public override void LogMessage(LogSeverity severity, string message)
        {
            _logger.Log(severity.ToLogLevel(), message);
        }

        public override SendingResult SendAlert(string providerName, AlertMessage alertMessage)
        {
            throw new NotImplementedException();
        }

        public override SendingResult SendToDataSource(byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return SendToDataSource(messages);
        }

        public override SendingResult SendToDataSource(object message)
        {
            object[] messages = new object[] { message };
            return SendToDataSource(messages);
        }

        public override SendingResult SendToDataSource(IEnumerable<byte[]> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            if (InstanceRegistration.DataSource == null)
                throw new InvalidOperationException($"Data source is not available in this context. (Instance {DigitalTwinModel}\\{InstanceId}).");

            ModelRegistration model = InstanceRegistration.DataSource.ModelRegistration;

            if (model.DeserializeMessage == null)
                throw new InvalidOperationException("Model was not configured to process messages.");

            List<object> deserializedMessages = new List<object>(messages.Count());
            foreach (var serializedMsg in messages)
            {
                object? deserializedMsg = model.DeserializeMessage(serializedMsg);
                if (deserializedMsg != null)
                    deserializedMessages.Add(deserializedMsg);
            }

            return SendToDataSource(deserializedMessages);
        }

        public override SendingResult SendToDataSource(IEnumerable<object> messages)
        {
            if (InstanceRegistration.DataSource == null)
                throw new InvalidOperationException($"Data source is not available in this context. (Instance {DigitalTwinModel}\\{InstanceId}).");

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            InstanceRegistration targetInstanceRegistration = InstanceRegistration.DataSource;

            if (targetInstanceRegistration.ModelRegistration.InvokeProcessMessages == null)
                throw new InvalidOperationException($"Model {targetInstanceRegistration.ModelRegistration.ModelName} is not configured to process messages.");

            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {targetInstanceRegistration.ModelRegistration.ModelName}\\{targetInstanceRegistration.DigitalTwinInstance.Id}.");

            var processingContext = new SimProcessingContext(targetInstanceRegistration,
                                                             _env,
                                                             nextMessageDepth,
                                                             _logger);

            targetInstanceRegistration.ModelRegistration.InvokeProcessMessages(processingContext,
                                                                         targetInstanceRegistration.DigitalTwinInstance,
                                                                         messages);

            return SendingResult.Handled;
        }

        public override SendingResult SendToTwin(string targetTwinModel, string targetTwinId, byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return SendToTwin(targetTwinModel, targetTwinId, messages);
        }

        public override SendingResult SendToTwin(string targetTwinModel, string targetTwinId, object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            object[] messages = new object[] { message };
            return SendToTwin(targetTwinModel, targetTwinId, messages);
        }

        public override SendingResult SendToTwin(string targetTwinModel, string targetTwinId, IEnumerable<byte[]> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            bool foundModel = _env.Models.TryGetValue(targetTwinModel, out var model);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {targetTwinModel} not found. Register it first with the {nameof(SimulationWorkbench)} before sending message to it.");

            if (model.DeserializeMessage == null)
                throw new InvalidOperationException("Model was not configured to process messages.");

            List<object> deserializedMessages = new List<object>(messages.Count());
            foreach (var serializedMsg in messages)
            {
                object? deserializedMsg = model.DeserializeMessage(serializedMsg);
                if (deserializedMsg != null)
                    deserializedMessages.Add(deserializedMsg);
            }

            return SendToTwin(targetTwinModel, targetTwinId, deserializedMessages);
        }

        public override SendingResult SendToTwin(string targetTwinModel, string targetTwinId, IEnumerable<object> messages)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            // Undocumented feature of the real ProcessingContextInternal class: If the targetTwinModel is null/empty,
            // assuming we're sending a message to another instance in the same model.
            if (string.IsNullOrEmpty(targetTwinModel))
                targetTwinModel = this.DigitalTwinModel;

            bool foundModel = _env.Instances.TryGetValue(targetTwinModel, out var instances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {targetTwinModel} not found. Register it first with the {nameof(SimulationWorkbench)} before sending message to it.");

            var instanceRegistration = instances.GetOrAdd(targetTwinId, key =>
            {
                // Create a new one:
                bool foundModelRegistration = _env.Models.TryGetValue(targetTwinModel, out var modelRegistration);
                if (!foundModel)
                    throw new KeyNotFoundException($"Model {targetTwinModel} not found when trying to create new instance.");
                if (modelRegistration.CreateNew == null)
                    throw new InvalidOperationException($"Model {targetTwinModel} is not able to create new instances.");

                DigitalTwinBase newInstance = modelRegistration.CreateNew();

                var registration = new InstanceRegistration(newInstance, modelRegistration, dataSource: null);
                newInstance.InitInternal(targetTwinId, targetTwinModel, new SimInitContext(registration, _env));
                return registration;
            });

            if (instanceRegistration.ModelRegistration.InvokeProcessMessages == null)
                throw new InvalidOperationException("Model was not configured to process messages.");


            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {targetTwinModel}\\{targetTwinId}");

            var processingContext = new SimProcessingContext(instanceRegistration,
                                                             _env,
                                                             nextMessageDepth,
                                                             _logger);

            instanceRegistration.ModelRegistration.InvokeProcessMessages(processingContext,
                                                                          instanceRegistration.DigitalTwinInstance,
                                                                          messages);

            return SendingResult.Handled;
        }

        public override TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerHandler timerCallback)
        {
            if (timerName == null) throw new ArgumentNullException(nameof(timerName));
            if (timerCallback == null) throw new ArgumentNullException(nameof(timerCallback));
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

            SimulationTimer timerRegistration = new SimulationTimer(InstanceRegistration,
                                                                    timerName,
                                                                    type,
                                                                    interval,
                                                                    timerCallback);
            bool added = _env.Timers.TryAdd(timerName, timerRegistration);
            if (added)
            {
                if (_env.EventGenerator == null)
                    return TimerActionResult.FailedInternalError;

                _env.EventGenerator.EnqueueEvent(timerRegistration, _env.CurrentTime + timerRegistration.Interval);
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
                timerReg.IsDeleted = true;
                _env.Timers.Remove(timerName);
                return TimerActionResult.Success;
            }
        }

        public void StopSimulation()
        {
            StopRequested = true;
        }
    }
}

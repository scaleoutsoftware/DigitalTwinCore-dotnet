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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimProcessingContext<TDigitalTwin> : ProcessingContext<TDigitalTwin>, ISimulationController
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>, new()
    {
        private SimulationWorkbench _env;
        private ILogger _logger;

        internal string InstanceId { get; }

        const int MAX_MESSAGE_DEPTH = 100;
        private int _messageDepth;

        internal SimProcessingContext(InstanceRegistration<TDigitalTwin> instanceRegistration,
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


        public override string DigitalTwinModel { get; }

        internal TimeSpan RequestedSimulationCycleDelay { get; set; } = TimeSpan.Zero;

        public bool StopRequested { get; set; } = false;

        public bool DeleteRequested { get; set; } = false;

        public override IAzureDigitalTwinsProvider AzureDigitalTwinsProvider => throw new NotImplementedException();

        public override Dictionary<string, IAnomalyDetectionProvider> AnomalyDetectionProviders => throw new NotImplementedException();


        public override ISimulationController SimulationController => this as ISimulationController;

        public override ISharedData SharedModelData => InstanceRegistration.ModelRegistration.SharedModelData;

        public override ISharedData SharedGlobalData => _env.SharedGlobalData;

        public InstanceRegistration<TDigitalTwin> InstanceRegistration { get; }

        public Task CreateTwinAsync(string modelName, string twinId, object newInstance)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("modelName cannot be null or whitespace");
            if (string.IsNullOrWhiteSpace(twinId))
                throw new ArgumentException("twinId cannot be null or whitespace");
            if (newInstance == null)
                throw new ArgumentNullException(nameof(newInstance));

            TDigitalTwin? dtInstance = newInstance as TDigitalTwin;
            if (dtInstance == null)
                throw new ArgumentException("newInstance must be a model that derives from DigitalTwinBase");

            _env.AddInstance<TDigitalTwin>(modelName, twinId, dtInstance);

            ModelRegistration modelRegistration = _env.Models[modelName];
            if (modelRegistration.SimulationProcessor != null)
            {
                // TODO: enqueue for immediate processing of this time step
                throw new NotImplementedException();
            }
            return Task.CompletedTask;
        }

        public Task CreateTwinFromPersistenceStoreAsync(string modelName, string twinId, object defaultInstance)
        {
            throw new NotSupportedException();
        }

        public Task CreateTwinFromPersistenceStoreAsync(string modelName, string twinId)
        {
            throw new NotSupportedException();
        }

        public void Delay(TimeSpan delay)
        {
            this.RequestedSimulationCycleDelay = delay;
        }

        public void DelayIndefinitely()
        {
            this.RequestedSimulationCycleDelay = TimeSpan.MaxValue;
        }

        public Task DeleteThisTwinAsync()
        {
            DeleteRequested = true; // prevents simulation instance from being re-enqueued in the scheduler.

            _env.RemoveInstance(this.DigitalTwinModel, this.InstanceId);
            return Task.CompletedTask;
        }

        public void RunThisTwin()
        {
            _env.EnqueueImmediate(this.InstanceRegistration);
        }

        public Task DeleteTwinAsync(string modelName, string twinId)
        {
            _env.RemoveInstance(modelName, twinId);
            return Task.CompletedTask;
        }


        public async Task EmitTelemetryAsync(string modelName, byte[] message)
        {
            bool foundModel = _env.Instances.TryGetValue(modelName, out var targetModelInstances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {modelName} not found. Register it first with the {nameof(SimulationWorkbench)} before sending telemetry to it.");

            var targetInstanceRegistration = targetModelInstances.GetOrAdd(InstanceId, key =>
            {
                // Create a new one:
                bool foundModelRegistration = _env.Models.TryGetValue(modelName, out var modelRegistration);
                if (!foundModel)
                    throw new KeyNotFoundException($"Model {modelName} not found when trying to create new instance.");
                
                return modelRegistration.CreateNewInitializedInstanceAsync(InstanceId, dataSource: this.InstanceRegistration, _logger).GetAwaiter().GetResult();
            });

            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {modelName}\\{InstanceId}");

            byte[][] messages = new byte[][] { message };
            foreach (var msg in messages)
            {
                await targetInstanceRegistration.ModelRegistration.ProcessMessageAsync(targetInstanceRegistration,
                                                                                       msg,
                                                                                       nextMessageDepth,
                                                                                       _logger);
            }

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

        public override Task LogMessageAsync(LogSeverity severity, string message)
        {
            _logger.Log(severity.ToLogLevel(), message);
            return Task.CompletedTask;
        }

        public override Task SendAlertAsync(string providerName, AlertMessage alertMessage)
        {
            throw new NotImplementedException();
        }

        public override Task SendAlertAsync(AlertMessage alertMessage)
        {
            throw new NotImplementedException();
        }

        public override Task SendToDataSourceAsync(byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return SendToDataSourceAsync(messages);
        }
        public async override Task SendToDataSourceAsync(IEnumerable<byte[]> messages)
        {
            if (InstanceRegistration.DataSource == null)
                throw new InvalidOperationException($"Data source is not available in this context. (Instance {DigitalTwinModel}\\{InstanceId}).");

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            InstanceRegistration targetInstanceRegistration = InstanceRegistration.DataSource;

            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {targetInstanceRegistration.ModelRegistration.ModelName}\\{targetInstanceRegistration.InstanceId}.");

            foreach (var msg in messages)
            {
                await targetInstanceRegistration.ModelRegistration.ProcessMessageAsync(targetInstanceRegistration,
                                                                                       msg,
                                                                                       nextMessageDepth,
                                                                                       _logger);
            }
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
            // assuming we're sending a message to another instance in the same model.
            if (string.IsNullOrEmpty(targetTwinModel))
                targetTwinModel = this.DigitalTwinModel;

            bool foundModel = _env.Instances.TryGetValue(targetTwinModel, out var instances);
            if (!foundModel)
                throw new KeyNotFoundException($"Model {targetTwinModel} not found. Register it first with the {nameof(SimulationWorkbench)} before sending message to it.");

            var targetInstanceRegistration = instances.GetOrAdd(targetTwinId, key =>
            {
                // Create a new one:
                bool foundModelRegistration = _env.Models.TryGetValue(targetTwinModel, out var targetModelRegistration);
                if (!foundModel)
                    throw new KeyNotFoundException($"Model {targetTwinModel} not found when trying to create new instance.");

                var registration = targetModelRegistration.CreateNewInitializedInstanceAsync(InstanceId, 
                                                                                             dataSource: this.InstanceRegistration, 
                                                                                             _logger)
                                                                                             .GetAwaiter().GetResult();
                return registration;
            });

            int nextMessageDepth = _messageDepth + 1;
            if (nextMessageDepth == MAX_MESSAGE_DEPTH)
                throw new InvalidOperationException($"Max message depth of {MAX_MESSAGE_DEPTH} has been hit. Sending from {this.DigitalTwinModel}\\{InstanceId} to {targetTwinModel}\\{targetTwinId}");

            foreach (var msg in messages)
            {
                await targetInstanceRegistration.ModelRegistration.ProcessMessageAsync(targetInstanceRegistration,
                                                                                       msg,
                                                                                       nextMessageDepth,
                                                                                       _logger);
            }

        }

        public override Task<TimerActionResult> StartTimerAsync(string timerName, TimeSpan interval, TimerType type, TimerAsyncHandler<TDigitalTwin> timerCallback)
        {
            if (timerName == null) throw new ArgumentNullException(nameof(timerName));
            if (timerCallback == null) throw new ArgumentNullException(nameof(timerCallback));
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

            SimulationTimer<TDigitalTwin> timerRegistration = new SimulationTimer<TDigitalTwin>(InstanceRegistration,
                                                                    timerName,
                                                                    type,
                                                                    interval,
                                                                    timerCallback);
            bool added = _env.Timers.TryAdd(timerName, timerRegistration);
            if (added)
            {
                if (_env.EventGenerator == null)
                    return Task.FromResult(TimerActionResult.FailedInternalError);

                _env.EventGenerator.EnqueueEvent(timerRegistration, _env.CurrentTime + timerRegistration.Interval);
                return Task.FromResult(TimerActionResult.Success);
            }
            else
            {
                return Task.FromResult(TimerActionResult.FailedTimerAlreadyExists);
            }
        }

        public override Task<TimerActionResult> StopTimerAsync(string timerName)
        {
            bool found = _env.Timers.TryGetValue(timerName, out var timerReg);
            if (!found)
                return Task.FromResult(TimerActionResult.FailedNoSuchTimer);
            else
            {
                timerReg.IsDeleted = true;
                _env.Timers.Remove(timerName);
                return Task.FromResult(TimerActionResult.Success);
            }
        }

        public void StopSimulation()
        {
            StopRequested = true;
        }

        public override Task RemoveRealTimeTwinAsync(string targetTwinModel, string targetTwinId)
        {
            // Undocumented feature of the real ProcessingContextInternal class: If the targetTwinModel is null/empty,
            // assuming we're sending a message to another instance in the same model.
            if (string.IsNullOrEmpty(targetTwinModel))
                targetTwinModel = this.DigitalTwinModel;

            _env.RemoveInstance(targetTwinModel, targetTwinId);
            return Task.CompletedTask;
        }

        
        
    }
}

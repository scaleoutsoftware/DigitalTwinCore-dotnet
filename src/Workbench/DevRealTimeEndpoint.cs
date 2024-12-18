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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class DevRealTimeEndpoint : IDigitalTwinModelEndpoint
    {
        private readonly RealTimeWorkbench _workbench;
        private readonly ModelRegistration _registration;
        private readonly ConcurrentDictionary<string, InstanceRegistration> _modelInstances;
        private readonly ILogger _logger;

        public DevRealTimeEndpoint(RealTimeWorkbench workbench, 
                                   ModelRegistration modelRegistration,
                                   ConcurrentDictionary<string, InstanceRegistration> modelInstances,
                                   ILogger logger)
        {
            _workbench = workbench; 
            _registration = modelRegistration;
            _modelInstances = modelInstances;
            _logger = logger;
        }

        public SendingResult CreateTwin(string digitalTwinId, object digitalTwin)
        {
            if (digitalTwin == null)
                throw new ArgumentNullException(nameof(digitalTwin));
            
            DigitalTwinBase? newInstance = digitalTwin as DigitalTwinBase;
            if (newInstance == null)
                throw new ArgumentException("digitalTwin must inherit from DigitalTwinBase");

            var instanceRegistration = new InstanceRegistration(newInstance, _registration, dataSource: null);

            var initContext = new RealTimeInitContext(instanceRegistration, _workbench, _logger);
            newInstance.InitInternal(digitalTwinId, _registration.ModelName, initContext);
            bool added = _modelInstances.TryAdd(digitalTwinId, instanceRegistration);
            if (added)
                _logger.LogInformation("Digital twin instance {DigitalTwinId} created for model {ModelName}", digitalTwinId, _registration.ModelName);
            else
                _logger.LogWarning("Digital twin instance {DigitalTwinId} could not be created for model {ModelName} because an instance with this ID already exists.", digitalTwinId, _registration.ModelName);

            return SendingResult.Handled;
        }

        public SendingResult CreateTwinFromPersistenceStore(string digitalTwinId, object defaultValue)
        {
            throw new NotSupportedException();
        }

        public SendingResult CreateTwinFromPersistenceStore(string digitalTwinId)
        {
            throw new NotSupportedException();
        }

        public SendingResult DeleteTwin(string digitalTwinId)
        {
            bool foundInstance = _modelInstances.TryRemove(digitalTwinId, out _);
            if (foundInstance)
                _logger.LogInformation("Digital twin instance {DigitalTwinId} removed for model {ModelName}", digitalTwinId, _registration.ModelName);
            else
                _logger.LogWarning("Digital twin instance {DigitalTwinId} could not be removed for model {ModelName} because it does not exist.", digitalTwinId, _registration.ModelName);

            return SendingResult.Handled;

        }

        public SendingResult Send(string digitalTwinId, IEnumerable<byte[]> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            if (_registration.DeserializeMessage == null)
                throw new InvalidOperationException("Model was not configured to process messages.");

            List<object> deserializedMessages = new List<object>(messages.Count());
            foreach (var serializedMsg in messages)
            {
                object? deserializedMsg = _registration.DeserializeMessage(serializedMsg);
                if (deserializedMsg != null)
                    deserializedMessages.Add(deserializedMsg);
            }

            return Send(digitalTwinId, deserializedMessages);
        }

        public SendingResult Send(string digitalTwinId, byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return Send(digitalTwinId, messages);
        }

        public SendingResult Send(string digitalTwinId, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            return Send(digitalTwinId, messageBytes);
        }

        public SendingResult Send(string digitalTwinId, object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            object[] messages = new object[] { message };
            return Send(digitalTwinId, messages);
        }

        public SendingResult Send(string digitalTwinId, IEnumerable<object> messages)
        {
            if (digitalTwinId == null) throw new ArgumentNullException(nameof(digitalTwinId));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var instanceRegistration = _modelInstances.GetOrAdd(digitalTwinId, key =>
            {
                // Create a new one:
                if (_registration.CreateNew == null)
                    throw new InvalidOperationException($"Model {_registration.ModelName} is not able to create new instances.");

                DigitalTwinBase newInstance = _registration.CreateNew();
                var registration = new InstanceRegistration(newInstance, _registration, dataSource: null);

                var initContext = new RealTimeInitContext(registration, _workbench, _logger);
                newInstance.InitInternal(digitalTwinId, _registration.ModelName, initContext);
                return registration;
            });

            if (_registration.InvokeProcessMessages == null)
                throw new InvalidOperationException("Model was not configured to process messages.");



            var processingContext = new RealTimeProcessingContext(null, // no data source when just sending from an endpoint.
                                                                 instanceRegistration,
                                                                 _workbench,
                                                                 0,
                                                                 _logger);

            _registration.InvokeProcessMessages(processingContext,
                                                instanceRegistration.DigitalTwinInstance,
                                                messages);

            return SendingResult.Handled;
        }

        public ISharedData SharedModelData => _registration.SharedModelData;

        public ISharedData SharedGlobalData => _workbench.SharedGlobalData;
    }
}

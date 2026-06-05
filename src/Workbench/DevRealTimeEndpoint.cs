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
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class DevRealTimeEndpoint<TDigitalTwin> : IDigitalTwinModelEndpoint
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>, new()
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

        public async Task<CreateResult> CreateInstanceAsync(string digitalTwinId, object digitalTwin)
        {
            if (digitalTwin == null)
                throw new ArgumentNullException(nameof(digitalTwin));
            
            TDigitalTwin? typedDigitalTwin = digitalTwin as TDigitalTwin;
            if (typedDigitalTwin == null)
                throw new ArgumentException($"digitalTwin must be a {typeof(TDigitalTwin)}");

            var instanceRegistration = new InstanceRegistration<TDigitalTwin>(digitalTwinId, typedDigitalTwin, _registration, dataSource: null);

            InitContext<TDigitalTwin> initContext = new RealTimeInitContext<TDigitalTwin>(instanceRegistration, _workbench, _logger);
            await typedDigitalTwin.InitInternalAsync(digitalTwinId, _registration.ModelName, initContext);
            bool added = _modelInstances.TryAdd(digitalTwinId, instanceRegistration);
            if (added)
            {
                return CreateResult.Success;
            }
            else
            {
                return CreateResult.AlreadyExists;
            }
        }

        public Task<DeleteResult> DeleteInstanceAsync(string digitalTwinId)
        {
            bool foundInstance = _modelInstances.TryRemove(digitalTwinId, out _);
            if (foundInstance)
            {
                return Task.FromResult(DeleteResult.Success);
            }
            else
            {
                return Task.FromResult(DeleteResult.NotFound);
            }
        }

        public Task SendAsync(string digitalTwinId, byte[] message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            byte[][] messages = new byte[][] { message };
            return SendAsync(digitalTwinId, messages);
        }


        public async Task SendAsync(string digitalTwinId, IEnumerable<byte[]> messages)
        {
            if (digitalTwinId == null) throw new ArgumentNullException(nameof(digitalTwinId));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var instanceRegistration = _modelInstances.GetOrAdd(digitalTwinId, key =>
            {
                return _registration.CreateNewInitializedInstanceAsync(digitalTwinId, null, _logger).GetAwaiter().GetResult();
            });

            foreach (var message in messages)
            {
                await instanceRegistration.ModelRegistration.ProcessMessageAsync(instanceRegistration,
                                                                                 message,
                                                                                 0,
                                                                                 _logger);
            }

        }

        public ISharedData SharedModelData => _registration.SharedModelData;

        public ISharedData SharedGlobalData => _workbench.SharedGlobalData;
    }
}

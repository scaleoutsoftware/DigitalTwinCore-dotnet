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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    /// <summary>
    /// Workbench for real-time digital twin development.
    /// </summary>
    public class RealTimeWorkbench : IDisposable
    {
        private ConcurrentDictionary<string, ModelRegistration> _models = new ConcurrentDictionary<string, ModelRegistration>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, InstanceRegistration>> _instances = new ConcurrentDictionary<string, ConcurrentDictionary<string, InstanceRegistration>>();
        private WorkbenchSharedData _sharedGlobalData = new WorkbenchSharedData();
        internal List<PostedAlert> _postedAlerts = new List<PostedAlert>();
        private ConcurrentDictionary<string, RealTimeTimer> _timers = new ConcurrentDictionary<string, RealTimeTimer>();
        private ILogger _logger;
        private bool _disposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">ILogger instance, or null.</param>
        public RealTimeWorkbench(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        internal IDictionary<string, ConcurrentDictionary<string, InstanceRegistration>> Instances
        {
            get => _instances;
        }

        internal IDictionary<string, ModelRegistration> Models
        {
            get => _models;
        }

        internal IDictionary<string, RealTimeTimer> Timers
        {
            get => _timers;
        }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared globally between all models in this workbench instance.
        /// </summary>
        public ISharedData SharedGlobalData
        {
            get => _sharedGlobalData;
        }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared between the objects in the specified model.
        /// </summary>
        /// <param name="modelName">Name of the model associated with the shared data.</param>
        /// <returns>An <see cref="ISharedData"/> instance that can be used to access shared objects.</returns>
        /// <exception cref="InvalidOperationException">The specified model has not been registered with the workbench.</exception>
        public ISharedData GetSharedModelData(string modelName)
        {
            bool foundModel = _models.TryGetValue(modelName, out var registration);
            if (!foundModel)
                throw new InvalidOperationException($"Model {modelName} has not been registered.");

            return registration.SharedModelData;
        }

        /// <summary>
        /// Adds a real-time model (with a message processor) to the workbench. The
        /// returned endpoint can be used to send messages to instances in the model.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of digital twin model (derived from <see cref="DigitalTwinBase"/>).</typeparam>
        /// <typeparam name="TMessage">Type of message sent to the model's message processor.</typeparam>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="processor">The model's <see cref="MessageProcessor{TDigitalTwin, TMessage}"/> implementation.</param>
        /// <returns><see cref="IDigitalTwinModelEndpoint"/> that can be used to send messages to an instance of the model.</returns>
        /// <exception cref="ArgumentNullException">The provided message processor was null.</exception>
        /// <exception cref="ArgumentException">The model name is invalid (null or whitespace).</exception>
        /// <exception cref="ArgumentException">A model with the same name already exists in this workbench.</exception>
        public IDigitalTwinModelEndpoint AddRealTimeModel<TDigitalTwin, TMessage>(string modelName, MessageProcessor<TDigitalTwin, TMessage> processor)
            where TDigitalTwin : DigitalTwinBase, new()
        {
            if (_disposed)
                throw new InvalidOperationException($"{nameof(RealTimeWorkbench)} has been disposed");

            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Invalid model name", nameof(modelName));

            if (_models.ContainsKey(modelName))
                throw new ArgumentException($"A model named {modelName} already exists.");

            ModelRegistration registration = new ModelRegistration(modelName, sharedModelData: new WorkbenchSharedData());
            registration.MessageProcessor = processor;

            // We have all the nice TDigitalTwin and TMessage type information right now.
            // Capture this type info in lambdas that do casting. We can use these lambdas later
            // during a simulation run (when we won't have the type information).
            registration.InvokeProcessMessages = (processingContext, twinInstance, messages) =>
            {
                if (twinInstance == null) throw new ArgumentNullException(nameof(twinInstance));

                TDigitalTwin? typedTwin = twinInstance as TDigitalTwin;
                if (typedTwin == null)
                    throw new ArgumentException($"Real time processor for {modelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {twinInstance.GetType()}");

                IEnumerable<TMessage> typedMessages = messages.Cast<TMessage>();

                return processor.ProcessMessages(processingContext, typedTwin, typedMessages);
            };

            registration.DeserializeMessage = (serializedMessage) =>
            {
                string msgJson = Encoding.UTF8.GetString(serializedMessage);
                return JsonConvert.DeserializeObject<TMessage>(msgJson);
            };

            registration.CreateNew = () =>
            {
                return new TDigitalTwin();
            };


            bool added = _models.TryAdd(modelName, registration);
            if (!added)
            {
                throw new ArgumentException($"A model named {modelName} already exists.");
            }

            var modelInstances = new ConcurrentDictionary<string, InstanceRegistration>();
            added = _instances.TryAdd(modelName, modelInstances);
            if (!added)
            {
                throw new InvalidOperationException($"Intances under model {modelName} already exist.");
            }

            return new DevRealTimeEndpoint(this, registration, modelInstances, _logger);
        }

        /// <summary>
        /// Gets a list of alerts that were issued from message processors.
        /// </summary>
        public IReadOnlyList<PostedAlert> PostedAlerts
        {
            get
            {
                // Return a copy of the collection since other threads
                // could be modifying it from a RealTimeProcessingContext.
                lock (_postedAlerts)
                {
                    List<PostedAlert> copy = new List<PostedAlert>(_postedAlerts);
                    return copy;
                }
            }
        }

        internal void RecordAlert(string providerName, AlertMessage alertMessage)
        {
            PostedAlert postedAlert = new PostedAlert(providerName, alertMessage);
            lock (_postedAlerts)
            {
                _postedAlerts.Add(postedAlert);
            }
        }

        /// <summary>
        /// Gets a read-only dictionary of all digital twin instances in a model.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of digital twin instance stored in the simulation workbench.</typeparam>
        /// <param name="modelName">Name of the model associated with the instances.</param>
        /// <returns>A read-only dictionary of digital twin instances, keyed by ID.</returns>
        /// <exception cref="InvalidOperationException">The specified model was not registered.</exception>
        public IReadOnlyDictionary<string, TDigitalTwin> GetInstances<TDigitalTwin>(string modelName)
            where TDigitalTwin : DigitalTwinBase
        {
            bool foundModel = _instances.TryGetValue(modelName, out var instances);
            if (!foundModel)
                throw new InvalidOperationException($"Model {modelName} has not been registered.");

            return new InstanceDictionary<TDigitalTwin>(instances);
        }

        /// <summary>
        /// When overridden in a derived class, releases the unmanaged resources used by the environment,
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// Indicates whether the method call comes from a Dispose method (its value is true)
        /// or from a finalizer (its value is false).
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    List<Task> stopTasks = new List<Task>();
                    foreach (var timer in Timers.Values)
                    {
                        stopTasks.Add(timer.Stop());
                    }
                    Task.WhenAll(stopTasks).GetAwaiter().GetResult();
                }

                _disposed = true;
            }
        }

        
        /// <summary>
        /// Frees resources associated with the workbench environment and stops any timers 
        /// associated with twins in the environmwent.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

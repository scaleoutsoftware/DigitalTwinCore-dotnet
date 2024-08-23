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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Scaleout.Streaming.DigitalTwin.Core;

namespace Scaleout.DigitalTwin.Workbench
{
    enum SimulationState
    {
        /// <summary>
        /// SimulationWorkbench has been instantiated but is not yet running a sim.
        /// Models and instances can be added.
        /// </summary>
        Initializing,

        /// <summary>
        /// Simulation is running.
        /// </summary>
        Running,

        /// <summary>
        /// Simulation run has completed.
        /// </summary>
        Completed
    }

    /// <summary>
    /// Workbench for simulation development.
    /// </summary>
    public class SimulationWorkbench
    {
        private ConcurrentDictionary<string, ModelRegistration> _models = new ConcurrentDictionary<string, ModelRegistration>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, InstanceRegistration>> _instances = new ConcurrentDictionary<string, ConcurrentDictionary<string, InstanceRegistration>>();
        private ConcurrentDictionary<string, SimulationTimer> _timers = new ConcurrentDictionary<string, SimulationTimer>();
        private WorkbenchSharedData _sharedGlobalData = new WorkbenchSharedData();
        private ILogger _logger;
        private SimulationState _simulationState = SimulationState.Initializing;

        internal EventGenerator? EventGenerator {get; private set;}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">ILogger instance.</param>
        public SimulationWorkbench(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        internal IDictionary<string, ModelRegistration> Models
        {
            get => _models;
        }

        internal IDictionary<string, ConcurrentDictionary<string, InstanceRegistration>> Instances
        {
            get => _instances;
        }

        internal IDictionary<string, SimulationTimer> Timers 
        { 
            get => _timers; 
        }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared globally between all models in the simulation.
        /// </summary>
        public ISharedData SharedGlobalData 
        {
            get => _sharedGlobalData;
        }


        /// <summary>
        /// Creates a console logger for use in the SimulationWorkbench constructor.
        /// </summary>
        /// <returns>ILogger instance that writes to the console.</returns>
        public static ILogger CreateConsoleLogger()
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            return factory.CreateLogger<SimulationWorkbench>();
        }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared between the instances of the specified model.
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
        /// Gets the CurrentTime of a running simulation.
        /// </summary>
        /// <exception cref="InvalidOperationException">A simulation is not running, or a simulation has been initialized but has not yet stepped into a time step.</exception>
        public DateTimeOffset CurrentTime
        {
            get
            {
                if (EventGenerator == null)
                    throw new InvalidOperationException($"Simulation is not running. Call {nameof(RunSimulation)} or {nameof(InitializeSimulation)} to start a simulation.");

                return EventGenerator.SimulationTime; // might throw InvalidOperationException if caller hasn't called Step() yet to enter a timestep.
            }
        }

        /// <summary>
        /// Gets next time step in the current simulation.
        /// </summary>
        /// <returns>Time of the next simulation step, or DateTimeOffset.MaxValue if the simulation is about to end because there is no remaining work.</returns>
        /// <exception cref="InvalidOperationException">A simulation is not running, or a simulation has been initialized but has not yet stepped into a time step</exception>
        public DateTimeOffset PeekNextTimeStep()
        {
            if (EventGenerator == null)
                throw new InvalidOperationException($"Simulation is not running. Call {nameof(RunSimulation)} or {nameof(InitializeSimulation)} to start a simulation.");

            return EventGenerator.PeekNextTime(); // might throw InvalidOperationException if caller hasn't called Step() yet to enter a timestep.
        }

        /// <summary>
        /// Gets a specified instance.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of instance to retrieve.</typeparam>
        /// <param name="modelName">Name of the model associated with the instance.</param>
        /// <param name="instanceId">ID of the instance.</param>
        /// <returns>Digital twin instance, or null if the instance was not found</returns>
        /// <exception cref="InvalidOperationException">The specified model was not registered.</exception>
        public TDigitalTwin? GetInstance<TDigitalTwin>(string modelName, string instanceId) 
            where TDigitalTwin : DigitalTwinBase 
        {
            bool foundModel = _instances.TryGetValue(modelName, out var instances);
            if (!foundModel)
                throw new InvalidOperationException($"Model {modelName} has not been registered.");

            bool foundInstance = instances.TryGetValue(instanceId, out var instanceRegistration);
            if (!foundInstance)
                return null;

            if (instanceRegistration.IsDeleted)
                return null;

            return (TDigitalTwin)instanceRegistration.DigitalTwinInstance;
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
        /// Adds a real-time model (with a message processor) to the workbench.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of digital twin model (derived from <see cref="DigitalTwinBase"/>).</typeparam>
        /// <typeparam name="TMessage">Type of message sent to the model's message processor.</typeparam>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="processor">The model's <see cref="MessageProcessor{TDigitalTwin, TMessage}"/> implementation.</param>
        /// <exception cref="ArgumentNullException">The provided message processor was null.</exception>
        /// <exception cref="ArgumentException">The model name is invalid (null or whitespace).</exception>
        /// <exception cref="ArgumentException">A model with the same name already exists in this workbench.</exception>
        /// <exception cref="InvalidOperationException">A model cannot be added after a simulation has been started (using InitializeSimulation() or RunSimulation()).</exception>
        public void AddRealTimeModel<TDigitalTwin, TMessage>(string modelName, MessageProcessor<TDigitalTwin, TMessage> processor)
            where TDigitalTwin : DigitalTwinBase, new()
        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            AddModel(modelName,
                     simProcessor: null,
                     realTimeProcessor: processor);
        }

        /// <summary>
        /// Adds a simulation model (with a simulation processor) to the workbench.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of digital twin model (derived from <see cref="DigitalTwinBase"/>).</typeparam>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="processor">The model's <see cref="SimulationProcessor{TDigitalTwin}"/> implementation.</param>
        /// <exception cref="ArgumentNullException">The provided simulation processor was null.</exception>
        /// <exception cref="ArgumentException">The model name is invalid (null or whitespace).</exception>
        /// <exception cref="ArgumentException">A model with the same name already exists in this workbench.</exception>
        /// <exception cref="InvalidOperationException">A model cannot be added after a simulation has been started (using InitializeSimulation() or RunSimulation()).</exception>
        public void AddSimulationModel<TDigitalTwin>(string modelName, SimulationProcessor<TDigitalTwin> processor)
            where TDigitalTwin : DigitalTwinBase, new()

        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));

            AddModel<TDigitalTwin, object>(modelName,
                                           simProcessor: processor,
                                           realTimeProcessor: null);

        }

        /// <summary>
        /// Adds a simulation model (with both a simulation processor and a message processor) to the workbench.
        /// </summary>
        /// <typeparam name="TDigitalTwin">Type of digital twin model (derived from <see cref="DigitalTwinBase"/>).</typeparam>
        /// <typeparam name="TMessage">Type of message sent to the model's message processor.</typeparam>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="simulationProcessor">The model's <see cref="SimulationProcessor{TDigitalTwin}"/> implementation.</param>
        /// <param name="messageProcessor">The model's <see cref="MessageProcessor{TDigitalTwin, TMessage}"/> implementation.</param>
        /// <exception cref="ArgumentNullException">The provided simulation processor or message processor was null.</exception>
        /// <exception cref="ArgumentException">The model name is invalid (null or whitespace).</exception>
        /// <exception cref="ArgumentException">A model with the same name already exists in this workbench.</exception>
        /// <exception cref="InvalidOperationException">A model cannot be added after a simulation has been started (using InitializeSimulation() or RunSimulation()).</exception>
        public void AddSimulationModel<TDigitalTwin, TMessage>(string modelName, 
                                                               SimulationProcessor<TDigitalTwin> simulationProcessor,
                                                               MessageProcessor<TDigitalTwin, TMessage> messageProcessor)
            where TDigitalTwin : DigitalTwinBase, new()
        {
            if (simulationProcessor == null)
                throw new ArgumentNullException(nameof(simulationProcessor));
            if (messageProcessor == null)
                throw new ArgumentNullException(nameof(messageProcessor));

            AddModel(modelName,
                     simulationProcessor,
                     messageProcessor);

        }

        private void AddModel<TDigitalTwin, TMessage>(string modelName, SimulationProcessor<TDigitalTwin>? simProcessor, MessageProcessor<TDigitalTwin, TMessage>? realTimeProcessor)
            where TDigitalTwin : DigitalTwinBase, new()
        {
            if (_simulationState > SimulationState.Initializing)
                throw new InvalidOperationException($"Cannot add model after starting simulation with {nameof(InitializeSimulation)} or {nameof(RunSimulation)}.");

            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentException("Invalid model name", nameof(modelName));

            if (_models.ContainsKey(modelName))
                throw new ArgumentException($"A model named {modelName} already exists.");

            ModelRegistration registration = new ModelRegistration(modelName, sharedModelData: new WorkbenchSharedData());

            if (realTimeProcessor != null)
            {
                registration.MessageProcessor = realTimeProcessor;

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

                    return realTimeProcessor.ProcessMessages(processingContext, typedTwin, typedMessages);
                };

                registration.DeserializeMessage = (serializedMessage) =>
                {
                    string msgJson = Encoding.UTF8.GetString(serializedMessage);
                    return JsonConvert.DeserializeObject<TMessage>(msgJson);
                };
            }

            if (simProcessor != null)
            {
                registration.SimulationProcessor = simProcessor;

                // We have the nice TDigitalTwin type information now, so capture it in a lambda
                // that does casting and makes the ProcessModel call. We can call this lambda later
                // during a simulation run (when we won't have the instanceRegistration's type information).
                registration.InvokeProcessModel = (processingContext, twinInstance, simTime) =>
                {
                    if (twinInstance == null) throw new ArgumentNullException(nameof(twinInstance));

                    TDigitalTwin? typedTwin = twinInstance as TDigitalTwin;
                    if (typedTwin == null)
                        throw new ArgumentException($"Simulation processor for {modelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {twinInstance.GetType()}");

                    return simProcessor.ProcessModel(processingContext, typedTwin, simTime);
                };
            }

            registration.CreateNew = () =>
            {
                return new TDigitalTwin();
            };


            bool added = _models.TryAdd(modelName, registration);
            if (!added)
            {
                throw new ArgumentException($"A model named {modelName} already exists.");
            }

            added = _instances.TryAdd(modelName, new ConcurrentDictionary<string, InstanceRegistration>());
            if (!added)
            {
                throw new InvalidOperationException($"Intances under model {modelName} already exist.");
            }
        }

        /// <summary>
        /// Adds a digital twin instance to a model that has been registered with this workbench.
        /// </summary>
        /// <param name="instanceId">ID of the instance being added.</param>
        /// <param name="modelName">Name of a model that has been registered with this workbench instance using <see cref="AddSimulationModel{TDigitalTwin}(string, SimulationProcessor{TDigitalTwin})"/> or <see cref="AddRealTimeModel{TDigitalTwin, TMessage}(string, MessageProcessor{TDigitalTwin, TMessage})"/>.</param>
        /// <param name="instance">The digital twin instance.</param>
        /// <exception cref="InvalidOperationException">The provided <paramref name="modelName"/> has not been registered as a simulation or realtime model.</exception>
        /// <exception cref="InvalidOperationException">The instance cannot be added because the simulation was already started (using InitializeSimulation() or RunSimulation()).</exception>
        public void AddInstance(string instanceId, string modelName, DigitalTwinBase instance)
        {
            if (_simulationState > SimulationState.Initializing)
                throw new InvalidOperationException($"Cannot add instances after starting simulation with {nameof(InitializeSimulation)} or {nameof(RunSimulation)}.");

            bool foundModel = _models.TryGetValue(modelName, out var modelRegistration);

            if (!foundModel )
                throw new InvalidOperationException($"{modelName} has not been registered as a simulation or realtime model. Call AddSimulationModel or AddRealTimeModel first.");

            InstanceRegistration instanceRegistration = new InstanceRegistration(instance, modelRegistration);

            SimInitContext initContext = new SimInitContext(instanceRegistration, this);
            instance.InitInternal(instanceId, modelName, initContext);

            bool foundInstances = _instances.TryGetValue(modelName, out var modelInstances);
            if (!foundInstances)
            {
                throw new InvalidOperationException($"Instances for {modelName} not found."); //shouldn't ever happen.
            }

            modelInstances[instanceId] = instanceRegistration;

        }

        /// <summary>
        /// Removes a digital twin instance.
        /// </summary>
        /// <param name="instanceId">ID of the instance being added.</param>
        /// <param name="modelName">Name of a model that has been registered with this workbench instance using <see cref="AddSimulationModel{TDigitalTwin}(string, SimulationProcessor{TDigitalTwin})"/> or <see cref="AddRealTimeModel{TDigitalTwin, TMessage}(string, MessageProcessor{TDigitalTwin, TMessage})"/>.</param>
        /// <exception cref="InvalidOperationException">The provided <paramref name="modelName"/> has not been registered as a simulation or realtime model.</exception>
        /// <exception cref="InvalidOperationException">The instance cannot be removed because the simulation was already started (using InitializeSimulation() or RunSimulation()).</exception>
        internal void RemoveInstance(string modelName, string instanceId)
        {
            bool foundModel = _instances.TryGetValue(modelName, out var modelInstances);
            if (foundModel)
            {
                bool foundInstance = modelInstances.TryRemove(instanceId, out var removedInstance);
                if (foundInstance)
                {
                    removedInstance.IsDeleted = true;
                }
            }
        }


        /// <summary>
        /// Runs a simulation to completion.
        /// </summary>
        /// <param name="startTime">Simulation start time (inclusive).</param>
        /// <param name="endTime">Simulation end time (exclusive).</param>
        /// <param name="simulationIterationInterval">Simulated time between steps in the simulation.</param>
        /// <param name="delayBetweenTimesteps">Sleep time between simulation time steps.</param>
        /// <returns><see cref="StepResult"/> containing the final status of the completed simulation.</returns>
        public StepResult RunSimulation(DateTimeOffset startTime, DateTimeOffset endTime, TimeSpan simulationIterationInterval, TimeSpan delayBetweenTimesteps)
        {
            InitializeSimulation(startTime, endTime, simulationIterationInterval);
            StepResult lastResult;
            while (true)
            {
                lastResult = Step();
                if (lastResult.SimulationStatus != SimulationStatus.Running)
                {
                    return lastResult;
                }

                if (delayBetweenTimesteps > TimeSpan.Zero)
                {
                    Thread.Sleep(delayBetweenTimesteps);
                }
            }
        }

        /// <summary>
        /// Initializes a simulation so it can be manually stepped through
        /// simulation using the <see cref="Step"/> method.
        /// </summary>
        /// <param name="startTime">Simulated time of the first time step.</param>
        /// <param name="endTime">End time (exclusive) for the simulation. Pass <see cref="DateTime.MaxValue"/> to run the simulation indefinitely.</param>
        /// <param name="simulationIterationInterval">Simulation interval.</param>
        public void InitializeSimulation(DateTimeOffset startTime, DateTimeOffset endTime, TimeSpan simulationIterationInterval)
        {
            if (startTime >= endTime)
                throw new ArgumentOutOfRangeException(nameof(endTime), "End time must be greater than start time.");

            switch (_simulationState)
            {
                case SimulationState.Initializing:
                    _simulationState = SimulationState.Running;
                    break;
                case SimulationState.Running:
                    throw new InvalidOperationException("Simulation is already running.");
                case SimulationState.Completed:
                    throw new InvalidOperationException("Simulation has already been run.");
                default:
                    break;
            }

            _logger.LogInformation("Initializing simulation, start: {StartTime}, end: {EndTime}, iteration interval: {IterationInterval}", startTime, endTime, simulationIterationInterval);
            _simulationState = SimulationState.Running;

            EventGenerator = new EventGenerator(startTime, endTime, simulationIterationInterval);
            

            int instanceCount = 0;
            foreach (var kvp in _models)
            {
                if (kvp.Value.SimulationProcessor == null)
                {
                    // No simulation processor means that this is just a real-time
                    // model. Skip.
                    continue;
                }

                var modelInstances = _instances[kvp.Key];
                foreach (var instanceRegistration in modelInstances.Values)
                {
                    EventGenerator.EnqueueEvent(instanceRegistration, startTime);
                    instanceCount++;
                }
            }
            
            // Some instance might have set up timers when they were first added
            // (via a DigitalTwinBase.Init override). Enqueue them now:
            foreach (var timerRegistration in Timers.Values)
            {
                EventGenerator.EnqueueEvent(timerRegistration, startTime + timerRegistration.Interval);
            }

            if (instanceCount == 0)
                throw new InvalidOperationException("No simulation instances were registered, no work to do.");
        }

        /// <summary>
        /// Executes events for the next time step.
        /// </summary>
        /// <returns><see cref="StepResult"/> containing the status and time of the next step in the simulation.</returns>
        public StepResult Step()
        {
            if (EventGenerator == null)
                throw new InvalidOperationException("Not debugging a simulation. Call Workbench.InitializeSimulation(startTime) before stepping.");

            // Make sure we aren't going over the simulation endTime. This could happen
            // if the user didn't inspect the status returned by prior Step() call.
            switch (_simulationState)
            {
                case SimulationState.Initializing:
                    throw new InvalidOperationException($"Simulation has not been started. Call {nameof(InitializeSimulation)} before stepping.");
                case SimulationState.Running:
                    // all good
                    break;
                case SimulationState.Completed:
                    throw new InvalidOperationException($"Simulation has completed, no more time steps to perform.");
                default:
                    throw new NotSupportedException($"Unknown simulation state {_simulationState}");
            }

            bool stopRequested = false;
            var events = EventGenerator.GetEventsForStep(); // advances the EventGenerator's CurrentTime/PeekNextTime

            foreach (var simEvent in events)
            {
                if (simEvent.IsDeleted)
                {
                    // This instance/timer must have been deleted by some other twin after
                    // it had been re-enqueued. Skip it.
                    continue;
                }    

                string modelName = simEvent.DigitalTwinInstance.Model;
                string instanceId = simEvent.DigitalTwinInstance.Id;

                var processingContext = new SimProcessingContext(null,
                                                                 simEvent,
                                                                 this,
                                                                 messageDepth: 0,
                                                                 logger: _logger
                                                                 );

                if (simEvent.ModelRegistration.InvokeProcessModel == null)
                    throw new InvalidOperationException("Model was not configured to process simulation events.");

                // NOTE: The returned ProcessingResult is ignored after invoking the callbacks below.
                // (Since we're working with inproc instances, all changes to the instance will be
                //  visible in the next time step.)
                switch (simEvent)
                {
                    case SimulationTimer timerRegistration:
                        _logger.LogTrace("Firing timer {TimerName} for {ModelName}\\{InstanceId}", timerRegistration.TimerName, timerRegistration.ModelRegistration.ModelName, timerRegistration.DigitalTwinInstance.Id);
                        _ = timerRegistration.TimerCallback(timerRegistration.TimerName, timerRegistration.DigitalTwinInstance, processingContext);
                        break;
                    case InstanceRegistration instanceRegistration:
                        _logger.LogTrace("Invoking ProcessModel for {ModelName}\\{InstanceId}", instanceRegistration.ModelRegistration.ModelName, instanceRegistration.DigitalTwinInstance.Id);
                        _ = instanceRegistration.ModelRegistration.InvokeProcessModel(processingContext, simEvent.DigitalTwinInstance, EventGenerator.SimulationTime);
                        break;
                    default:
                        throw new NotSupportedException($"Unexpected registration type {simEvent.GetType()}");
                }
                

                if (processingContext.DeleteRequested)
                {
                    RemoveInstance(modelName, instanceId);

                    // Skip the math/enqueuing that we do below--just move on
                    // to the next instance.
                    continue;
                }

                if (processingContext.StopRequested)
                {
                    stopRequested = true;
                }

                DateTimeOffset nextStepTimeForInstance;
                SimulationTimer? timerEvent = simEvent as SimulationTimer;
                if (timerEvent != null)
                {
                    if (timerEvent.TimerType == TimerType.Recurring && !timerEvent.IsDeleted)
                    {
                        // We just fired a recurring timer event. Enqueue for the timer's next interval.
                        long requestedWaitMillis = (long)timerEvent.Interval.TotalMilliseconds;
                        long intervalMillis = (long)EventGenerator.SimulationIterationInterval.TotalMilliseconds;

                        long intervalCount = requestedWaitMillis / intervalMillis;

                        // If the user's requested wait time falls between intervals,
                        // round up to the next interval:
                        if (requestedWaitMillis % intervalMillis > 0)
                            intervalCount++;

                        long actualWaitMillis = intervalCount * intervalMillis;
                        nextStepTimeForInstance = EventGenerator.SimulationTime + TimeSpan.FromMilliseconds(actualWaitMillis);
                    }
                    else
                    {
                        // This was a one-time timer. Or else it was deleted. Either way, delete the timer
                        nextStepTimeForInstance = DateTimeOffset.MaxValue;
                        Timers.Remove(timerEvent.TimerName);
                    }
                }
                else if (processingContext.RequestedSimulationCycleDelay == TimeSpan.Zero)
                {
                    // Normal simulation event, and user didn't ask for a delay. Use the default interval.
                    nextStepTimeForInstance = EventGenerator.SimulationTime + EventGenerator.SimulationIterationInterval;
                }
                else if (processingContext.RequestedSimulationCycleDelay == TimeSpan.MaxValue)
                {
                    // User doesn't want this instance to be subject to more simulation events
                    // (but does *not* want the instance deleted).
                    nextStepTimeForInstance = DateTimeOffset.MaxValue;
                }
                else
                {
                    // Normal simulation event, and user asked for a delay
                    long requestedWaitMillis = (long)processingContext.RequestedSimulationCycleDelay.TotalMilliseconds;
                    long intervalMillis = (long)EventGenerator.SimulationIterationInterval.TotalMilliseconds;

                    long intervalCount = requestedWaitMillis / intervalMillis;

                    // If the user's requested wait time falls between intervals,
                    // round up to the next interval:
                    if (requestedWaitMillis % intervalMillis > 0)
                        intervalCount++;

                    long actualWaitMillis = intervalCount * intervalMillis;
                    nextStepTimeForInstance = EventGenerator.SimulationTime + TimeSpan.FromMilliseconds(actualWaitMillis);
                }

                if (nextStepTimeForInstance != DateTimeOffset.MaxValue)
                {
                    EventGenerator.EnqueueEvent(simEvent, nextStepTimeForInstance);
                }
            }

            DateTimeOffset nextStep = EventGenerator.PeekNextTime();
            if (stopRequested)
            {
                _simulationState = SimulationState.Completed;
                return new StepResult(SimulationStatus.StopRequested, nextStep);
            }
            if (nextStep == DateTimeOffset.MaxValue)
            {
                _simulationState = SimulationState.Completed;
                return new StepResult(SimulationStatus.NoRemainingWork, DateTimeOffset.MaxValue);
            }
            else if (nextStep >= EventGenerator.EndTime)
            {
                _simulationState = SimulationState.Completed;
                return new StepResult(SimulationStatus.EndTimeReached, nextStep);
            }
            else
                return new StepResult(SimulationStatus.Running, nextStep);
        }

        internal void EnqueueImmediate(InstanceRegistration instanceRegistration)
        {
            if (instanceRegistration == null) 
                throw new ArgumentNullException(nameof(instanceRegistration));

            if (EventGenerator == null)
                throw new InvalidOperationException("Not debugging a simulation. Call Workbench.InitializeSimulation(startTime) before stepping.");

            EventGenerator.EnqueueEvent(instanceRegistration, EventGenerator.SimulationTime);
        }

    }
}

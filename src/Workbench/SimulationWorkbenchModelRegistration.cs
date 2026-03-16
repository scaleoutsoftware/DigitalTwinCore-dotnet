using Microsoft.Extensions.Logging;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimulationWorkbenchModelRegistration<TDigitalTwin> : ModelRegistration
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>, new()
    {
        internal delegate TDigitalTwin? MessageDeserializer(byte[] message);

        public SimulationWorkbenchModelRegistration(string modelName, ISharedData sharedModelData, SimulationWorkbench? simulationWorkbench)
            : base(modelName, sharedModelData, realTimeWorkbench: null, simulationWorkbench: simulationWorkbench)
        {
        }

        public override ProcessingResult InitSimulation(InitSimulationContext initSimulationContext, InstanceRegistration instanceRegistration, DateTimeOffset simulationTime)
        {
            if (instanceRegistration == null) throw new ArgumentNullException(nameof(instanceRegistration));
            InstanceRegistration<TDigitalTwin>? typedInstanceRegistration = instanceRegistration as InstanceRegistration<TDigitalTwin>;
            if (typedInstanceRegistration == null)
                throw new ArgumentException($"Simulation processor for {ModelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {instanceRegistration.GetType()}");


            SimulationProcessor<TDigitalTwin> simProcessor = SimulationProcessor as SimulationProcessor<TDigitalTwin> ?? throw new InvalidOperationException($"Simulation processor for {ModelName} is not of the expected type.");

            return simProcessor.OnInitSimulation(initSimulationContext, typedInstanceRegistration.DigitalTwinInstance, simulationTime);
        }

        public override async Task<SimProcessingResult> ProcessModelAsync(InstanceRegistration instanceRegistration, DateTimeOffset simulationTime, int messageDepth, ILogger? logger = null)
        {
            if (instanceRegistration == null) throw new ArgumentNullException(nameof(instanceRegistration));
            InstanceRegistration<TDigitalTwin>? typedInstanceRegistration = instanceRegistration as InstanceRegistration<TDigitalTwin>;
            if (typedInstanceRegistration == null)
                throw new ArgumentException($"Simulation processor for {ModelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {instanceRegistration.GetType()}");
            if (SimulationWorkbench == null)
                throw new InvalidOperationException($"Model {ModelName} is not registered for simulation.");


            SimulationProcessor<TDigitalTwin> simProcessor = SimulationProcessor as SimulationProcessor<TDigitalTwin> ?? throw new InvalidOperationException($"Simulation processor for {ModelName} is not of the expected type.");
            SimProcessingContext<TDigitalTwin> processingContext = new SimProcessingContext<TDigitalTwin>(typedInstanceRegistration, SimulationWorkbench, messageDepth, logger);
            ProcessingResult processing = await simProcessor.ProcessModelAsync(processingContext, typedInstanceRegistration.DigitalTwinInstance, simulationTime);
            
            return new SimProcessingResult
            {
                ProcessingResult = processing,
                StopRequested = processingContext.StopRequested,
                DeleteRequested = processingContext.DeleteRequested,
                RequestedSimulationCycleDelay = processingContext.RequestedSimulationCycleDelay
            };
        }

        public async override Task<ProcessingResult> ProcessMessageAsync(InstanceRegistration instanceRegistration, byte[] msgBytes, int messageDepth, ILogger? logger = null)
        {
            if (instanceRegistration == null) throw new ArgumentNullException(nameof(instanceRegistration));
            InstanceRegistration<TDigitalTwin>? typedInstanceRegistration = instanceRegistration as InstanceRegistration<TDigitalTwin>;
            if (typedInstanceRegistration == null)
                throw new ArgumentException($"Message processor for {ModelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {instanceRegistration.GetType()}");
            if (SimulationWorkbench == null)
                throw new InvalidOperationException($"Model {ModelName} is not registered for simulation processing.");

            MessageProcessor<TDigitalTwin> processor = MessageProcessor as MessageProcessor<TDigitalTwin> ?? throw new InvalidOperationException($"Message processor for {ModelName} is not of the expected type.");
            SimProcessingContext<TDigitalTwin> processingContext = new SimProcessingContext<TDigitalTwin>(typedInstanceRegistration, SimulationWorkbench, messageDepth, logger);
            ProcessingResult result = await processor.ProcessMessageAsync(processingContext, typedInstanceRegistration.DigitalTwinInstance, msgBytes);
            if (result == ProcessingResult.Remove)
            {
                // Remove the instance from the model:
                SimulationWorkbench.Instances[ModelName].Remove(typedInstanceRegistration.DigitalTwinInstance.Id, out var _);
                // TODO: Need to cleanup timers?
            }
            return result;
        }

        public override async Task<InstanceRegistration> CreateNewInitializedInstanceAsync(string instanceId, InstanceRegistration? dataSource, ILogger logger)
        {
            if (SimulationWorkbench == null)
                throw new InvalidOperationException($"Model {ModelName} is not registered for simulation processing.");
            TDigitalTwin newInstance = new TDigitalTwin();

            var instanceRegistration = new InstanceRegistration<TDigitalTwin>(instanceId, newInstance, this, dataSource);
            InitContext<TDigitalTwin> initContext = new SimInitContext<TDigitalTwin>(instanceRegistration, SimulationWorkbench);
            await newInstance.InitInternalAsync(instanceId, ModelName, initContext);
            return instanceRegistration;
        }
    }
}

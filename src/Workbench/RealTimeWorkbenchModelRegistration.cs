using Microsoft.Extensions.Logging;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class RealTimeWorkbenchModelRegistration<TDigitalTwin> : ModelRegistration
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>, new()
    {
        internal delegate TDigitalTwin? MessageDeserializer(byte[] message);

        public RealTimeWorkbenchModelRegistration(string modelName, ISharedData sharedModelData, RealTimeWorkbench? realTimeWorkbench)
            : base(modelName, sharedModelData, realTimeWorkbench, simulationWorkbench: null)
        {
        }

        public override ProcessingResult InitSimulation(InitSimulationContext initSimulationContext, InstanceRegistration instanceRegistration, DateTimeOffset simulationTime)
        {
            throw new InvalidOperationException("Model registered in RealTimeWorkbench cannot be used for simulation.");
        }

        public override Task<SimProcessingResult> ProcessModelAsync(InstanceRegistration instanceRegistration, DateTimeOffset simulationTime, int messageDepth, ILogger? logger = null)
        {
            throw new InvalidOperationException("Model registered in RealTimeWorkbench cannot be used for simulation.");
        }

        public async override Task<ProcessingResult> ProcessMessageAsync(InstanceRegistration instanceRegistration, byte[] msgBytes, int messageDepth, ILogger? logger = null)
        {
            if (instanceRegistration == null) throw new ArgumentNullException(nameof(instanceRegistration));
            InstanceRegistration<TDigitalTwin>? typedInstanceRegistration = instanceRegistration as InstanceRegistration<TDigitalTwin>;
            if (typedInstanceRegistration == null)
                throw new ArgumentException($"Message processor for {ModelName} is for a different digital twin type. Expected: {typeof(TDigitalTwin)}; Actual: {instanceRegistration.GetType()}");
            if (RealTimeWorkbench == null)
                throw new InvalidOperationException($"Model {ModelName} is not registered for real-time processing.");

            MessageProcessor<TDigitalTwin> processor = MessageProcessor as MessageProcessor<TDigitalTwin> ?? throw new InvalidOperationException($"Message processor for {ModelName} is not of the expected type.");
            RealTimeProcessingContext<TDigitalTwin> processingContext = new RealTimeProcessingContext<TDigitalTwin>(typedInstanceRegistration, RealTimeWorkbench, messageDepth, logger);
            ProcessingResult result = await processor.ProcessMessageAsync(processingContext, typedInstanceRegistration.DigitalTwinInstance, msgBytes);
            if (result == ProcessingResult.Remove)
            {
                // Remove the instance from the model:
                RealTimeWorkbench.Instances[ModelName].Remove(typedInstanceRegistration.DigitalTwinInstance.Id, out var _);
            }
            return result;
        }

        public override async Task<InstanceRegistration> CreateNewInitializedInstanceAsync(string instanceId, InstanceRegistration? dataSource, ILogger logger)
        {
            if (RealTimeWorkbench == null)
                throw new InvalidOperationException($"Model {ModelName} is not registered for real-time processing.");
            TDigitalTwin newInstance = new TDigitalTwin();

            var instanceRegistration = new InstanceRegistration<TDigitalTwin>(instanceId, newInstance, this, dataSource);
            InitContext<TDigitalTwin> initContext = new RealTimeInitContext<TDigitalTwin>(instanceRegistration, RealTimeWorkbench, logger);
            await newInstance.InitInternalAsync(instanceId, ModelName, initContext);
            return instanceRegistration;
        }
    }
}

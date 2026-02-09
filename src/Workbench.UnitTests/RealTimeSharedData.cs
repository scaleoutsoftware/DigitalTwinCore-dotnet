using Scaleout.DigitalTwin.DevEnv.Tests.Messages;
using Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench.UnitTests
{
    public class RealTimeSharedData
    {
        public class SharedDataMessageProcessor : MessageProcessor<RealTimeCarModel>
        {
            public override Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context, 
                                                                        RealTimeCarModel digitalTwin, 
                                                                        byte[] msgBytes)
            {
                // Modify global shared data:
                var res = context.SharedGlobalData.Get("foo");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    throw new Exception($"Shared global data item not found.");
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    context.SharedGlobalData.Put("foo", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                // Modify model-specific shared data:
                res = context.SharedModelData.Get("bar");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    throw new Exception($"Shared model data item not found.");
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    context.SharedModelData.Put("bar", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task ModifySharedDataAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel("RealTimeCar", new SharedDataMessageProcessor());

            var res = endpoint.SharedGlobalData.Put("foo", BitConverter.GetBytes(0));
            Assert.Equal(CacheOperationStatus.ObjectPut, res.Status);

            res = endpoint.SharedModelData.Put("bar", BitConverter.GetBytes(0));
            Assert.Equal(CacheOperationStatus.ObjectPut, res.Status);

            // Causes shared data to be incremented:
            var msg = new CarMessage { Speed = 22 };
            byte[] msgBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(msg));
            await endpoint.SendAsync("Car1", msgBytes);

            res = endpoint.SharedGlobalData.Get("foo");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            int value = BitConverter.ToInt32(res.Value);
            Assert.Equal(1, value);

            res = endpoint.SharedModelData.Get("bar");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(1, value);
        }

        public class RealTimeTrainModel : DigitalTwinBase
        {
            public int Speed { get; set; }

            public override void Init(string id, string model, InitContext initContext)
            {
                base.Init(id, model, initContext);

                // Modify global shared data:
                var res = initContext.SharedGlobalData.Get("foo");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    initContext.SharedGlobalData.Put("foo", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    initContext.SharedGlobalData.Put("foo", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }

                // Modify model-specific shared data:
                res = initContext.SharedModelData.Get("bar");
                if (res.Status == CacheOperationStatus.ObjectDoesNotExist)
                    initContext.SharedModelData.Put("bar", BitConverter.GetBytes(1));
                else if (res.Status == CacheOperationStatus.ObjectRetrieved)
                {
                    int current = BitConverter.ToInt32(res.Value);
                    initContext.SharedModelData.Put("bar", BitConverter.GetBytes(++current));
                }
                else
                {
                    throw new Exception($"Bad status {res.Status}");
                }
            }
        }

        public class TrainMessageProcessor : MessageProcessor<RealTimeTrainModel>
        {
            public override Task<ProcessingResult> ProcessMessagesAsync(ProcessingContext context,
                                                                        RealTimeTrainModel digitalTwin,
                                                                        byte[] msgBytes)
            {
                return Task.FromResult(ProcessingResult.DoUpdate);
            }
        }

        [Fact]
        public async Task SharedDataInInitContextAsync()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel("RealTimeTrain", new TrainMessageProcessor());

            // Add object via endpoint
            await endpoint.CreateTwinAsync("train1", new RealTimeTrainModel { Speed = 42 });

            // The model's Init method should have incremented shared model
            // and global objects.
            var res = endpoint.SharedGlobalData.Get("foo");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            int value = BitConverter.ToInt32(res.Value);
            Assert.Equal(1, value);

            res = endpoint.SharedModelData.Get("bar");
            Assert.Equal(CacheOperationStatus.ObjectRetrieved, res.Status);
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(1, value);

            // Send a msg to another (non-exising) train instnce. This should cause another
            // instance to be created:
            var msg = new CarMessage { Speed = 11 };
            byte[] msgBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(msg));
            await endpoint.SendAsync("train2", msgBytes);

            // This second instance's Init method should have incremented shared model
            // and global objects again.
            res = endpoint.SharedGlobalData.Get("foo");
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(2, value);

            res = endpoint.SharedModelData.Get("bar");
            value = BitConverter.ToInt32(res.Value);
            Assert.Equal(2, value);
        }

    }
}

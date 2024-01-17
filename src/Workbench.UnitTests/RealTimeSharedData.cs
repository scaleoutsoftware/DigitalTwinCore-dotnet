using Scaleout.DigitalTwin.DevEnv.Tests.Messages;
using Scaleout.DigitalTwin.DevEnv.Tests.RealTimeCar;
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench.UnitTests
{
    public class RealTimeSharedData
    {
        public class SharedDataMessageProcessor : MessageProcessor<RealTimeCarModel, CarMessage>
        {
            public override ProcessingResult ProcessMessages(ProcessingContext context, 
                                                             RealTimeCarModel digitalTwin, 
                                                             IEnumerable<CarMessage> newMessages)
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

                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void ModifySharedData()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel("RealTimeCar", new SharedDataMessageProcessor());

            var res = endpoint.SharedGlobalData.Put("foo", BitConverter.GetBytes(0));
            Assert.Equal(CacheOperationStatus.ObjectPut, res.Status);

            res = endpoint.SharedModelData.Put("bar", BitConverter.GetBytes(0));
            Assert.Equal(CacheOperationStatus.ObjectPut, res.Status);

            // Causes shared data to be incremented:
            endpoint.Send("Car1", new CarMessage { Speed = 22 });

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

        public class TrainMessageProcessor : MessageProcessor<RealTimeTrainModel, CarMessage>
        {
            public override ProcessingResult ProcessMessages(ProcessingContext context,
                                                             RealTimeTrainModel digitalTwin,
                                                             IEnumerable<CarMessage> newMessages)
            {
                return ProcessingResult.DoUpdate;
            }
        }

        [Fact]
        public void SharedDataInInitContext()
        {
            RealTimeWorkbench wb = new RealTimeWorkbench();
            var endpoint = wb.AddRealTimeModel("RealTimeTrain", new TrainMessageProcessor());

            // Add object via endpoint
            endpoint.CreateTwin("train1", new RealTimeTrainModel { Speed = 42 });

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
            endpoint.Send("train2", new CarMessage { Speed = 11 });

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

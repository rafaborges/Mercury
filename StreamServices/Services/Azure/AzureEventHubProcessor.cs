using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;
using AzureEventData = Microsoft.Azure.EventHubs.EventData;

namespace StreamServices.Services.Azure
{
    class AzureEventHubProcessor : IEventProcessor
    {
        private Action<EventData> CallBack;
        private Guid ID;

        public AzureEventHubProcessor(Guid id, Action<EventData> callback)
        {
            CallBack = callback;
            ID = id;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            //Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            //Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            //Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<AzureEventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                //Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");
                //OnNewData(new StreamDataEventArgs(eventData));
                CallBack(new EventData(ID, DateTime.Now, data));
            }

            return context.CheckpointAsync();
        }
    }
}
using Microsoft.Azure.EventHubs.Processor;
using System;

namespace StreamServices.Services.Azure
{
    public class EventProcessorFactory : IEventProcessorFactory
    {
        private Action<EventData> Callback;
        private Guid ID;

        public EventProcessorFactory(Guid id, Action<EventData> callback)
        {
            Callback = callback;
            ID = id;
        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new AzureEventHubProcessor(ID, Callback);
        }
    }
}

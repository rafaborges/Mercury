using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace StreamServices.Services.Azure
{
    public class AzureEventHub
    {
        private EventProcessorHost Host;

        private async Task Register(string entityPath, string connectionString, 
            string storageConnectionString, string storageContainerName)
        {
            Console.WriteLine("Registering EventProcessor...");

            Host = new EventProcessorHost( entityPath,
                PartitionReceiver.DefaultConsumerGroupName,
                connectionString, storageConnectionString,
                storageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await Host.RegisterEventProcessorAsync<AzureEventHubProcessor>();
        }

        private async Task Unregister()
        {
            // Disposes of the Event Processor Host
            await Host.UnregisterEventProcessorAsync();
        }
    }
}
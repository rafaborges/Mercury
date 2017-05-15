using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataServices.Interfaces;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace DataServices.Services.Azure
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
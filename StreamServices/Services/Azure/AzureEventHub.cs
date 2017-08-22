using System;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Collections.Generic;
using StreamServices.Interfaces;
using StreamServices.Buffer;

namespace StreamServices.Services.Azure
{
    public class AzureEventHub<T> : IStreamConsumer
    {
        /// <summary>
        /// Host object that holds the connection to Azure
        /// </summary>
        private EventProcessorHost Host;
        /// <summary>
        /// Dictionary with all necessary configuration to connect to Azure
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; }
        /// <summary>
        /// Unique ID for this instance of the consumer
        /// </summary>
        public Guid ID { get; }
        /// <summary>
        /// Buffered data
        /// </summary>
        public IBuffer Buffer { get; set; }
        /// <summary>
        /// Identifier of the Service
        /// </summary>
        public ServiceType ServiceType => ServiceType.Azure;
        /// <summary>
        /// Event fired when new data comes in
        /// </summary>
        public event EventHandler<StreamDataEventArgs> NewData;

        /// <summary>
        /// Registering the consumer
        /// </summary>
        public void RegisterConsumer()
        {
            Host = new EventProcessorHost(
                Configuration["entityPath"].ToString(), //"{Event Hub path/name}";
                PartitionReceiver.DefaultConsumerGroupName,
                Configuration["connectionString"].ToString(),//{Event Hubs connection string }
                Configuration["storageConnectionString"].ToString(), //"{Storage connection string}"
                Configuration["storageContainer"].ToString()); //"{Storage account container name}"

            // Creating the Action that is called back when the event processor
            // receives a new message. This is the worst architechture Microsoft
            // could came up with. At least they provided an IFactory...
            Action<EventData> action = new Action<EventData>(NewDataFromService);
            var eventProcessorFactory = new EventProcessorFactory(ID, action);
            // Start/register an EventProcessorHost
            Host.RegisterEventProcessorFactoryAsync(eventProcessorFactory);
        }

        /// <summary>
        /// Method that process incoming data. It's sent to
        /// the EventProcessor by an Action
        /// </summary>
        /// <param name="e"></param>
        private void NewDataFromService(EventData e)
        {
            Buffer.Push(e);
            var eventArgs = new StreamDataEventArgs(e, Buffer.ToList());
            NewData?.Invoke(null, eventArgs);
        }

        public void UnregisterConsumer()
        {
            Host.UnregisterEventProcessorAsync();
        }

        public bool ValidateConfiguration(Dictionary<string, object> configuration)
        {
            if (configuration.ContainsKey("connectionString"))
                if (configuration.ContainsKey("entityPath"))
                    if (configuration.ContainsKey("storageConnectionString"))
                        if (configuration.ContainsKey("storageContainer"))
                            return true;
            return false;
        }
    }
}
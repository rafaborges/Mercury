using StreamServices.Buffer;
using StreamServices.Interfaces;
using StreamServices.Services.Random;
using StreamServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamServices
{
    /// <summary>
    /// Singleton class to access data from registerd stream services
    /// </summary>
    public class StreamService
    {
       // private static volatile StreamService instance;
        private readonly static Lazy<StreamService> instance = new Lazy<StreamService>(() => new StreamService());

        // Lets make it thread safe
        private static object syncRoot = new Object();
        private List<IStreamConsumer> consumers;
        private Dictionary<Guid, List<Action<object>>> clientDelegates;

        private StreamService() {
            consumers = new List<IStreamConsumer>();
            clientDelegates = new Dictionary<Guid, List<Action<object>>>();
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static StreamService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        return instance.Value;
                    }
                }

                return instance.Value;
            }
        }

        /// <summary>
        /// Initiates a stream service
        /// </summary>
        /// <param name="connectionString">Connection string for the service</param>
        /// <param name="serviceType">The <see cref="ServiceType"/>ServiceType to be consumed</param>
        /// <returns>Returns a GUID for the requested service</returns>
        public Guid InitService(string connectionString, ServiceType serviceType, int bufferSize, BufferInvalidationType bufferType)
        {
            // Checking if there is already a service with this connection string for
            // a given service type. This ensures that we don't spam the service provider
            IStreamConsumer consumer = consumers.SingleOrDefault( c => 
                c.ConnectionSring == connectionString
                && c.ServiceType == serviceType);

            if (consumer == null)
            {
                switch (serviceType)
                {
                    case ServiceType.Azure:
                        consumer = null;
                        break;
                    case ServiceType.Kafka:
                        consumer = null;
                        break;
                    case ServiceType.Random:
                        consumer = StreamServiceFactory.CreateConsumer<RandomGenerator<double>>();
                        break;
                    default:
                        consumer = null;
                        break;
                }

                switch (bufferType)
                {
                    case BufferInvalidationType.Events:
                        consumer.Buffer = StreamServiceFactory.CreateBuffer<RingBuffer>(bufferSize);
                        break;
                    case BufferInvalidationType.Time:
                        consumer.Buffer = StreamServiceFactory.CreateBuffer<TimeBuffer>(bufferSize);
                        break;
                }   

                if (consumer != null)
                {
                    consumers.Add(consumer);
                    // Registering the consumer
                    consumer.RegisterConsumer();
                    // Subscribing to new events from the consumer
                    consumer.NewData += ConsumerNewDataEvent;
                }
            }

            if (consumer == null)
                throw new NullReferenceException("Could not Find an available service");
            else
                return consumer.ID;
        }

        /// <summary>
        /// Gets a snapshot of the buffer
        /// </summary>
        /// <param name="id">The ID of the service</param>
        /// <returns>A List<EventData> with the current buffered data</returns>
        public List<EventData> GetBuffuredData(Guid id)
        {
            return consumers.SingleOrDefault(c => c.ID == id)?.Buffer.ToList();
        }

        public bool HasID(Guid id)
        {
            return consumers.SingleOrDefault(c => c.ID == id) != null ? true : false;
        }

        /// <summary>
        /// Start listening to a service by providing a service ID and an 
        /// action to be executed whenever new data comes is
        /// </summary>
        /// <param name="id">The ID of the service</param>
        /// <param name="action">The action to be executed when new data arrives</param>
        public void SubscribeService(Guid id, Action<object> action)
        {
            // Reasons to use an action to a function:
            // (1) Security: subscribers have to have a valid guid. 
            // Otherwise they will not be able to consume it
            // (2) Performance: We can later implement a priority queue
            // and throttle down if needed

            if (clientDelegates.ContainsKey(id))
            {
                clientDelegates[id].Add(action);
            }
            else
            {
                clientDelegates.Add(id, new List<Action<object>> { action });
            }
        }

        /// <summary>
        /// Stop listening to a service
        /// </summary>
        /// <param name="id">The ID of the service</param>
        /// <param name="action">The action that is executed when new data arrives</param>
        public void UnsubscribeService(Guid id, Action<object> action)
        {
            if (clientDelegates.ContainsKey(id))
            {
                clientDelegates[id].Remove(action);
            }
        }

        /// <summary>
        /// Event that is fired when new data comes in
        /// </summary>
        /// <param name="sender">Null all the time</param>
        /// <param name="e">The EventData with all needed data</param>
        private void ConsumerNewDataEvent(object sender, StreamDataEventArgs e)
        {
            if (clientDelegates.ContainsKey(e.EventData.Source))
            {
                // Here is where the magic happens by dynamically invoking the client's function
                clientDelegates[e.EventData.Source].ForEach(d => d.DynamicInvoke(e.EventData));
            }
        }
    }
}
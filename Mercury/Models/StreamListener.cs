using DataServices.Buffer;
using Mercury.Scripts.Hubs;
using Microsoft.AspNet.SignalR;
using StreamServices;
using StreamServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace Mercury.Models
{
    public class StreamListener
    {
        public string Name { get; set; }
        public ServiceType Source { get; set; }
        public string ConnectionString { get; set; }
        public Guid ID { get; set; }
        public List<EventData> BufferedValues { get; set; }
        public Type DataType { get; set; }

        private Action<object> action;

        /// <summary>
        /// Default ctor for StreamListener class
        /// </summary>
        /// <param name="name">Name of the instance</param>
        /// <param name="source">Sources listed in <see cref="ServiceType"/> </param>
        /// <param name="connectionString">Connection string to be used</param>
        /// <param name="dataType">Expected type of data. Data Will always be casted to this type.</param>
        public StreamListener(string name, ServiceType source, string connectionString, Type dataType, int bufferSize, BufferInvalidationType bufferType)
        {
            Name = name;
            Source = source;
            ConnectionString = connectionString;
            DataType = dataType;
            action = new Action<object>(EventReceived);
            // Asking an ID for the StreamServices. If the source already exists with a given
            // connection string, the same source is reused
            ID = StreamService.Instance.InitService(connectionString, source, bufferSize, bufferType);
            
            // Get current buffered data. Note that if takes too long to start listening,
            // it may be outdated
            BufferedValues = StreamService.Instance.GetBuffuredData(ID);
        }

        /// <summary>
        /// Broadcast data to all registered clients matching the stream ID
        /// </summary>
        /// <param name="d">The value of the object</param>
        private void EventReceived(object data)
        {
            DataUpdater.Instance.BroadcastNewData(
                ID, 
                DateTime.Now,
                Convert.ChangeType(data, DataType));
        }

        /// <summary>
        /// Start consuming data from the data source
        /// </summary>
        public void StartListening()
        {
            StreamService.Instance.SubscribeService(ID, action);
        }

        public void StopListening()
        {
            StreamService.Instance.UnsubscribeService(ID, action);
        }
    }
}
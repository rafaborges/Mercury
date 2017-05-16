using StreamServices.Buffer;
using Mercury.Scripts.Hubs;
using Microsoft.AspNet.SignalR;
using StreamServices;
using StreamServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Xml.Linq;

namespace Mercury.Models
{
    [Serializable]
    public class StreamListener
    {
        public string Name { get; set; }
        public ServiceType Source { get; set; }
        public string ConnectionString { get; set; }
        public Guid ID { get; set; }
        public List<EventData> BufferedValues { get; set; }
        public Type DataType { get; set; }
        public int BufferSize { get; set; }
        public BufferInvalidationType BufferType { get; set; }

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
            BufferSize = bufferSize;
            BufferType = bufferType;
            action = new Action<object>(EventReceived);
            // Asking an ID for the StreamServices. If the source already exists with a given
            // connection string, the same source is reused
            ID = StreamService.Instance.InitService(connectionString, source, bufferSize, bufferType);
            
            // Get current buffered data. Note that if takes too long to start listening,
            // it may be outdated
            BufferedValues = StreamService.Instance.GetBuffuredData(ID);
        }

        /// <summary>
        /// Private constructor for late biding through confguration load
        /// </summary>
        private StreamListener()
        {

        }

        /// <summary>
        /// Broadcast data to all registered clients matching the stream ID
        /// </summary>
        /// <param name="d">The value of the object</param>
        private void EventReceived(object data)
        {
            var eventData = (EventData)data;
            
            DataUpdater.Instance.BroadcastNewData(
                ID,
                eventData.TimeStamp,
                Convert.ChangeType(eventData.Value, DataType));
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

        public XElement GetConfiguration()
        {
            return new XElement("Stream",
                    new XAttribute("Name", this.Name),
                    new XAttribute("Source", this.Source),
                    new XAttribute("DataType", this.DataType),
                    new XAttribute("ConnectionString", this.ConnectionString),
                    new XAttribute("ID", this.ID),
                    new XAttribute("BufferSize", this.BufferSize),
                    new XAttribute("BufferType", this.BufferType)
                    );
        }

        /// <summary>
        /// Creates an instance of StreamListener based on a saved configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static StreamListener GetStreamFromConfiguration(XElement configuration)
        {
            var listener = new StreamListener()
            {
                Name = configuration.Attribute("Name").Value,
                Source = (ServiceType)Enum.Parse(typeof(ServiceType), configuration.Attribute("Source").Value),
                BufferType = (BufferInvalidationType)Enum.Parse(typeof(BufferInvalidationType), configuration.Attribute("BufferType").Value),
                ConnectionString = configuration.Attribute("ConnectionString").Value,
                BufferSize = Convert.ToInt32(configuration.Attribute("BufferSize").Value),
                DataType = Type.GetType(configuration.Attribute("DataType").Value)
            };

            listener.action = new Action<object>(listener.EventReceived);

            // It tries to reuse the same ID, but there is no guarantee that it exists
            var tempID = Guid.Parse(configuration.Attribute("ID").Value);

            if (StreamService.Instance.HasID(tempID))
                listener.ID = tempID;
            else
                listener.ID = StreamService.Instance.InitService(listener.ConnectionString, listener.Source, listener.BufferSize, listener.BufferType);
            listener.BufferedValues = StreamService.Instance.GetBuffuredData(listener.ID);
            return listener;
        }
    }
}
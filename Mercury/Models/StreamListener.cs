using StreamServices.Buffer;
using Mercury.Scripts.Hubs;
using StreamServices;
using StreamServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Mercury.Models
{
    [Serializable]
    public class StreamListener
    {
        public string Name { get; set; }
        public ServiceType Source { get; set; }
        public Guid ID { get; set; }
        public List<EventData> BufferedValues { get; set; }
        public Type DataType { get; set; }
        public int BufferSize { get; set; }
        public BufferInvalidationType BufferType { get; set; }
        public Dictionary<string, object> Configuration { get; set; }

        private Action<StreamDataEventArgs> action;

        /// <summary>
        /// Default ctor for StreamListener class
        /// </summary>
        /// <param name="name">Name of the instance</param>
        /// <param name="source">Sources listed in <see cref="ServiceType"/> </param>
        /// <param name="dataType">Expected type of data. Data Will always be casted to this type.</param>
        public StreamListener(string name, ServiceType source, Type dataType, int bufferSize, 
            BufferInvalidationType bufferType, Dictionary<string, object> configuration)
        {
            Name = name;
            Source = source;
            DataType = dataType;
            BufferSize = bufferSize;
            BufferType = bufferType;
            Configuration = configuration;
            action = new Action<StreamDataEventArgs>(EventReceived);
            // Asking an ID for the StreamServices. If the source already exists with a given
            // configuration, the same source is reused
            ID = StreamService.Instance.InitService(source, 
                bufferSize, bufferType, configuration, BufferPersistenceOptions.SQLServer);
            
            // Get current buffered data. Note that if takes too long to start listening,
            // it may be outdated
            BufferedValues = StreamService.Instance.GetBuffuredData(ID);
        }

        /// <summary>
        /// Private constructor for late biding through confguration load
        /// </summary>
        private StreamListener() { }

        /// <summary>
        /// Broadcast data to all registered clients matching the stream ID
        /// </summary>
        /// <param name="d">The value of the object</param>
        private void EventReceived(StreamDataEventArgs data)
        {   
            //DataUpdater.Instance.BroadcastNewData(
            //    ID,
            //    data.EventData.TimeStamp,
            //    Convert.ChangeType(data.EventData.Value, DataType));

            DataUpdater.Instance.BroadcastAllData(ID, data.BufferedValues);
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
            var config = new XElement("Stream",
                    new XAttribute("Name", this.Name),
                    new XAttribute("Source", this.Source),
                    new XAttribute("DataType", this.DataType),
                    new XAttribute("ID", this.ID),
                    new XAttribute("BufferSize", this.BufferSize),
                    new XAttribute("BufferType", this.BufferType));

            var extendedConfig = new XElement("ExtendConfiguration");
            Configuration.Keys.ToList().ForEach(
                k => extendedConfig.Add(new XAttribute(k, Configuration[k].ToString())));

            config.Add(extendedConfig);

            return config;
        }

        /// <summary>
        /// Creates an instance of StreamListener based on a saved configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static StreamListener GetStreamFromConfiguration(XElement configuration)
        {
            Dictionary<string, object> extendedConfiguration = new Dictionary<string, object>();
            configuration.Element("ExtendConfiguration")?.Attributes().ToList().ForEach(a => extendedConfiguration.Add(a.Name.ToString(), a.Value));

            var listener = new StreamListener()
            {
                Name = configuration.Attribute("Name").Value,
                Source = (ServiceType)Enum.Parse(typeof(ServiceType), configuration.Attribute("Source").Value),
                BufferType = (BufferInvalidationType)Enum.Parse(typeof(BufferInvalidationType), configuration.Attribute("BufferType").Value),
                BufferSize = Convert.ToInt32(configuration.Attribute("BufferSize").Value),
                DataType = Type.GetType(configuration.Attribute("DataType").Value),
                Configuration = extendedConfiguration
            };

            listener.action = new Action<StreamDataEventArgs>(listener.EventReceived);

            // It tries to reuse the same ID, but there is no guarantee that it exists
            var tempID = Guid.Parse(configuration.Attribute("ID").Value);

            if (StreamService.Instance.HasID(tempID))
                listener.ID = tempID;
            else
                listener.ID = StreamService.Instance.InitService(listener.Source, listener.BufferSize, 
                    listener.BufferType, listener.Configuration, BufferPersistenceOptions.SQLServer);
            listener.BufferedValues = StreamService.Instance.GetBuffuredData(listener.ID);
            return listener;
        }
    }
}
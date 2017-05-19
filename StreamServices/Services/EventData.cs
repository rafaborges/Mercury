using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StreamServices.Services
{
    /// <summary>
    /// Objects tha represents a message sent from a stream source
    /// </summary>
    public class EventData
    {
        public Guid Source { get; set; }
        public DateTime TimeStamp { get; set; }
        public object Value { get; set; }
        public string FormatedTimeStamp => TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff");

        public EventData(Guid source, DateTime timestamp, object value)
        {
            Source = source;
            TimeStamp = timestamp;
            Value = value;
        }

        /// <summary>
        /// Gets a XML representation of the event
        /// </summary>
        /// <returns>A XML element</returns>
        public XElement GetXML()
        {
            var config = new XElement("EventData",
                    new XAttribute("Source", Source),
                    new XAttribute("TimeStamp", TimeStamp),
                    new XAttribute("Value", Value));

            return config;
        }

        /// <summary>
        /// Converts a XML representantion into an EventData object
        /// </summary>
        /// <param name="xml">The XML to be converted</param>
        /// <returns>An EventData from the provided XML</returns>
        public static EventData GetEventFromXML(XElement xml)
        {
            var sourceID = (Guid) Convert.ChangeType(xml.Attribute("Source").Value, typeof(Guid));
            var timeStamp = (DateTime) Convert.ChangeType(xml.Attribute("TimeStamp").Value, typeof(DateTime));
            var value = xml.Attribute("Value").Value;
            return new EventData(sourceID, timeStamp, value);
        }
    }
}

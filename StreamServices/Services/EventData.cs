using System;
using System.Collections.Generic;

namespace StreamServices.Services
{
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
    }
}

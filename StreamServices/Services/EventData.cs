using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamServices.Services
{
    public class EventData
    {
        public Guid Source { get; set; }
        public DateTime TimeStamp { get; set; }
        public object Value { get; set; }

        public EventData(Guid source, DateTime timestamp, object value)
        {
            Source = source;
            TimeStamp = timestamp;
            Value = value;
        }
    }
}

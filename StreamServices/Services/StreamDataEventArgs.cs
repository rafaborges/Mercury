using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamServices.Services
{
    /// <summary>
    /// EventArg to be used whenever firing events from a IStreamConsumer class
    /// </summary>
    public class StreamDataEventArgs : EventArgs
    {
        public EventData EventData { get; set; }

        public StreamDataEventArgs(EventData data)
        {
            EventData = data;
        }
    }
}

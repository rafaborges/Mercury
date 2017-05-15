using DataServices.Buffer;
using StreamServices;
using StreamServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Interfaces
{
    public interface IStreamConsumer
    {
        // The main event that fires when new data comes in
        event EventHandler<StreamDataEventArgs> NewData;
        // Usually the method that connects to a source
        void RegisterConsumer();
        // Well...
        void UnregisterConsumer();
        string ConnectionSring { get; set; }

        Guid ID { get; set; }

        IBuffer Buffer { get; set; }

        ServiceType ServiceType { get; }
    }
}

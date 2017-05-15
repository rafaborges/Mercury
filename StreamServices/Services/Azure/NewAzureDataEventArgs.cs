using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Services.Azure
{
    class NewAzureDataEventArgs : EventArgs
    {
        private EventData Data;

        public NewAzureDataEventArgs(EventData data)
        {
            Data = data;
        }
    }
}

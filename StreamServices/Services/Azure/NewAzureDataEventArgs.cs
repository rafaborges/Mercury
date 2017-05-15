using System;
using AzureEventData = Microsoft.Azure.EventHubs.EventData;

namespace StreamServices.Services.Azure
{
    class NewAzureDataEventArgs : EventArgs
    {
        private AzureEventData Data;

        public NewAzureDataEventArgs(AzureEventData data)
        {
            Data = data;
        }
    }
}

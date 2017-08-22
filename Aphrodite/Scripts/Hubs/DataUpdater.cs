using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using StreamServices.Services;
using System;
using System.Collections.Generic;

namespace Aphrodite.Scripts.Hubs
{
    /// <summary>
    /// Class responsible to broadcast data to all registered SignalR clients
    /// </summary>
    public class DataUpdater
    {
        private readonly static Lazy<DataUpdater> _instance = new Lazy<DataUpdater>(() => new DataUpdater(GlobalHost.ConnectionManager.GetHubContext<DataHub>().Clients));

        private DataUpdater(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        public static DataUpdater Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public void BroadcastNewData(Guid id, DateTime timeStamp, object value)
        {
            Clients.All.updateStream(id, timeStamp, value);
        }

        public void BroadcastAllData(Guid id, List<EventData> data)
        {
            Clients.All.updateAllData(id, data);
        }

        public void InitChartData(Guid id, List<EventData> data)
        {
            Clients.All.initData(id, data);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Mercury.Scripts.Hubs
{
    /// <summary>
    /// Hub class used by SignalR to concentrate all clients. 
    /// Data broadcast is done by <see cref="DataUpdater"/>
    /// </summary>
    public class DataHub : Hub
    {
        private readonly DataUpdater _updater;

        public DataHub() : this(DataUpdater.Instance) { }

        public DataHub(DataUpdater updater)
        {
            _updater = updater;
        }
    }
}
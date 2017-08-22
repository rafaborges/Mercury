using Microsoft.AspNet.SignalR;

namespace Aphrodite.Scripts.Hubs
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
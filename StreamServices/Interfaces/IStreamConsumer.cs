using StreamServices.Buffer;
using StreamServices.Services;
using System;
using System.Collections.Generic;

namespace StreamServices.Interfaces
{
    public interface IStreamConsumer
    {
        /// <summary>
        /// The main event that fires when new data comes in
        /// </summary>
        event EventHandler<StreamDataEventArgs> NewData;
        /// <summary>
        /// Usually the metho that connects to the data source
        /// </summary>
        /// <param name="configuration"></param>
        void RegisterConsumer();
        /// <summary>
        /// Unregister itself from the data producer
        /// </summary>
        void UnregisterConsumer();
        /// <summary>
        /// Configuration to be passed to the StreamConsumer
        /// </summary>
        Dictionary<string, object> Configuration { get; set; }
        /// <summary>
        /// Unique ID for the IStreamConsumer instance
        /// </summary>
        Guid ID { get; }
        /// <summary>
        /// Buffer that is injected later
        /// </summary>
        IBuffer Buffer { get; set; }
        /// <summary>
        /// Just a pointer to <see cref="ServiceType"/>
        /// </summary>
        ServiceType ServiceType { get; }

        /// <summary>
        /// Validates if a given configuration conforms to what
        /// is expected by the IStreamConsumer
        /// </summary>
        /// <param name="configuration">A Dictionary containing the necessary configuration</param>
        /// <returns>A bool indicating whether the configuration conforms or not</returns>
        bool ValidateConfiguration(Dictionary<string, object> configuration);
    }
}

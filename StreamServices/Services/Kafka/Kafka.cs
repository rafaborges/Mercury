using System;
using StreamServices.Buffer;
using StreamServices.Interfaces;
using Confluent.Kafka.Serialization;
using System.Collections.Generic;
using Confluent.Kafka;
using System.Text;
using System.Threading.Tasks;

namespace StreamServices.Services.Kafka
{
    /// <summary>
    /// Class that implements a Kafka consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Kafka<T> : IStreamConsumer
    {
        public Guid ID { get; }
        public IBuffer Buffer { get; set; }
        public ServiceType ServiceType => ServiceType.Kafka;
        public Dictionary<string, object> Configuration { get; set; }

        public event EventHandler<StreamDataEventArgs> NewData;

        /// <summary>
        /// This is the task where the main consumer is executed
        /// </summary>
        private Task consumerTask;

        public Kafka()
        {
            ID = Guid.NewGuid();
        }

        public bool ValidateConfiguration(Dictionary<string, object> configuration)
        {
            if (configuration.ContainsKey("host"))
                if (configuration.ContainsKey("topic"))
                    return true;

            return false;
        }

        public void UnregisterConsumer()
        {
            consumerTask?.Dispose();
        }

        protected virtual void OnNewData(StreamDataEventArgs e)
        {
            NewData?.Invoke(null, e);
        }

        public void RegisterConsumer()
        {
            // Kafka consumers are usually STA. Starting a new task allows the application
            // to fully manage the access to the server.
            consumerTask = Task.Factory.StartNew(() =>
            {
                var config = new Dictionary<string, object>
                {
                    { "group.id", "mercury-consumer" },
                    { "bootstrap.servers", Configuration["host"].ToString() }
                };

                using (var consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8)))
                {
                    consumer.OnMessage += (_, e) => {
                        EventData data = new EventData(ID, DateTime.Now, Convert.ChangeType(e.Value, typeof(T)));
                        Buffer.Push(data);
                        OnNewData(new StreamDataEventArgs(data, Buffer.ToList()));
                    };

                    consumer.Subscribe(Configuration["topic"].ToString());

                    // Ugly, I known, but it's encapsulated in a separated Task than can later
                    // be disposed
                    while (true) 
                    {
                        consumer.Poll();
                    }
                }
            });
        }
    }
}

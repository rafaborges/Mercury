using System;
using StreamServices.Buffer;
using StreamServices.Interfaces;
using Confluent.Kafka.Serialization;
using System.Collections.Generic;
using Confluent.Kafka;
using System.Text;

namespace StreamServices.Services.Kafka
{
    class Kafka<T> : IStreamConsumer
    {
        public string ConnectionSring { get; set; }
        public Guid ID { get; set; }
        public IBuffer Buffer { get; set; }

        public ServiceType ServiceType => ServiceType.Kafka;

        public event EventHandler<StreamDataEventArgs> NewData;

        private Consumer<Null, string> consumer;
        public Kafka()
        {
            ID = Guid.NewGuid();

            

            //while (true)
            //{
            //    Message<Null, string> msg;
            //    if (consumer.Consume(out msg))
            //    {
            //        Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
            //    }
            //}

            //var config = new Config() { GroupId = "example-csharp-consumer" };
            //using (var consumer = new EventConsumer(config, "127.0.0.1:9092"))
            //{
            //    consumer.OnMessage += (obj, msg) =>
            //    {
            //        string text = Encoding.UTF8.GetString(msg.Payload, 0, msg.Payload.Length);
            //        Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {text}");
            //    };

            //    consumer.Subscribe(new[] { "testtopic" });
            //    consumer.Start();

            //    Console.WriteLine("Started consumer, press enter to stop consuming");
            //    Console.ReadLine();
            //}
        }

        private void Consumer_OnMessage(object sender, Message<Null, string> e)
        {
            EventData data = new EventData(ID, e.Timestamp.UtcDateTime, Convert.ChangeType(e.Value,typeof(T)));
            Buffer.Push(data);
            OnNewData(new StreamDataEventArgs(data,Buffer.ToList()));
        }

        public void RegisterConsumer()
        {
            var config = new Dictionary<string, object>
            {
                { "group.id", "simple-csharp-consumer" },
                { "bootstrap.servers", "Localhost:9092" }
            };

            consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8));
            consumer.OnMessage += Consumer_OnMessage;
            consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset("tutorial", 0, 0) });
            consumer.Poll();
        }

        public void UnregisterConsumer()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnNewData(StreamDataEventArgs e)
        {
            NewData?.Invoke(null, e);
        }
    }
}

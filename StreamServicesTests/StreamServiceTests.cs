using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamServices.Services.Random;
using StreamServices.Services.Azure;
using StreamServices.Services.Kafka;
using StreamServices.Buffer;
using StreamServices.Services;

namespace StreamServices.Tests
{
    [TestClass]
    public class StreamServiceTests
    {
        [TestMethod]
        public void InitServiceTest()
        {
            // Testing intantiating all services
            var random = StreamServiceFactory.CreateConsumer<RandomGenerator<double>>();
            Assert.IsNotNull(random.ID);
            var azure = StreamServiceFactory.CreateConsumer<AzureEventHub<double>>();
            Assert.IsNotNull(azure.ID);
            var kafka = StreamServiceFactory.CreateConsumer<Kafka<double>>();
            Assert.IsNotNull(kafka.ID);

            // For obvious reasons, test is executed on random.
            random.Buffer = StreamServiceFactory.CreateBuffer<RingBuffer>(10, BufferPersistenceOptions.None, random.ID);
            Assert.IsNotNull(random.Buffer);
            random.RegisterConsumer();
            random.NewData += delegate (object sender, StreamDataEventArgs e)
                {
                    // Is the message correctly configured?
                    Assert.IsNotNull(e);
                    Assert.AreEqual(e?.EventData.Source, random.ID);
                    Assert.IsNotNull(e.EventData.Value);

                    // Was the buffer proper set?
                    Assert.IsTrue(e.BufferedValues.Count == 10);
                };
        }
    }
}
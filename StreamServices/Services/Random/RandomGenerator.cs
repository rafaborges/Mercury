using StreamServices.Buffer;
using StreamServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace StreamServices.Services.Random
{
    /// <summary>
    /// Random interface for testing purpose
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RandomGenerator<T> : IStreamConsumer
    {
        /// <summary>
        /// Event fired whenever new data comes in
        /// </summary>
        public event EventHandler<StreamDataEventArgs> NewData;
        /// <summary>
        /// Timer used to simulate data
        /// </summary>
        private Timer timer;
        /// <summary>
        /// Random that will be used to generate - guess what - random values
        /// </summary>
        private System.Random seed = new System.Random();
        // Chars that are used for random string generation
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        /// <summary>
        /// ID for this instance of consumer
        /// </summary>
        public Guid ID { get; set; }
        /// <summary>
        /// Buffer containing previous data
        /// </summary>
        public IBuffer Buffer { get; set; }
        /// <summary>
        /// Identification of the service type
        /// </summary>
        public ServiceType ServiceType => ServiceType.Random;
        /// <summary>
        /// Dictionary holding the necessary configuration
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; }

        public RandomGenerator()
        {
            ID = Guid.NewGuid();
        }

        public void RegisterConsumer()
        {
            if (IsSupported())
            {
                timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += Timer_Tick;
                FillBuffer();
                timer.Start();
            }
            else
            {
                throw new NotSupportedException(String.Format("Type {0} is not supported", typeof(T).Name));
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            object o = GetRandom();
            var eventData = new EventData(ID, DateTime.Now, o);
            Buffer.Push(eventData);
            OnNewData(new StreamDataEventArgs(eventData, Buffer.ToList()));
        }

        public void UnregisterConsumer()
        {
            timer.Stop();
        }

        protected virtual void OnNewData(StreamDataEventArgs e)
        {
            NewData?.Invoke(null, e);
        }

        /// <summary>
        /// Checks wether the specified type is supported by the random generator
        /// </summary>
        /// <returns></returns>
        private bool IsSupported()
        {
            HashSet<Type> supportedTypes = new HashSet<Type>
            {
                typeof(Int16),
                typeof(Int32),
                typeof(Int64),
                typeof(Decimal),
                typeof(Double),
                typeof(Single),
                typeof(String),
                typeof(Boolean)
            };
            return supportedTypes.Contains(typeof(T));
        }

        /// <summary>
        /// Fills up the buffer with random data
        /// </summary>
        private void FillBuffer()
        {
            var baseDate = DateTime.Now;
            for (int i = 0; i < Buffer.Capacity; i++)
            {
                Buffer.Push(new EventData(
                                    ID,
                                    baseDate.AddSeconds(i - Buffer.Capacity),
                                    GetRandom()));
            }
        }
   
        /// <summary>
        /// Generates a random based on the class type
        /// </summary>
        /// <returns></returns>
        private object GetRandom()
        {
            if (typeof(T) == typeof(Int16) || typeof(T) == typeof(Int32) || typeof(T) == typeof(Int64))
            {
                return seed.Next();
            }
            else if (typeof(T) == typeof(Double) || typeof(T) == typeof(Decimal) || typeof(T) == typeof(Single))
            {
                return Convert.ChangeType(seed.NextDouble(), typeof(T));
            }
            else if (typeof(T) == typeof(String))
            {
                return new string(Enumerable.Repeat(chars, 4).Select(s => s[seed.Next(s.Length)]).ToArray());
            }
            else if (typeof(T) == typeof(String))
            {
                return Convert.ToBoolean(seed.Next(0, 1));
            }
            else
            {
                return null;
            }
        }

        public bool ValidateConfiguration(Dictionary<string, object> configuration)
        {
            // There is no configuration for the random generator!
            // TODO: Frequency interval!!
            return true;
        }
    }
}

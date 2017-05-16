using StreamServices.Buffer;
using StreamServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace StreamServices.Services.Random
{
    class RandomGenerator<T> : IStreamConsumer
    {
        public event EventHandler<StreamDataEventArgs> NewData;
        private Timer timer;
        private System.Random seed = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public string ConnectionSring { get; set; }
        public Guid ID { get; set; }
        public IBuffer Buffer { get; set; }
        public ServiceType ServiceType => ServiceType.Random;

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
            OnNewData(new StreamDataEventArgs(eventData));
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
    }
}

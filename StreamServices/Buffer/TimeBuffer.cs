using System;
using System.Collections.Generic;
using StreamServices.Services;
using System.Collections;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;

namespace StreamServices.Buffer
{
    /// <summary>
    /// Buffer with data invalidation based on residence time
    /// </summary>
    class TimeBuffer : IBuffer
    {
        /// <summary>
        /// Lock object for the monitor
        /// </summary>
        static readonly object _locker = new object();

        /// <summary>
        /// Data repository
        /// </summary>
        private List<Tuple<Guid, EventData>> _data;

        public TimeBuffer()
        {
            _data = new List<Tuple<Guid, EventData>>();
        }

        /// <summary>
        /// Retention in seconds
        /// </summary>
        public int Capacity { get; set; }

        public bool IsEmpty => _data.Count == 0 ? true : false;

        /// <summary>
        /// In this Time buffer implementation, the buffer
        /// is never full
        /// </summary>
        public bool IsFull => false;

        public int Size => _data.Count;

        public IBufferPersistence BufferPersistence { get; set; }

        public IEnumerator GetEnumerator()
        {
            return _data.Select(d => d.Item2).GetEnumerator();
        }

        public void Pop()
        {
            lock (_locker)
            {
                if (_data.Count > 0)
                    _data.RemoveAt(0);
            }
        }

        public void Push(EventData item)
        {
            // Generating an unique identifier for the tuple
            var id = Guid.NewGuid();
            // locking...
            lock (_locker)
            {
                _data.Add(new Tuple<Guid, EventData>(id, item));
            }

            BufferPersistence?.StoreData(item);

            // Here we create a new tas that will be schedule
            // To be executed in span seconds
            // This is the best approach for a asp.net mvc
            // fire & forget.
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(Capacity * 1000);
                DeleteData(id);
                BufferPersistence?.RemoveData(item);
            });
        }

        public EventData[] ToArray()
        {
            return _data.Select(d => d.Item2).ToArray();
        }

        public List<EventData> ToList()
        {
            return _data.Select(d => d.Item2).ToList();
        }

        private void DeleteData(Guid id)
        {
            lock(_locker)
            {
                _data.RemoveAll(o => o.Item1 == id);
            }
        }
    }
}
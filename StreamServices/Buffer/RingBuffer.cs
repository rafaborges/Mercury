using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using StreamServices.Services;

namespace DataServices.Buffer
{
    /// <summary>
    /// Circular buffer.
    /// 
    /// When writting to a full buffer:
    /// PushBack -> removes this[0] / Front()
    /// PushFront -> removes this[Size-1] / Back()
    /// 
    /// this implementation is heavily inspired by
    /// https://github.com/joaoportela/CircullarBuffer-CSharp
    /// </summary>
    public class RingBuffer : IBuffer
    {
        private object[] _buffer;

        /// <summary>
        /// The _start. Index of the first element in buffer.
        /// </summary>
        private int _start;

        /// <summary>
        /// The _end. Index after the last element in the buffer.
        /// </summary>
        private int _end;

        /// <summary>
        /// The _size. Buffer size.
        /// </summary>
        private int _size;

        /// <summary>
        /// The buffer session ID
        /// </summary>
        private Guid _id;

        /// <summary>
        /// The buffer persistence option
        /// </summary>
        private BufferPersistenceOptions _persistenceOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer.RingBuffer{T}"/> class.
        /// 
        /// </summary>
        /// <param name='capacity'>
        /// Buffer capacity. Must be positive.
        /// </param>
        public RingBuffer(int capacity, Guid id, BufferPersistenceOptions options)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer cannot have negative or zero capacity.", "capacity");
            }
            if (id == null)
            {
                throw new ArgumentNullException("Missing ID");
            }

            // Loading from repository. Returns an empty [] if no previous data
            var items = Load();

            if (items.Length > capacity)
            {
                throw new ArgumentException(
                    "Too many items to fit circular buffer", "items");
            }

            _buffer = new object[capacity];

            Array.Copy(items, _buffer, items.Length);
            _size = items.Length;

            _start = 0;
            _end = _size == capacity ? 0 : _size;

            _id = id;
        }

        /// <summary>
        /// Empty constructor to be used by the IBuffer factory
        /// </summary>
        public RingBuffer()
        {

        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after
        /// maximum capacity is reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity { get { return _buffer.Length; } set { _buffer = new object[value]; } }

        public bool IsFull
        {
            get
            {
                return Size == Capacity;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Size == 0;
            }
        }

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Size { get { return _size; } }

        /// <summary>
        /// Element at the front of the buffer - this[0].
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer.</returns>
        public object Front()
        {
            ThrowIfEmpty();
            return _buffer[_start];
        }

        /// <summary>
        /// Element at the back of the buffer - this[Size - 1].
        /// </summary>
        /// <returns>The value of the element of type T at the back of the buffer.</returns>
        public object Back()
        {
            ThrowIfEmpty();
            return _buffer[(_end != 0 ? _end : _size) - 1];
        }

        public object this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
                }
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
                }
                int actualIndex = InternalIndex(index);
                return _buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer is empty", index));
                }
                if (index >= _size)
                {
                    throw new IndexOutOfRangeException(string.Format("Cannot access index {0}. Buffer size is {1}", index, _size));
                }
                int actualIndex = InternalIndex(index);
                _buffer[actualIndex] = value;
            }
        }

        /// <summary>
        /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Front()/this[0] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the back of the buffer</param>
        public void PushBack(object item)
        {
            if (IsFull)
            {
                _buffer[_end] = item;
                Increment(ref _end);
                _start = _end;
            }
            else
            {
                _buffer[_end] = item;
                Increment(ref _end);
                ++_size;
            }
        }

        /// <summary>
        /// Pushes a new element to the front of the buffer. Front()/this[0]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Back()/this[Size-1] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the front of the buffer</param>
        public void PushFront(object item)
        {
            if (IsFull)
            {
                Decrement(ref _start);
                _end = _start;
                _buffer[_start] = item;
            }
            else
            {
                Decrement(ref _start);
                _buffer[_start] = item;
                ++_size;
            }
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreassing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            Decrement(ref _end);
            _buffer[_end] = default(object);
            --_size;
        }

        /// <summary>
        /// Removes the element at the front of the buffer. Decreassing the 
        /// Buffer size by 1.
        /// </summary>
        public void PopFront()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            _buffer[_start] = default(object);
            Increment(ref _start);
            --_size;
        }

        /// <summary>
        /// Copies the buffer contents to an array, acording to the logical
        /// contents of the buffer (i.e. independent of the internal 
        /// order/contents)
        /// </summary>
        /// <returns>A new array with a copy of the buffer contents.</returns>
        public object[] ToArray()
        {
            object[] newArray = new object[Size];
            int newArrayOffset = 0;
            var segments = new ArraySegment<object>[2] { ArrayOne(), ArrayTwo() };
            foreach (ArraySegment<object> segment in segments)
            {
                Array.Copy(segment.Array, segment.Offset, newArray, newArrayOffset, segment.Count);
                newArrayOffset += segment.Count;
            }
            return newArray;
        }

        #region IEnumerable implementation
        public IEnumerator GetEnumerator()
        {
            var segments = new ArraySegment<object>[2] { ArrayOne(), ArrayTwo() };
            foreach (ArraySegment<object> segment in segments)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    yield return segment.Array[segment.Offset + i];
                }
            }
        }
        #endregion
        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        #endregion

        private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Increment(ref int index)
        {
            if (++index == Capacity)
            {
                index = 0;
            }
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        private void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = Capacity;
            }
            index--;
        }

        /// <summary>
        /// Converts the index in the argument to an index in <code>_buffer</code>
        /// </summary>
        /// <returns>
        /// The transformed index.
        /// </returns>
        /// <param name='index'>
        /// External index.
        /// </param>
        private int InternalIndex(int index)
        {
            return _start + (index < (Capacity - _start) ? index : index - Capacity);
        }

        #region Array items easy access.
        // The array is composed by at most two non-contiguous segments, 
        // the next two methods allow easy access to those.

        private ArraySegment<object> ArrayOne()
        {
            if (_start < _end)
            {
                return new ArraySegment<object>(_buffer, _start, _end - _start);
            }
            else
            {
                return new ArraySegment<object>(_buffer, _start, _buffer.Length - _start);
            }
        }

        private ArraySegment<object> ArrayTwo()
        {
            if (_start < _end)
            {
                return new ArraySegment<object>(_buffer, _end, 0);
            }
            else
            {
                return new ArraySegment<object>(_buffer, 0, _end);
            }
        }

        /// <summary>
        /// Persist buffer data to external repository
        /// </summary>
        private void Persist()
        {
            switch (this._persistenceOption)
            {
                case BufferPersistenceOptions.None:
                    break;
                case BufferPersistenceOptions.XML:
                    PersistToFile();
                    break;
                case BufferPersistenceOptions.SQLServer:
                    PersistToSQLServer();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Load data from persisted repository
        /// </summary>
        /// <returns>Array with previous buffered data</returns>
        private object[] Load()
        {
            switch (this._persistenceOption)
            {
                case BufferPersistenceOptions.None:
                    return new object[] { };
                case BufferPersistenceOptions.XML:
                    return LoadFromFile();
                case BufferPersistenceOptions.SQLServer:
                    return LoadFromSQLServer();
                default:
                    return new object[] { };
            }
        }

        private void PersistToFile()
        {
            using (var stream = File.Open(_id.ToString(), FileMode.OpenOrCreate))
            {
                var serializer = new XmlSerializer(typeof(object[]));
                serializer.Serialize(stream, this.ToArray());
            }
        }

        private object[] LoadFromFile()
        {
            if (File.Exists(_id.ToString()))
            {
                using (var stream = new FileStream(_id.ToString(), FileMode.Open))
                {
                    try
                    {
                        var serializer = new XmlSerializer(typeof(object[]));
                        var persistedBuffer = (object[])serializer.Deserialize(stream);
                        return persistedBuffer;
                    }
                    catch
                    {
                        throw new InvalidDataException("Corrupted Buffer File");
                    }
                }
            }
            else
            {
                return new object[] { };
            }

        }

        private void PersistToSQLServer()
        {
            throw new NotImplementedException();
        }

        private object[] LoadFromSQLServer()
        {
            throw new NotImplementedException();
        }

        public void Serialize()
        {
            throw new NotImplementedException();
        }

        public List<EventData> ToList()
        {
            return null;
        }
        #endregion
    }
}
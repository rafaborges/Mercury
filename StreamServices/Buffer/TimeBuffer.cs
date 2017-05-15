using System;
using System.Collections.Generic;
using StreamServices.Services;
using System.Collections;

namespace StreamServices.Buffer
{
    class TimeBuffer : IBuffer
    {
        public int Capacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsEmpty => throw new NotImplementedException();

        public bool IsFull => throw new NotImplementedException();

        public int Size => throw new NotImplementedException();

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Pop()
        {
            throw new NotImplementedException();
        }

        public void Push(EventData item)
        {
            throw new NotImplementedException();
        }

        public void Serialize()
        {
            throw new NotImplementedException();
        }

        public EventData[] ToArray()
        {
            throw new NotImplementedException();
        }

        public List<EventData> ToList()
        {
            throw new NotImplementedException();
        }
    }
}

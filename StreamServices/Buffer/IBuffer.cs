using StreamServices.Services;
using System.Collections;
using System.Collections.Generic;

namespace StreamServices.Buffer
{
    public interface IBuffer : IEnumerable
    {
        int Capacity { get; set; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        int Size { get; }
        void Pop();
        void Push(EventData item);
        EventData[] ToArray();
        List<EventData> ToList();
        IBufferPersistence BufferPersistence { get; set; }
    }
}
using StreamServices.Services;
using System.Collections;
using System.Collections.Generic;

namespace DataServices.Buffer
{
    public interface IBuffer : IEnumerable
    {
        int Capacity { get; set; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        int Size { get; }
        object Back();
        object Front();
        void PopBack();
        void PopFront();
        void PushBack(object item);
        void PushFront(object item);
        object[] ToArray();
        void Serialize();
        List<EventData> ToList();
    }
}
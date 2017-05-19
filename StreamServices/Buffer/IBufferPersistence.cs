using StreamServices.Services;
using System;
using System.Collections.Generic;

namespace StreamServices.Buffer
{
    public interface IBufferPersistence
    {
        List<EventData> GetAllStoredValues(Guid id);
        void StoreData(EventData data);
        void RemoveData(EventData data);
    }
}

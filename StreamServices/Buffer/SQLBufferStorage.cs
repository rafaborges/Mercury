using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamServices.Services;

namespace StreamServices.Buffer
{
    class SQLBufferStorage : IBufferPersistence
    {
        public List<EventData> GetAllStoredValues(Guid id)
        {
            throw new NotImplementedException();
        }

        public void RemoveData(EventData data)
        {
            throw new NotImplementedException();
        }

        public void StoreData(EventData data)
        {
            throw new NotImplementedException();
        }
    }
}

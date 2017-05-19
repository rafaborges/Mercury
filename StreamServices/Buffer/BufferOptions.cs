using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamServices.Buffer
{
    public enum BufferPersistenceOptions
    {
        None,
        XML,
        SQLServer
    }

    public enum BufferInvalidationType
    {
        Events,
        Time
    }
}
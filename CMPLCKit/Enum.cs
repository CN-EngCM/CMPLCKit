using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMPLCKit
{
    public enum Status
    {
        Opened,
        Closed,
        Error,
        Running,
        Timeout,
        Fail,
        Success
    }

    public enum RegisterType
    {
        R,
        D,
        M,
        DR,
        DD
    }

    public enum DataType
    {
        Float,
        UInt16,
        UInt32,
        Int16,
        Int32,
        String,
        Bool
    }

    public enum DataAccess
    {
        Write,
        Read,
        ReadAndWrite
    }
}

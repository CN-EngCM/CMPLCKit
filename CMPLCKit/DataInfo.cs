using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMPLCKit
{
    public class DataInfo
    {
        public string Name { get; set; }
        public RegisterType RegisterType { get; set; }
        public int Index { get; set; }
        public DataType DataType { get; set; }
        public object Value { get; set; }
        public DataAccess DataAccess { get; set; }
        public bool EnableWrite { get; set; }
        public int CycleTime { get; set; }

    }





}

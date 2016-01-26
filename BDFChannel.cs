using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class BDFChannel
    {
        public BDFChannelHeader Header { get; set; }
        public int[] Data { get; set; }
    }
}

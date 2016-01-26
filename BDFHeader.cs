using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BDFPatcher
{
    class BDFHeader
    {
        public string LocalSubject { get; set; }
        public string LocalRecording { get; set; }
        public DateTime StartDateTime { get; set; }
        public int HeaderByteCount { get; set; }
        public string DataFormat { get; set; }
        public int RecordCount { get; set; }
        public int SecondsPerDataRecord { get; set; }
        public int ChannelCount { get; set; }
    }
}

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
        public BDFChannelHeader[] ChannelHeaders { get; set; }
        public byte[] Reserved { get; set; }

        public bool compatible(BDFHeader header)
        {
            if (!(LocalSubject.Equals(header.LocalSubject)))
                return false;
            if (!(LocalRecording.Equals(header.LocalRecording)))
                return false;
            if (!(DataFormat.Equals(header.DataFormat)))
                return false;
            if (ChannelCount != header.ChannelCount)
                return false;
            for (int i = 0; i < ChannelCount; i++)
            {
                if (!(ChannelHeaders[i].Equals(header.ChannelHeaders[i])))
                    return false;
            }
            return true;
        }

        public static BDFHeader Copy(BDFHeader header)
        {
            BDFHeader res = new BDFHeader();
            res.LocalSubject = String.Copy(header.LocalSubject);
            res.LocalRecording = String.Copy(header.LocalRecording);
            res.StartDateTime = header.StartDateTime;
            res.HeaderByteCount = header.HeaderByteCount;
            res.DataFormat = String.Copy(header.DataFormat);
            res.RecordCount = header.RecordCount;
            res.SecondsPerDataRecord = header.SecondsPerDataRecord;
            res.ChannelCount = header.ChannelCount;
            res.ChannelHeaders = new BDFChannelHeader[res.ChannelCount];
            for (int i = 0; i < res.ChannelCount; i++)
            {
                res.ChannelHeaders[i] = BDFChannelHeader.Copy(header.ChannelHeaders[i]);
            }
            res.Reserved = new byte[header.Reserved.Length];
            Array.Copy(header.Reserved, res.Reserved, header.Reserved.Length);
            return res;
        }

        public bool isHandled()
        {
            bool res = true;
            for (int i = 16; i < Reserved.Length && res; i++)
                res = res && (Reserved[i] == (byte)' ');
            return res;
        }
    }
}

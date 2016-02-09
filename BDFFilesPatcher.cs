using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class BDFFilesPatcher
    {
        private string fileName;
        private BDFHeader header;
        private FileStream stream;
        public BDFFilesPatcher(string fileName)
        {
            this.fileName = fileName;
            stream = new FileStream(fileName, FileMode.Create);
        }

        public void patch(BDFFile file)
        {
            patch(file, 0, file.Header.RecordCount);
        }

        public void patch(BDFFile file, int off, int recordCount)
        {
            if (!(off >= 0 && off + recordCount <= file.Header.RecordCount))
                throw new ArgumentOutOfRangeException("Incorrect intrerval for patching!");
            if (header == null)
            {
                header = BDFHeader.Copy(file.Header);
                header.StartDateTime = header.StartDateTime.AddSeconds((double)off);
                header.RecordCount = -1;
                patchHeader();
            }

            if (!(header.compatible(file.Header)))
                throw new BDFPatchException(String.Format("Cannnot patch file {0} to file {1}", file.FileName, fileName));

            if (header.StartDateTime.AddSeconds((double)recordCountWrote).CompareTo(file.Header.StartDateTime.AddSeconds((double)off)) > 0)
                throw new BDFPatchException("Begin of patching piece belows sooner than end of file");

            TimeSpan timeSpan = file.Header.StartDateTime.AddSeconds((double)off) - header.StartDateTime.AddSeconds((double)recordCountWrote);

            int samplesPerDataRecord = 0;
            for (int i = 0; i < header.ChannelCount; i++)
            {
                samplesPerDataRecord += header.ChannelHeaders[i].SamplesPerDataRecord;
            }

            patchBytes(new byte[samplesPerDataRecord * (int)timeSpan.TotalSeconds * 3]);
            recordCountWrote += (int)timeSpan.TotalSeconds;

            for (int dataRecord = 0; dataRecord < recordCount; dataRecord++)
            {
                for (int channel = 0; channel < header.ChannelCount; channel++)
                {
                    for (int sample = 0; sample < header.ChannelHeaders[channel].SamplesPerDataRecord; sample++)
                    {
                        patchByteInt(file.Channels[channel].Data[off * header.ChannelHeaders[channel].SamplesPerDataRecord + sample], 3);
                    }
                }
            }
            recordCountWrote += recordCount;
        }

        private void patchHeader()
        {
            patchBytes(new byte[1] { 255 });
            patchStr("BIOSEMI", 7);
            patchStr(header.LocalSubject, 80);
            patchStr(header.LocalRecording, 80);
            patchDateTime(header.StartDateTime);
            patchStrInt(header.HeaderByteCount, 8);
            patchStr(header.DataFormat, 44);
            patchStrInt(header.RecordCount, 8);
            patchStrInt(header.SecondsPerDataRecord, 8);
            patchStrInt(header.ChannelCount, 4);
            int channelCount = header.ChannelCount;

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(header.ChannelHeaders[i].Label, 16);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(header.ChannelHeaders[i].TransuderType, 80);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(header.ChannelHeaders[i].Dimension, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(header.ChannelHeaders[i].MinValue, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(header.ChannelHeaders[i].MaxValue, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(header.ChannelHeaders[i].DigitalMin, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(header.ChannelHeaders[i].DigitalMax, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(header.ChannelHeaders[i].Prefiltered, 80);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(header.ChannelHeaders[i].SamplesPerDataRecord, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr("reserved", 32);
            }
        }

        private void patchBytes(byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        private void patchStr(string str, int byteCount)
        {
            str = str.PadRight(byteCount, ' ');
            patchBytes(Encoding.ASCII.GetBytes(str));
        }

        private void patchStrInt(int num, int byteCount)
        {
            patchStr(num.ToString(), byteCount);
        }

        private void patchDateTime(DateTime dateTime)
        {
            patchStr((dateTime.Day % 100).ToString("D2") + '.' +
                     (dateTime.Month % 100).ToString("D2") + '.' +
                     (dateTime.Year % 100).ToString("D2"), 8);

            patchStr((dateTime.Hour % 100).ToString("D2") + '.' +
                     (dateTime.Minute % 100).ToString("D2") + '.' +
                     (dateTime.Second % 100).ToString("D2"), 8);
        }

        private void patchByteInt(int num, int byteCount)
        {
            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = (byte)(num % 256);
                num /= 256;
            }
            patchBytes(bytes);
        }

        private int recordCountWrote;
    }
}

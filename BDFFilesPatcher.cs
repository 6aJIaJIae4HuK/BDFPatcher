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
        private int samplesPerDataRecord = 0;
        private DateTime beginTime;
        private int recordCountWrote;

        public BDFFilesPatcher(string fileName, DateTime beginTime)
        {
            this.fileName = fileName;
            bool exists = false;
            if (File.Exists(fileName))
            {
                exists = true;
                BDFReader reader = new BDFReader(fileName);
                header = reader.readHeader();
                if (header == null)
                    exists = false;
                if (exists)
                {
                    stream = new FileStream(fileName, FileMode.Open);
                    stream.Seek(0, SeekOrigin.End);
                    this.beginTime = header.StartDateTime;
                    recordCountWrote = header.RecordCount;
                }
            }
            if (!exists)
            {
                this.beginTime = beginTime;
                stream = new FileStream(fileName, FileMode.Create);
                recordCountWrote = 0;
            }
        }
        
        public bool tryPatch(BDFFile file, int off, int recordCount)
        {
            bool res;
            try
            {
                patch(file, off, recordCount);
                res = true;
            }
            catch (BDFPatchException e)
            {
                Console.WriteLine(e.Message);
                res = false;
            }
            return res;
        }

        public bool tryPatchEmpty(BDFFile file, int recordCount)
        {
            bool res;
            try
            {
                patchEmpty(file, recordCount);
                res = true;
            }
            catch (BDFPatchException e)
            {
                Console.WriteLine(e.Message);
                res = false;
            }
            return res;
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
                patchHeader(file.Header);
            }

            if (!(header.compatible(file.Header)))
                throw new BDFPatchException(String.Format("Cannnot patch file {0} to file {1}", file.FileName, fileName));

            //if (header.StartDateTime.AddSeconds((double)recordCountWrote).CompareTo(file.Header.StartDateTime.AddSeconds((double)off)) > 0)
            //    throw new BDFPatchException("Begin of patching piece belows sooner than end of file");

            for (int dataRecord = 0; dataRecord < recordCount; dataRecord++)
            {
                for (int channel = 0; channel < header.ChannelCount; channel++)
                {
                    for (int sample = 0; sample < header.ChannelHeaders[channel].SamplesPerDataRecord; sample++)
                    {
                        patchByteInt(file.Channels[channel].Data[dataRecord * header.ChannelHeaders[channel].SamplesPerDataRecord + sample], 3);
                    }
                }
            }
            recordCountWrote += recordCount;
        }

        public void patchEmpty(BDFFile file, int recordCount)
        {
            if (recordCount < 0)
                throw new BDFPatchException("Begin of patching piece belows sooner than end of file");
            if (header == null)
            {
                patchHeader(file.Header);
            }
            patchBytes(new byte[samplesPerDataRecord * recordCount * 3]);
            recordCountWrote += recordCount;
        }

        public void close()
        {
            stream.Seek(8 + 80 + 80 + 8 + 8 + 8 + 44, SeekOrigin.Begin);
            stream.Write(Encoding.ASCII.GetBytes(recordCountWrote.ToString().PadRight(8, ' ')), 0, 8);
            stream.Close();
            stream.Dispose();
        }

        private void patchHeader(BDFHeader header)
        {
            this.header = BDFHeader.Copy(header);
            this.header.StartDateTime = beginTime;
            this.header.RecordCount = -1;
            samplesPerDataRecord = 0;
            for (int i = 0; i < header.ChannelCount; i++)
            {
                samplesPerDataRecord += header.ChannelHeaders[i].SamplesPerDataRecord;
            }

            patchBytes(new byte[1] { 255 });
            patchStr("BIOSEMI", 7);
            patchStr(this.header.LocalSubject, 80);
            patchStr(this.header.LocalRecording, 80);
            patchDateTime(this.header.StartDateTime);
            patchStrInt(this.header.HeaderByteCount, 8);
            patchStr(this.header.DataFormat, 44);
            patchStrInt(this.header.RecordCount, 8);
            patchStrInt(this.header.SecondsPerDataRecord, 8);
            patchStrInt(this.header.ChannelCount, 4);
            int channelCount = this.header.ChannelCount;

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(this.header.ChannelHeaders[i].Label, 16);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(this.header.ChannelHeaders[i].TransuderType, 80);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(this.header.ChannelHeaders[i].Dimension, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(this.header.ChannelHeaders[i].MinValue, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(this.header.ChannelHeaders[i].MaxValue, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(this.header.ChannelHeaders[i].DigitalMin, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(this.header.ChannelHeaders[i].DigitalMax, 8);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStr(this.header.ChannelHeaders[i].Prefiltered, 80);
            }

            for (int i = 0; i < channelCount; i++)
            {
                patchStrInt(this.header.ChannelHeaders[i].SamplesPerDataRecord, 8);
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

        public BDFHeader Header
        {
            get
            {
                return header;
            }
        }
    }
}

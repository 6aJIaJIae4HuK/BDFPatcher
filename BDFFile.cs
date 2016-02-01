using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;

namespace BDFPatcher
{
    class BDFFile : INotifyPropertyChanged
    { 
        public BDFFile()
        {

        }

        private BDFFile(string fileName) //Is this correct??
        {
            readFromFile(fileName);
        }

        public void readFromFile(string path)
        {
            fileName = path;
            try
            {
                reader = new BinaryReader(File.OpenRead(path));
                readHeader();
                readBody();
                reader.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void saveToFile(string path)
        {
            try
            {
                writer = new BinaryWriter(File.OpenWrite(path));
                writeHeader();
                writeBody();
                writer.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void generateFromFiles(List<BDFFile> files)
        {
            try
            {
                if (!areCompatible(files))
                    throw new ArgumentException("File are not compatible!");
                files.Sort((x, y) => DateTime.Compare(x.header.StartDateTime, y.header.StartDateTime));
                header = new BDFHeader();
                header.DataFormat = files.First().header.DataFormat;
                header.ChannelCount = files.First().header.ChannelCount;
                header.SecondsPerDataRecord = files.First().header.SecondsPerDataRecord;
                header.LocalSubject = string.Copy(files.First().header.LocalSubject);
                header.LocalRecording = string.Copy(files.First().header.LocalRecording);
                header.HeaderByteCount = 256 * (header.ChannelCount + 1);
                header.StartDateTime = files.First().header.StartDateTime;
                header.RecordCount = files.Last().header.RecordCount + (int)((files.Last().header.StartDateTime - files.First().header.StartDateTime).TotalSeconds);


                channels = new BDFChannel[header.ChannelCount];
                for (int i = 0; i < channels.Length; i++)
                {
                    channels[i] = new BDFChannel();

                    channels[i].Header = new BDFChannelHeader();
                    channels[i].Header.Label = String.Copy(files.First().channels[i].Header.Label);
                    channels[i].Header.TransuderType = String.Copy(files.First().channels[i].Header.TransuderType);
                    channels[i].Header.Dimension = String.Copy(files.First().channels[i].Header.Dimension);
                    channels[i].Header.MinValue = files.First().channels[i].Header.MinValue;
                    channels[i].Header.MaxValue = files.First().channels[i].Header.MaxValue;
                    channels[i].Header.DigitalMin = files.First().channels[i].Header.DigitalMin;
                    channels[i].Header.DigitalMax = files.First().channels[i].Header.DigitalMax;
                    channels[i].Header.Prefiltered = String.Copy(files.First().channels[i].Header.Prefiltered);
                    channels[i].Header.SamplesPerDataRecord = files.First().channels[i].Header.SamplesPerDataRecord;


                    channels[i].Data = new int[channels[i].Header.SamplesPerDataRecord * header.RecordCount];
                    
                    foreach (BDFFile file in files)
                    {
                        Array.Copy(file.channels[i].Data, 0, channels[i].Data, (int)((file.header.StartDateTime - header.StartDateTime).TotalSeconds) * channels[i].Header.SamplesPerDataRecord, file.channels[i].Data.Length);
                    }
                }
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool areCompatible(List<BDFFile> files)
        {
            if (files == null)
                throw new ArgumentNullException("Array must be initialized!");
            if (!files.Any())
                throw new ArgumentException("Array must be not empty!");
            if (files.First().header.SecondsPerDataRecord != 1) //Think about it.....
                throw new ArgumentException("Every file must have 1 second in every data record!");
            foreach (BDFFile file in files)
            {
                if (file == null)
                    throw new ArgumentNullException("All files bust be initialized!");
                if (!files.First().header.DataFormat.Equals(file.header.DataFormat))
                    return false;
                if (files.First().header.SecondsPerDataRecord != file.header.SecondsPerDataRecord)
                    return false;
                if (files.First().header.ChannelCount != file.header.ChannelCount)
                    return false;
                for (int i = 0; i < files[0].header.ChannelCount; i++)
                {
                    if (!files.First().channels[i].Header.Equals(file.channels[i].Header))
                        return false;
                }
            }
            return true;
        }

        private void readHeader()
        {
            try
            {
                header = new BDFHeader();
                byte[] bytes = readBytes(8);
                if (bytes.Length != 8 || bytes[0] != 255 || !Encoding.ASCII.GetString(bytes.Skip(1).ToArray()).Equals("BIOSEMI"))
                    throw new Exception("Not BDFFile!");
                header.LocalSubject = readStr(80);
                header.LocalRecording = readStr(80);
                header.StartDateTime = readDateTime();
                header.HeaderByteCount = readStrInt(8);
                header.DataFormat = readStr(44);
                if (!header.DataFormat.Equals("24BIT"))
                    throw new Exception("Incorrect data format!");
                header.RecordCount = readStrInt(8);
                header.SecondsPerDataRecord = readStrInt(8);
                header.ChannelCount = readStrInt(4);

                channels = new BDFChannel[header.ChannelCount];

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i] = new BDFChannel();
                    channels[i].Header = new BDFChannelHeader();
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.Label = readStr(16);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.TransuderType = readStr(80);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.Dimension = readStr(8);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.MinValue = readStrInt(8);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.MaxValue = readStrInt(8);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.DigitalMin = readStrInt(8);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.DigitalMax = readStrInt(8);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.Prefiltered = readStr(80);
                }

                for (int i = 0; i < header.ChannelCount; i++)
                {
                    channels[i].Header.SamplesPerDataRecord = readStrInt(8);
                }

                reader.ReadBytes(32 * header.ChannelCount);

                if (header.RecordCount == -1)
                {
                    int bytesPerDataRecord = 0;
                    for (int i = 0; i < header.ChannelCount; i++)
                    {
                        bytesPerDataRecord += (channels[i].Header.SamplesPerDataRecord * 3);
                    }

                    header.RecordCount = ((int)(new FileInfo(fileName)).Length - header.HeaderByteCount) / bytesPerDataRecord;
                }
                
                Size = header.RecordCount;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void readBody()
        {
            for (int i = 0; i < header.ChannelCount; i++)
            {
                channels[i].Data = new int[channels[i].Header.SamplesPerDataRecord * header.RecordCount];
            }

            for (int i = 0; i < header.RecordCount; i++)
            {
                for (int j = 0; j < header.ChannelCount; j++)
                {
                    for (int k = 0; k < channels[j].Header.SamplesPerDataRecord; k++)
                    {
                        channels[j].Data[i * channels[j].Header.SamplesPerDataRecord + k] = readByteInt(3);
                    }
                }
                Read++;
            }
        }

        private void writeHeader()
        {
            writeBytes(new byte[1] { 255 });
            writeStr("BIOSEMI", 7);
            writeStr(header.LocalSubject, 80);
            writeStr(header.LocalRecording, 80);
            writeDateTime(header.StartDateTime);
            writeStrInt(header.HeaderByteCount, 8);
            writeStr(header.DataFormat, 44);
            writeStrInt(header.RecordCount, 8);
            writeStrInt(header.SecondsPerDataRecord, 8);
            writeStrInt(header.ChannelCount, 4);

            Size = header.RecordCount;

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStr(channels[i].Header.Label, 16);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStr(channels[i].Header.TransuderType, 80);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStr(channels[i].Header.Dimension, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStrInt(channels[i].Header.MinValue, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStrInt(channels[i].Header.MaxValue, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStrInt(channels[i].Header.DigitalMin, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStrInt(channels[i].Header.DigitalMax, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStr(channels[i].Header.Prefiltered, 80);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStrInt(channels[i].Header.SamplesPerDataRecord, 8);
            }

            for (int i = 0; i < header.ChannelCount; i++)
            {
                writeStr("reserved", 32);
            }
        }

        private void writeBody()
        {
            for (int i = 0; i < header.RecordCount; i++)
            {
                for (int j = 0; j < header.ChannelCount; j++)
                {
                    for (int k = 0; k < channels[j].Header.SamplesPerDataRecord; k++)
                    {
                        writeByteInt(channels[j].Data[i * channels[j].Header.SamplesPerDataRecord + k], 3);
                    }
                }
                Wrote++;
            }
        }

        private byte[] readBytes(int byteCount)
        {
            byte[] res = reader.ReadBytes(byteCount);
            if (res.Length < byteCount)
                throw new IOException("Unexpected end of file");
            return res;
        }

        private void writeBytes(byte[] bytes)
        {
            writer.Write(bytes);
        }

        private string readStr(int byteCount)
        {
            string res = "";
            try
            {
                byte[] buffer = reader.ReadBytes(byteCount);
                res = Encoding.ASCII.GetString(buffer).Trim();
            }
            catch (IOException e)
            {
                throw e;
            }
            return res;
        }

        private void writeStr(string str, int byteCount)
        {
            str = str.PadRight(byteCount, ' ');
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            writeBytes(bytes);
        }

        private int readStrInt(int byteCount)
        {
            int res;
            try
            {
                string str = readStr(byteCount);
                res = Int32.Parse(str);
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (FormatException e)
            {
                throw e;
            }
            return res;
        }

        private void writeStrInt(int num, int byteCount)
        {
            writeStr(num.ToString(), byteCount);
        }

        private DateTime readDateTime()
        {
            DateTime res;
            try
            {
                int[] nums = new int[6];
                for (int i = 0; i < 2; i++)
                {
                    string str = readStr(8);
                    if (str[2] != '.' || str[5] != '.')
                        throw new Exception("Incorrect datetime format!");
                    for (int j = 0; j < 3; j++)
                    {
                        nums[3 * i + j] = (str[3 * j + 0] - '0') * 10 + (str[3 * j + 1] - '0');
                    }
                }
                nums[2] += 2000;
                res = new DateTime(nums[2], nums[1], nums[0], nums[3], nums[4], nums[5]);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        private void writeDateTime(DateTime dateTime)
        {
            writeStr((dateTime.Day % 100).ToString("D2") + '.' + 
                     (dateTime.Month % 100).ToString("D2") + '.' +
                     (dateTime.Year % 100).ToString("D2"), 8);

            writeStr((dateTime.Hour % 100).ToString("D2") + '.' +
                     (dateTime.Minute % 100).ToString("D2") + '.' +
                     (dateTime.Second % 100).ToString("D2"), 8);
        }

        private int readByteInt(int byteCount)
        {
            int res = 0;
            try
            {
                if (byteCount > 4)
                    throw new ArgumentException("Illegal byte count!");
                byte[] buffer = reader.ReadBytes(byteCount);
                res = buffer[byteCount - 1];
                for (int i = 0; i < byteCount - 1; i++)
                {
                    res = (buffer[byteCount - (i + 2)] + 256 * res);
                }
            }
            catch (IOException e)
            {
                throw e;
            }
            return res;
        }

        private void writeByteInt(int num, int byteCount)
        {
            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = (byte)(num % 256);
                num /= 256;
            }
            writeBytes(bytes);
        }

        private BDFChannel[] channels;
        private BinaryReader reader;
        private BinaryWriter writer;
        private string fileName;

        public event PropertyChangedEventHandler PropertyChanged;

        private BDFHeader header;
        public BDFHeader Header
        {
            get
            {
                return header;
            }
        }


        private int _size;
        public int Size
        {
            get
            {
                return _size;
            }

            private set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        private int _curRead;
        public int Read
        {
            get
            {
                return _curRead;
            }

            private set
            {
                _curRead = value;
                OnPropertyChanged("Read");
            }
        }

        private int _curWrote;
        public int Wrote
        {
            get
            {
                return _curWrote;
            }
            private set
            {
                _curWrote = value;
                OnPropertyChanged("Wrote");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

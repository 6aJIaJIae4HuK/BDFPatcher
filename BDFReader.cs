using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class BDFReader
    {
        public BDFReader(string fileName)
        {
            this.fileName = fileName;
        }

        public BDFHeader readHeader()
        {
            BDFFile file = new BDFFile(fileName);
            BDFHeader header = null;
            try
            {
                header = file.Header;
            }
            catch (BDFHeaderReadException e)
            {
                Exception ex = e as Exception;
                Console.WriteLine();
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                Console.WriteLine();
                return null;
            }
            finally
            {
                file.Close();
            }
            return BDFHeader.Copy(header);
        }

        public bool readFile()
        {
            file = new BDFFile(fileName);
            return file.tryReadFromFile(fileName);
        }

        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
        }
        
        private BDFFile file;
        public BDFFile File
        {
            get
            {
                return file;
            }
        }
    }
}

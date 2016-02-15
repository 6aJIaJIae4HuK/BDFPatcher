using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    class MainProgram
    {
        public const string iniFileName = "ini.ini";

        private string sourcePath;
        private string targetPath;

        private HashSet<string> handledFiles = new HashSet<string>();
        
        public static void Main(string[] argv)
        {
            MainProgram app = new MainProgram();
            app.run();
        }

        public void run()
        {
            try
            {
                initHandledFiles();
                initPaths();
                string[] files = System.IO.Directory.GetFiles(sourcePath, "*_*_*_cmpltd.bdf");
                
                Dictionary<string, List<string>> patients = new Dictionary<string, List<string>>();

                foreach (string file in files)
                {
                    string fileName = file.Substring(file.LastIndexOf("\\") + 1);
                    string tmp = fileName.Substring(0, fileName.LastIndexOf("_cmpltd.bdf"));
                    string patientName = tmp.Substring(tmp.LastIndexOf('_') + 1);
                    if (!patients.ContainsKey(patientName))
                    {
                        patients.Add(patientName, new List<string>());
                    }
                    patients[patientName].Add(file);
                }

                foreach (var entry in patients)
                {
                    System.Console.WriteLine("{0}:", entry.Key);
                    foreach (var file in entry.Value)
                    {
                        System.Console.WriteLine("\t{0}", file);
                    }
                }

                foreach (string patient in patients.Keys)
                {
                    List<KeyValuePair<BDFHeader, string>> headers = new List<KeyValuePair<BDFHeader, string>>();

                    foreach (string fileName in patients[patient])
                    {
                        if (handledFiles.Contains(fileName))
                        {
                            System.Console.WriteLine("{0} has been handled", fileName);
                            continue;
                        }
                        BDFReader reader = new BDFReader(fileName);
                        headers.Add(new KeyValuePair<BDFHeader, string>(reader.readHeader(), fileName));
                    }

                    headers.RemoveAll((x) => x.Key == null);
                    headers.Sort((x, y) => x.Key.StartDateTime.CompareTo(y.Key.StartDateTime));

                    if (!System.IO.Directory.Exists(targetPath + patient))
                        System.IO.Directory.CreateDirectory(targetPath + patient);

                    DateTime cur = headers.First().Key.StartDateTime;
                    DateTime pos = cur;
                    BDFFilesPatcher patcher = null;
                    {
                        string name = targetPath + patient + '\\' + generateFileName(pos, patient);
                        Console.WriteLine(String.Format("Writing to file {0}", name));
                        patcher = new BDFFilesPatcher(name, cur);
                    }

                    foreach (KeyValuePair<BDFHeader, string> header in headers)
                    {
                        BDFReader reader = new BDFReader(header.Value);
                        Console.WriteLine(String.Format("Reading file: {0}", header.Value));
                        if (!reader.readFile())
                        {
                            Console.WriteLine();
                            Console.WriteLine(String.Format("FAILED TO READ FILE: {0}", header.Value));
                            Console.WriteLine();
                            continue;
                        }

                        if (reader.File.Header.StartDateTime.CompareTo(pos) < 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine(String.Format("File {0} begins at time {1}, while data already recorded to {2}", header.Value, reader.File.Header.StartDateTime, pos));
                            Console.WriteLine();
                            continue;
                        }

                        while (header.Key.StartDateTime.CompareTo(cur.AddDays(1.0)) >= 0)
                        {
                            cur = cur.AddDays(1.0);
                            patcher.patchEmpty(reader.File, (int)(cur - pos).TotalSeconds);
                            pos = cur;
                            patcher.close();
                            string name = targetPath + patient + '\\' + generateFileName(pos, patient);
                            Console.WriteLine(String.Format("Writing to file {0}", name));
                            patcher = new BDFFilesPatcher(name, cur);
                        }

                        patcher.patchEmpty(reader.File, (int)(header.Key.StartDateTime - pos).TotalSeconds);

                        pos = header.Key.StartDateTime;

                        DateTime end = header.Key.StartDateTime.AddSeconds(header.Key.RecordCount);

                        while (end.CompareTo(cur.AddDays(1.0)) > 0)
                        {
                            patcher.patch(reader.File, (int)(pos - header.Key.StartDateTime).TotalSeconds, (int)(cur.AddDays(1.0) - pos).TotalSeconds);
                            cur = cur.AddDays(1.0);
                            pos = cur;
                            patcher.close();
                            string name = targetPath + patient + '\\' + generateFileName(pos, patient);
                            Console.WriteLine(String.Format("Writing to file {0}", name));
                            patcher = new BDFFilesPatcher(name, cur);
                        }

                        patcher.patch(reader.File, (int)(pos - header.Key.StartDateTime).TotalSeconds, (int)(end - pos).TotalSeconds);
                        pos = end;

                        reader = null;

                        handledFiles.Add(header.Value);

                    }

                    patcher.close();
                }

                System.Console.WriteLine("Success!");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }


            saveHandledFiles();
        }

        private void initHandledFiles()
        {
            //geage
        }

        private void saveHandledFiles()
        {
            //defefa

        }

        private void initPaths()
        {

            string[] lines = null;
            try
            {
                lines = System.IO.File.ReadAllLines(iniFileName);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (lines.Length != 2)
            {
                throw new Exception("Incorrect iniFile");
            }

            try
            {
                sourcePath = getPathArgument("SOURCE_PATH", lines[0]);
                targetPath = getPathArgument("TARGET_PATH", lines[1]);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                throw e;
            }
        }

        private string getPathArgument(string key, string str)
        {
            if (str.IndexOf(key + '=') != 0)
            {
                throw new Exception("Incorrect iniFile");
            }
            else
            {
                str = str.Substring((key + '=').Length);
                if (!System.IO.Directory.Exists(str))
                {
                    System.IO.Directory.CreateDirectory(str);
                }
                return str;
            }
        }

        private string generateFileName(DateTime dateTime, string patientName)
        {
            string res = "";

            res += (dateTime.Year % 100).ToString("D2");
            res += dateTime.Month.ToString("D2");
            res += dateTime.Day.ToString("D2");

            res += '_';

            res += dateTime.Hour.ToString("D2");
            res += dateTime.Minute.ToString("D2");
            res += dateTime.Second.ToString("D2");

            res += '_';

            res += patientName;

            res += "_cmpltd.bdf";

            return res;
        }

        /*
        private void saveSubList(string patientName, List<BDFFile> files)
        {
            BDFFile newFile = new BDFFile();
            bool status;
            status = newFile.tryGenerateFromFiles(files);
            
            if (!status)
            {
                System.Console.WriteLine("Cannot patch following files: ");
                foreach (BDFFile file in files)
                {
                    System.Console.WriteLine("\t" + file.FileName);
                }
            }

            string newFileFullPath = targetPath + patientName + '\\' + generateFileName(newFile.Header.StartDateTime, patientName);

            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(newFileFullPath);
                fileInfo.Directory.Create();
                newFile.saveToFile(fileInfo.FullName);
            }
            catch (BDFSaveException)
            {
                System.Console.WriteLine("Cannot save patched file " + newFile.FileName);
            }
        }
        */
    }
}
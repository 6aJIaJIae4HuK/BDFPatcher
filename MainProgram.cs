﻿using System;
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
        
        public static void Main(string[] argv)
        {
            MainProgram app = new MainProgram();
            app.run();
        }

        public void run()
        {
            try
            {
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
                        BDFReader reader = new BDFReader(fileName);
                        BDFHeader header = reader.readHeader();
                        if (header == null)
                            continue;
                        if (header.isHandled())
                        {
                            System.Console.WriteLine("{0} is already handled", fileName);
                            continue;
                        }
                        headers.Add(new KeyValuePair<BDFHeader, string>(header, fileName));
                    }

                    if (!headers.Any())
                        continue;
                    headers.Sort((x, y) => x.Key.StartDateTime.CompareTo(y.Key.StartDateTime));
                    
                    /*
                    using (StreamWriter s = new StreamWriter(@"test.txt", false))
                    {
                        foreach (var header in headers)
                        {
                            s.WriteLine(header.Key.StartDateTime.ToString("dd-MM-yyyy HH:mm:ss") + " — " + header.Key.RecordCount + " с");
                        }
                    }
                    */

                    if (!System.IO.Directory.Exists(targetPath + patient))
                        System.IO.Directory.CreateDirectory(targetPath + patient);

                    DateTime cur = headers.First().Key.StartDateTime;
                    DateTime pos = cur;

                    string[] generatedFiles = System.IO.Directory.GetFiles(targetPath + patient + '\\');
                    List<KeyValuePair<BDFHeader, string>> generatedHeaders = new List<KeyValuePair<BDFHeader, string>>();

                    foreach (string file in generatedFiles)
                    {
                        BDFReader reader = new BDFReader(file);
                        BDFHeader header = reader.readHeader();
                        if (header != null)
                            generatedHeaders.Add(new KeyValuePair<BDFHeader,string>(header, file));
                    }

                    generatedHeaders.Sort((x, y) => x.Key.StartDateTime.CompareTo(y.Key.StartDateTime));


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

                        if (generatedHeaders.Any())
                        {
                            pos = generatedHeaders.Last().Key.StartDateTime.AddSeconds(generatedHeaders.Last().Key.RecordCount);
                            cur = generatedHeaders.Last().Key.StartDateTime;
                        }
                        else
                        {
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(null, targetPath + patient + '\\' + generateFileName(pos, patient)));
                        }

                        //<Stupid piece of shit>
                        if (reader.File.Header.StartDateTime.CompareTo(pos) < 0)
                        {
                            reader.File.setStartDate(pos);
                            //Console.WriteLine();
                            //Console.WriteLine(String.Format("File {0} begins at time {1}, while data already recorded to {2}", header.Value, reader.File.Header.StartDateTime, pos));
                            //Console.WriteLine();
                            //continue;
                        }

                        else
                        
                        if ((reader.File.Header.StartDateTime - pos).TotalSeconds <= 10)
                        {
                            reader.File.setStartDate(pos);
                        }

                        //</Stupid piece of shit>

                        if (!(generatedHeaders.Last().Key == null || generatedHeaders.Last().Key.compatible(reader.File.Header)))
                        {
                            cur = pos = reader.File.Header.StartDateTime;
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(null, targetPath + patient + '\\' + generateFileName(pos, patient)));
                        }

                        while (reader.File.Header.StartDateTime.CompareTo(cur.AddDays(1.0)) >= 0)
                        {
                            BDFFilesPatcher patcher = new BDFFilesPatcher(generatedHeaders.Last().Value, cur);
                            cur = cur.AddDays(1.0);
                            patcher.patchEmpty(reader.File, (int)(cur - pos).TotalSeconds);
                            pos = cur;
                            patcher.close();
                            string name = generatedHeaders.Last().Value;
                            generatedHeaders.Remove(generatedHeaders.Last());
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>((new BDFReader(name)).readHeader(), name));
                            //generatedHeaders.Add(new KeyValuePair<BDFHeader,string>(patcher.Header, name));

                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(null, targetPath + patient + '\\' + generateFileName(pos, patient)));
                        }

                        {
                            BDFFilesPatcher patcher = new BDFFilesPatcher(generatedHeaders.Last().Value, cur);
                            patcher.patchEmpty(reader.File, (int)(reader.File.Header.StartDateTime - pos).TotalSeconds);
                            patcher.close();
                            string name = generatedHeaders.Last().Value;
                            generatedHeaders.Remove(generatedHeaders.Last());
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>((new BDFReader(name)).readHeader(), name));
                            //generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(patcher.Header, name));
                            pos = reader.File.Header.StartDateTime;
                        }

                        DateTime end = reader.File.Header.StartDateTime.AddSeconds(header.Key.RecordCount);

                        while (end.CompareTo(cur.AddDays(1.0)) > 0)
                        {

                            BDFFilesPatcher patcher = new BDFFilesPatcher(generatedHeaders.Last().Value, cur);
                            patcher.patch(reader.File, (int)(pos - reader.File.Header.StartDateTime).TotalSeconds, (int)(cur.AddDays(1.0) - pos).TotalSeconds);
                            cur = cur.AddDays(1.0);
                            pos = cur;
                            patcher.close();
                            string name = generatedHeaders.Last().Value;
                            generatedHeaders.Remove(generatedHeaders.Last());
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>((new BDFReader(name)).readHeader(), name));
                            //generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(patcher.Header, name));

                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(null, targetPath + patient + '\\' + generateFileName(pos, patient)));
                        }

                        {
                            BDFFilesPatcher patcher = new BDFFilesPatcher(generatedHeaders.Last().Value, cur);
                            patcher.patch(reader.File, (int)(pos - reader.File.Header.StartDateTime).TotalSeconds, (int)(end - pos).TotalSeconds);
                            patcher.close();
                            string name = generatedHeaders.Last().Value;
                            generatedHeaders.Remove(generatedHeaders.Last());
                            generatedHeaders.Add(new KeyValuePair<BDFHeader, string>((new BDFReader(name)).readHeader(), name));
                            //generatedHeaders.Add(new KeyValuePair<BDFHeader, string>(patcher.Header, name));
                            pos = end;
                        }


                        reader.File.markAsHandled();

                        reader = null;
                    }
                }

                System.Console.WriteLine("Success!");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
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
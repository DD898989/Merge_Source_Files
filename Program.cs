using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace ConsoleApplication12
{
    class Program
    {
        static bool b_Merge_Extract = true;
        
        static string sourcePath = @"C:\Users\dave.gan\Desktop\DP";

        static int nSkipPrefix = 0;//如果是4  就是多拿掉"\MPI"這四個字   原本   "C:\Users\dave.gan\Desktop\DP\MPIGui" 變成 "Gui"

        static string[] extensions = { 
                                     //"h",
                                     "cs",
                                     "cpp",
                                     //"trss",
                                     //"cxx",
                                     //"hxx",
                                     //"xsl",
                                     //"xml",
                                     //"sln",
                                     //"xslt",
                                     //"csproj",
                                     //"lib",
                                     //"txt", 
                                };




        static void Main(string[] args)
        {
            if (b_Merge_Extract)
            {
                //------------------------------------------------------------------------------------
                List<string> seperateFiles = new List<string>();

                foreach (string ext in extensions)
                    seperateFiles.Add(sourcePath + "\\" + ext + "_GAN_MERGE");
                //------------------------------------------------------------------------------------
                foreach (string ext in extensions)
                {
                    string writeFile = sourcePath + "\\" + ext + "_GAN_MERGE";

                    if (System.IO.File.Exists(writeFile))
                        System.IO.File.Delete(writeFile);

                    using (StreamWriter writer = new StreamWriter(writeFile))
                    {
                        foreach (string everyFile in GetFiles(sourcePath))
                        {
                            string ext_ = everyFile.Substring(everyFile.Length - ext.Length - 1);
                            if (string.Compare("." + ext, ext_, true) == 0)
                            {
                                string fullPath = everyFile.Substring(sourcePath.Length + nSkipPrefix);

                                if (
                                    fullPath.Contains(@"#")
                                    ||
                                    fullPath.Contains(@" - Copy")
                                    ||
                                    fullPath.Contains(@"\Copy of ")
                                    )
                                {

                                }
                                else
                                {
                                    int lineNum = 0;
                                    string line;

                                    System.IO.StreamReader sr = new System.IO.StreamReader(everyFile,Encoding.Default);
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        line = fullPath + "【" + ++lineNum + "】" + line + "\n";
                                        writer.Write(line);
                                    }
                                    writer.Write("XXX.XXX【0】 //*/" + "\n" + "XXX.XXX【0】 }" + "\n" + "XXX.XXX【0】 {" + "\n");

                                    sr.Close();
                                }
                                System.Console.Write("*");
                            }
                        }
                        writer.Close();
                    }
                }
                //------------------------------------------------------------------------------------
                const int chunkSize = 2 * 1024; // 2KB

                using (var output = File.Create(sourcePath + "\\GAN_MERGE_ALL"))
                {
                    foreach (var file in seperateFiles)
                    {
                        using (var input = File.OpenRead(file))
                        {
                            var buffer = new byte[chunkSize];
                            int bytesRead;
                            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                //------------------------------------------------------------------------------------
                foreach (string ext in extensions)
                {
                    if (System.IO.File.Exists(sourcePath + "\\" + ext + "_GAN_MERGE"))
                    {
                        System.IO.File.Delete(sourcePath + "\\" + ext + "_GAN_MERGE");
                    }
                }
                //------------------------------------------------------------------------------------
            }
            else
            {
                string line;
                List<string> st = new List<string> { };
                System.IO.StreamReader sr = new System.IO.StreamReader(sourcePath + "\\GAN_MERGE_ALL");

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("Johnson"))
                    {
                        st.Add(line);
                    }
                }


                using (StreamWriter writer = new StreamWriter(sourcePath + "\\GAN_MERGE_ALL_EXTRACT"))
                {
                    foreach(string temp in st)
                        writer.WriteLine(temp);
                }
            }
        }

        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------

        static IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int f = 0; f < files.Length; f++)
                    {
                        yield return files[f];
                    }
                }
            }
        }
    }
}

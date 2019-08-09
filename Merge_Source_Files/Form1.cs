using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace Merge_Source_Files
{
    public partial class Form1 : Form
    {
        string iniFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini";
        public Form1()
        {
            InitializeComponent();

            if (File.Exists(iniFile))
            {
                using (StreamReader sr = new StreamReader(iniFile))
                {
                    while (sr.Peek() >= 0)
                    {
                        string file = sr.ReadLine();
                        if (file.Length > 0)
                        {
                            this.textBox2.Items.Add(file);
                        }
                    }
                }
            }
            else
            {
                File.Create(iniFile);
            }
        }

        static bool b_Merge_Extract = false;

        static string sourcePath = @"";

        static int nSkipPrefix = 0;

        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string fileName in files)
            {
                textBox2.Items.Add(fileName);
            }
        }


        public string Merge(string sourcePath)
        {


            b_Merge_Extract = this.textBox3.Text.Length > 0;

            nSkipPrefix = (int)this.numericUpDown1.Value;

            string[] extensions = this.textBox1.Text.Split(
            new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
            );


            //string[] extensions = { 
            //                     //"h",
            //                     "cs",
            //                     "cpp",
            //                     //"trss",
            //                     //"cxx",
            //                     //"hxx",
            //                     //"xsl",
            //                     //"xml",
            //                     //"sln",
            //                     //"xslt",
            //                     //"csproj",
            //                     //"lib",
            //                     //"txt", 
            //                };

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
                                //(
                                //    !fullPath.Contains(@"C#")
                                //    &&
                                //    fullPath.Contains(@"#")
                                //)
                                //||
                                //fullPath.Contains(@" - Copy")
                                //||
                                //fullPath.Contains(@"\Copy of ")
                                false
                                )
                            {

                            }
                            else
                            {
                                int lineNum = 0;
                                string line;

                                System.IO.StreamReader sr = new System.IO.StreamReader(everyFile, Encoding.Default);


                                bool isWrite = false;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    ++lineNum;

                                    if (!b_Merge_Extract)
                                    {
                                        line = fullPath + "【" + lineNum + "】" + line + "\n";
                                        writer.Write(line);
                                        isWrite = true;
                                    }
                                    else
                                    {
                                        if (Regex.Match(line, this.textBox3.Text).Success)
                                        {
                                            line = fullPath + "【" + lineNum + "】" + line + "\n";
                                            writer.Write(line);
                                            isWrite = true;
                                        }
                                    }
                                }

                                if (isWrite)
                                {
                                    FileInfo f = new FileInfo(everyFile);
                                    writer.WriteLine("XXX.XXX【0】 //*/");
                                    writer.WriteLine("XXX.XXX【0】 }");
                                    writer.WriteLine("XXX.XXX【0】 {");
                                    writer.WriteLine("XXX.XXX【0】 " + "File         :" + f.ToString());
                                    writer.WriteLine("XXX.XXX【0】 " + "CreationTime :" + f.CreationTime.ToString("yyyy/MM/dd HH:mm:ss"));
                                    writer.WriteLine("XXX.XXX【0】 " + "LastWriteTime:" + f.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
                                }


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

            string temp;
            if (!b_Merge_Extract)
                temp = "\\GAN_MERGE_ALL";
            else
                temp = "\\GAN_MERGE_ALL_EXTRACT";


            using (var output = File.Create(sourcePath + temp))
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

            string fileName = sourcePath + temp;
            return fileName;
            //------------------------------------------------------------------------------------

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> openFils = new List<string> { };

            foreach(string paths in this.textBox2.Items)
            { 
                openFils.Add(Merge(paths));
            }

            foreach (string fileName in openFils)
            {
                System.Diagnostics.Process.Start("notepad++.exe", "\"" + fileName + "\"");
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

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                ListBox lb = (ListBox)sender;
                if (e.KeyCode == Keys.Delete)
                {
                    lb.Items.RemoveAt(lb.SelectedIndex);
                    lb.SelectedIndex = 0;
                }

            }
            catch
            {

            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            System.Diagnostics.Process.Start(iniFile);
        }
    }
}

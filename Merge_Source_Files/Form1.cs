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
                            if (file.Contains(":") || file.Contains("\\"))
                            {
                                this.listBox1.Items.Add(file);
                            }
                            else
                            {
                                this.textBox1.Text += Environment.NewLine;
                                this.textBox1.Text += file;
                            }
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
                listBox1.Items.Add(fileName);
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

                using (StreamWriter writer = new StreamWriter(writeFile))//,false,Encoding.UTF8))
                {
                    foreach (string everyFile in GetFiles(sourcePath))
                    {
                        if (everyFile.ToLower().EndsWith(ext))
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


                                string exact = "";
                                if (this.checkBox2.Checked)
                                {
                                    exact = @"\b";
                                }

                                RegexOptions option = RegexOptions.None;
                                if (this.checkBox1.Checked)
                                {
                                    option = RegexOptions.IgnoreCase;
                                }

                                Encoding encode = Encoding.Default;
                                if (this.checkBox3.Checked)
                                {
                                    encode = Encoding.UTF8;
                                }






                                System.IO.StreamReader sr = new System.IO.StreamReader(everyFile, encode);


                                bool isWrite = false;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    bool isWriteLine = false;
                                    ++lineNum;

                                    if (!b_Merge_Extract)
                                    {
                                        isWriteLine = true;
                                        isWrite = true;
                                    }
                                    else
                                    {
                                        if (checkBox4.Checked)
                                        {
                                            if (Regex.Match(line, exact + this.textBox3.Text + exact, option).Success)
                                            {
                                                isWriteLine = true;
                                                isWrite = true;
                                            }
                                        }
                                        else
                                        {
                                            if (option == RegexOptions.IgnoreCase)
                                            {
                                                if (line.ToLower().Contains(this.textBox3.Text.ToLower()))
                                                {
                                                    isWriteLine = true;
                                                    isWrite = true;
                                                }
                                            }
                                            else
                                            {
                                                if (line.Contains(this.textBox3.Text))
                                                {
                                                    isWriteLine = true;
                                                    isWrite = true;
                                                }
                                            }
                                        }
                                    }

                                    if (isWriteLine)
                                    {
                                        line = fullPath + "【" + lineNum + "】" + line + "\n";
                                        writer.Write(line);
                                        isWrite = true;
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

            foreach (string paths in this.listBox1.Items)
            {
                try
                {
                    openFils.Add(Merge(paths));
                }
                catch (Exception excp)
                {
                    MessageBox.Show(excp.Message);
                    return;
                }
            }

            var notepadpp = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
            if (File.Exists(notepadpp))
            {
                foreach (string fileName in openFils)
                {
                    System.Diagnostics.Process.Start(notepadpp, "\"" + fileName + "\"");
                }
            }
            else
            {
                throw new Exception("沒有找到"+ notepadpp);
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

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string fileName in files)
            {
                listBox1.Items.Add(fileName);
            }
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

        private void btn_Add_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.listBox1.Items.Add(folderBrowserDialog1.SelectedPath);
            }
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            Dictionary<string, int> check_dic = new Dictionary<string, int>();

            foreach (string paths in this.listBox1.Items)
            {
                foreach (string everyFile in GetFiles(paths))
                {
                    string key;

                    key = Path.GetExtension(everyFile);
                    if (key == "")
                    {
                        key = Path.GetFileName(everyFile);
                    }


                    if (check_dic.ContainsKey(key))
                    {
                        check_dic[key]++;
                    }
                    else
                    {
                        check_dic[key] = 1;
                    }
                }
            }

            textBox4.Text = "";
            foreach (var pair in check_dic.OrderByDescending(p => p.Value))
            {
                if (pair.Value <= 1)
                    break;

                textBox4.Text += pair.Value;
                textBox4.Text += ":";
                textBox4.Text += pair.Key;
                textBox4.Text += "\r\n";
            }


        }
    }
}

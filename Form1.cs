using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest_1(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectFolderDialog.ShowDialog();
            
            var path = SelectFolderDialog.SelectedPath;
            SelectFolderText.Text = path;

            TryEnableApplyButton();
        }

        private void SelectFolderText_TextChanged(object sender, EventArgs e)
        {

        }

        private void OutputFileButton_Click(object sender, EventArgs e)
        {
            OutputFileDialog.AddExtension = true;
            OutputFileDialog.DefaultExt = "iso";
            OutputFileDialog.FileName = "patched.iso";
            OutputFileDialog.Filter = "ISO file (*.iso)|*.iso)";

            OutputFileDialog.ShowDialog();

            var path = OutputFileDialog.FileName;
            OutputFileText.Text = path;

            TryEnableApplyButton(); 
        }

        private void TryEnableApplyButton()
        {
            var input = SelectFolderText.Text; 
            var output = OutputFileText.Text;
            var patchData = PatchFileData.Text;

            var ready = !string.IsNullOrEmpty(input) 
                && !string.IsNullOrEmpty(output) 
                && !string.IsNullOrEmpty(patchData);

            ApplyPatchButton.Enabled = ready; 
        }

        private void PatchProgressBar_Click(object sender, EventArgs e)
        {

        }

        private void ApplyPatchButton_Click(object sender, EventArgs e)
        {
            var inputPath = SelectFolderText.Text;
            var outputPath = OutputFileText.Text;
            var patchData = PatchFileData.Text;

            // skip first line, its the header info 
            PatchProgressBar.Minimum = 0;
            PatchProgressBar.Maximum = patchData.Split('\n').Length * AddFileCounter(inputPath);

            OutputFileButton.Enabled = false;
            SelectFolderButton.Enabled = false; 
            ApplyPatchButton.Enabled = false;

            PatchProgressBar.Value = 0; 

            var thread = new System.Threading.Thread(() =>
            {
                Log("Starting..");
                DoApplyPatch(inputPath, outputPath, patchData);
                Log("Finished.");
                
                // run on main thread 
                this.Invoke(new Action(() =>
                {
                    ApplyPatchButton.Enabled = true;
                    OutputFileButton.Enabled = true;
                    SelectFolderButton.Enabled = true;
                    PatchProgressBar.Value = PatchProgressBar.Maximum; 
                }));
            });

            thread.Start();
        }

        private void SelectPatchFileButton_Click(object sender, EventArgs e)
        {
            SelectPatchFileDialog.ShowDialog();

            var patchFilePath = SelectPatchFileDialog.FileName;
            var patchData = System.IO.File.ReadAllText(patchFilePath);
            PatchFileData.Text = patchData;

            TryEnableApplyButton(); 
        }
        
        private void DoApplyPatch(string inputFolder, string outputFile, string patchData)
        {

            // skip first line, its the header info 
            var patches = patchData.Split('\n');
            patches = patches.OrderByDescending((a) => a.Length).ToArray();
            
            FindAndPatch(inputFolder, patches); 
        }

        private int AddFileCounter(string path)
        {
            var counter = 0; 
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                counter += AddFileCounter(directory); 
            }
            
            counter += Directory.GetFiles(path).Length;
            return counter; 
        }

        private bool FindAndPatch(string path, string[] patches)
        {
            bool found = false;
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                if (FindAndPatch(directory, patches))
                {
                    found = true; 
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if(PatchFile(file, patches))
                {
                    found = true; 
                }
            }

            return found;
        }

        private bool PatchFile(string file, string[] patches)
        {
            var found = false;

            byte[] buffer = null;
            var replaced_count = 0;
            
            using (var reader = File.OpenRead(file))
            {
                var totalBytes = reader.Length;
                buffer = new byte[totalBytes];
                
                var read_per = (int)Math.Min(totalBytes, 1024);
                for (var b = 0; b < totalBytes - read_per; b += read_per)
                {
                    reader.Read(buffer, b, read_per);
                }

                //
                for (var p = 1; p < patches.Length; ++p)
                {
                    UpdateProress(1);

                    var patchData = patches[p];
                    var data = patchData.Split(',');

                    if (data.Length < 3)
                    {
                        continue;
                    }

                    var original = data[0];
                    var translation = data[2];

                    if(string.IsNullOrEmpty(original) || string.IsNullOrEmpty(translation))
                    {
                        continue; 
                    }
                    
                    // enforce the translation to equal the number of bytes we're patching 
                    var encoding = Encoding.GetEncoding("shift_jis"); // "shift_jis"
                    var searchTerm = encoding.GetBytes(original);
                    var patch = encoding.GetBytes(translation);
                    var spaces = encoding.GetBytes(" ");

                    if (searchTerm.Length != patch.Length)
                    {
                        var cutTranslation = new byte[searchTerm.Length];
                        for (var b = 0; b < searchTerm.Length; ++b)
                        {
                            if (b >= patch.Length)
                            {
                                cutTranslation[b] = spaces[0];
                            }
                            else
                            {
                                cutTranslation[b] = patch[b];
                            }
                        }
                        patch = cutTranslation;
                    }

                    // var success = FindAndPatch(inputFolder, originalBytes, translationBytes);

                    for (var b = 0; b < buffer.Length - searchTerm.Length; ++b)
                    {
                        var match = true;
                        for (var r = 0; r < searchTerm.Length; ++r)
                        {
                            if (buffer[b + r] != searchTerm[r])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (!match)
                        {
                            continue;
                        }

                        // match found, replace!
                        for (var r = 0; r < searchTerm.Length; ++r)
                        {
                            buffer[b + r] = patch[r];
                        }

                        replaced_count += 1;
                    }
                }
                //



                found = replaced_count > 0;
            }

            if (found)
            {
                using (var writer = File.OpenWrite(file))
                {
                    var totalBytes = buffer.Length;
                    var write_per = (int)Math.Min(totalBytes, 1024);
                    for(var b = 0; b < totalBytes - write_per; b += write_per)
                    {
                        writer.Write(buffer, b, write_per);
                    }
                }

                // var encoding = Encoding.GetEncoding("shift_jis");
                // var searchTermString = encoding.GetString(searchTerm);
                // var patchString = encoding.GetString(patch);
                // var log = string.Format($"Replaced \"{searchTermString}\" with \"{patchString}\" at \"{file}\", ({replaced_count} instances)");
                // Log(log);
                Log($"success! ({replaced_count} instances) at \"{file}\"");
            }
            else
            {
                Log($"skipping \"{file}\"");
            }

            return found; 
        }

        public void Log(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), new object[] { value });
                return;
            }

            OutputText.Text = string.Format("{0}\r\n{1}", value, OutputText.Text);
        }

        public void UpdateProress(int addProgress)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateProress), new object[] { addProgress });
                return;
            }

            PatchProgressBar.Value += addProgress;
        }
    }
}

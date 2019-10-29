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
            PatchProgressBar.Maximum = patchData.Split('\n').Length;

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
            for(var p = 1; p < patches.Length; ++p) 
            {
                UpdateProress(1);

                var patch = patches[p];
                var data = patch.Split(',');

                var original = data[0];
                var translation = data[1];

                Log($"Searching for {original}..");

                // enforce the translation to equal the number of bytes we're patching 
                var encoding = Encoding.GetEncoding("shift_jis");
                var originalBytes = encoding.GetBytes(original);
                var translationBytes = encoding.GetBytes(translation);
                var spaces = encoding.GetBytes(" ");

                if (originalBytes.Length != translationBytes.Length)
                {
                    var cutTranslation = new byte[originalBytes.Length];
                    for(var b = 0; b < originalBytes.Length; ++b)
                    {
                        if(b >= translationBytes.Length)
                        {
                            cutTranslation[b] = spaces[0]; 
                        }
                        else
                        {
                            cutTranslation[b] = translationBytes[b];
                        }
                    }
                    translationBytes = cutTranslation;
                }

                try
                {
                    var success = FindAndPatch(inputFolder, originalBytes, translationBytes);

                }
                catch(System.Exception e)
                {
                    Log(e.StackTrace);
                    Log(e.Message);
                }
            }

        }

        private bool FindAndPatch(string path, byte[] searchTerm, byte[] patch)
        {
            bool found = false;
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                if (FindAndPatch(directory, searchTerm, patch))
                {
                    found = true; 
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if(PatchFile(file, searchTerm, patch))
                {
                    found = true; 
                }
            }

            return found;
        }

        private bool PatchFile(string file, byte[] searchTerm, byte[] patch)
        {
            var found = false;

            byte[] buffer = null;
            var replaced_count = 0;

            using (var reader = File.OpenRead(file))
            {
                var totalBytes = reader.Length;
                buffer = new byte[totalBytes];

                var max_string_size = searchTerm.Length;
                var read_per = (int)Math.Min(totalBytes, 1024);
                for (var b = 0; b < totalBytes - read_per; b += read_per)
                {
                    reader.Read(buffer, b, read_per);
                }

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

                var encoding = Encoding.GetEncoding("shift_jis");
                var searchTermString = encoding.GetString(searchTerm);
                var patchString = encoding.GetString(patch);
                var log = string.Format($"Replaced \"{searchTermString}\" with \"{patchString}\" at \"{file}\", ({replaced_count} instances)");
                Log(log);
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

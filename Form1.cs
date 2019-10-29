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

            DoApplyPatch(inputPath, outputPath, patchData);
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
            PatchProgressBar.Minimum = 0;
            PatchProgressBar.Maximum = 100;

            // skip first line, its the header info 
            var patches = patchData.Split('\n');
            for(var p = 1; p < patches.Length; ++p) 
            {
                var patch = patches[p];
                var data = patch.Split(',');

                var original = data[0];
                var translation = data[1];
                
                // enforce the translation to equal the number of bytes we're patching 
                var originalBytes = Encoding.UTF8.GetBytes(original);
                var translationBytes = Encoding.UTF8.GetBytes(translation); 

                if(originalBytes.Length != translationBytes.Length)
                {
                    var cutTranslation = new byte[originalBytes.Length];
                    for(var b = 0; b < cutTranslation.Length; ++b)
                    {
                        cutTranslation[b] = translationBytes[b];
                    }
                    translationBytes = cutTranslation;
                }

                var success = FindAndPatch(inputFolder, original, translation);
            }

        }

        private bool FindAndPatch(string path, string searchTerm, string patch)
        {
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                if (FindAndPatch(directory, searchTerm, patch))
                {
                    return true;
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                byte[] write_buffer = null;
                bool found = false;
                
                using (var reader = File.OpenRead(file))
                {
                    var totalBytes = reader.Length;
                    var read_buffer = new byte[totalBytes]; 

                    var read_per = (int) Math.Min(totalBytes, 1024); 
                    for(var b = 0; b < totalBytes - read_per; b += read_per)
                    {
                        reader.Read(read_buffer, b, read_per);
                        PatchProgressBar.Value = (int) ((float) (b / totalBytes) * 100 / 2);
                    }

                    var dataString = Encoding.UTF8.GetString(read_buffer);
                    var foundIndex = dataString.IndexOf(searchTerm);
                    if(foundIndex >= 0)
                    {
                        dataString = dataString.Replace(searchTerm, patch);
                        write_buffer = Encoding.UTF8.GetBytes(dataString);
                        found = true; 
                    }
                }

                if (found)
                {
                    // using (var writer = File.OpenWrite(file))
                    // {
                    //     var totalBytes = write_buffer.Length;
                    //     var write_per = (int)Math.Min(totalBytes, 1024);
                    //     for(var b = 0; b < totalBytes - write_per; b += write_per)
                    //     {
                    //         writer.Write(write_buffer, b, write_per);
                    //         PatchProgressBar.Value = (int)((float)(b / totalBytes) * 100 / 2);
                    //     }
                    // }

                    var log = string.Format($"Replaced \"{searchTerm}\" with \"{patch}\" at \"{path}\"");
                    OutputText.AppendText(log);

                    return false; 
                }
            }

            return false;
        }
    }
}

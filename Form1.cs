using DiscUtils.Iso9660;
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
            OpenISODialog.ShowDialog();
            
            var path = OpenISODialog.FileName;
            SelectISOText.Text = path;

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
            var input = SelectISOText.Text; 
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
            var inputFile = SelectISOText.Text;
            var outputPath = OutputFileText.Text;
            var patchData = PatchFileData.Text;

            // todo 
            // skip first line, its the header info 
            PatchProgressBar.Minimum = 0;
            PatchProgressBar.Maximum = patchData.Split('\n').Length; // * AddFileCounter(inputFile);

            OutputFileButton.Enabled = false;
            SelectFolderButton.Enabled = false; 
            ApplyPatchButton.Enabled = false;

            PatchProgressBar.Value = 0; 

            var thread = new System.Threading.Thread(() =>
            {
                Log("Starting..");

                    DoApplyPatchISO(inputFile, outputPath, patchData);


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
        
        private void DoApplyPatchISO(string inputFile, string outputFile, string patchData)
        {
            var tempFolder = "./temp";

            ExtractISO(inputFile, tempFolder);

            // todo: extract from AFS files
            // todo: decompress extracted AFS files
            // todo: patch decompressed file
            // todo: recompress patched extracted AFS files
            // todo: reinject AFS file

            RepackISO(tempFolder, outputFile); 
        }

        private void ExtractISO(string inputFile, string outputFolder)
        {
            using (FileStream isoStream = File.Open(inputFile, FileMode.Open))
            {
                var reader_cd = new CDReader(isoStream, true);
                ExtractISO_Directory(reader_cd, outputFolder, "");
            }
        }

        private void ExtractISO_Directory(CDReader cd, string outputFolder, string cd_path)
        {
            var real_directory = string.Format("{0}/{1}", outputFolder, cd_path);
            if (!Directory.Exists(real_directory))
            {
                Directory.CreateDirectory(real_directory);
            }
            
            var cd_directories = cd.GetDirectories(cd_path);
            foreach (var cd_directory in cd_directories)
            {
                ExtractISO_Directory(cd, outputFolder, cd_directory); 
            }

            var cd_files = cd.GetFiles(cd_path);
            foreach(var cd_file in cd_files)
            {
                var fileStream = cd.OpenFile(cd_file, FileMode.Open);
                var real_file = string.Format("{0}/{1}", outputFolder, cd_file);

                Log($"writing {cd_file} to {real_file}");

                using (var writerStream = File.OpenWrite(real_file))
                {
                    var max_chunk = 1024;
                    var buffer = new byte[max_chunk];

                    for(var b = 0; b < fileStream.Length - max_chunk; b += max_chunk)
                    {
                        var amount = (int) Math.Min(max_chunk, fileStream.Length - max_chunk);
                        fileStream.Read(buffer, 0, amount);
                        writerStream.Write(buffer, 0, amount); 
                    }
                }

                fileStream.Dispose(); 
            }
        }

        private void RepackISO(string inputFolder, string outputFile)
        {
            var writer_cd = new CDBuilder();
            RepackISO_Directory(writer_cd, inputFolder, inputFolder);
            writer_cd.Build(outputFile);
        }

        private void RepackISO_Directory(CDBuilder cd, string rootFolder, string inputFolder)
        {
            var directories = Directory.GetDirectories(inputFolder);
            foreach(var directory in directories)
            {
                var cd_directory = directory.Replace(rootFolder, string.Empty);
                cd.AddDirectory(cd_directory);

                RepackISO_Directory(cd, rootFolder, directory);
            }

            var files = Directory.GetFiles(inputFolder);
            foreach(var file in files)
            {
                byte[] buffer = null;
                using (var readStream = File.OpenRead(file))
                {
                    buffer = new byte[readStream.Length];
                    var max_size = 1024; 
                    
                    for(var b = 0; b < readStream.Length - max_size; b += max_size)
                    {
                        readStream.Read(buffer, b, (int) Math.Min(max_size, readStream.Length - max_size)); 
                    }
                }
                
                var cd_file = file.Replace(rootFolder, string.Empty);
                var writeStream = cd.AddFile(cd_file, buffer);
            }
        }

        private void DoApplyPatchRecursivelyForFolder(string inputFolder, string outputFile, string patchData)
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

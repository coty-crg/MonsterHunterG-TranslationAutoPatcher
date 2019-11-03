using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DiscUtils.Iso9660; 

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void TryEnableApplyButton()
        {
            var input = SelectISOText.Text; 
            var output = OutputFileText.Text;
            var patchData = PatchFileData.Text;
            var imgBurnPath = ImgBurnPathTextBox.Text;

            var ready = !string.IsNullOrEmpty(input)
                && !string.IsNullOrEmpty(output)
                && !string.IsNullOrEmpty(patchData)
                && !string.IsNullOrEmpty(imgBurnPath);

            ApplyPatchButton.Enabled = ready; 
        }

        private void EnableAllOptions(bool enable)
        {
            ApplyPatchButton.Enabled = enable;

            SelectFolderButton.Enabled = enable;
            OutputFileButton.Enabled = enable;
            SelectISOText.Enabled = enable;
            OutputFileText.Enabled = enable;
            PatchFileData.Enabled = enable;
            ImgBurnPathTextBox.Enabled = enable;
            SelectPatchFileButton.Enabled = enable;
            CleanupCheckbox.Enabled = enable; 
            KeepOldArchivesCheckBox.Enabled = enable;
            DeepSearchCheckbox.Enabled = enable;
        }
        
        private class TempFileMetadata
        {
            public string originalFilename;
            public string unpackedFilename;

            public bool uncompressed;
            public bool patched;
            public bool recompressed; 
            public bool reinjected;
        }

        private Dictionary<string, List<string>> GetArchivePatchInfo(string[] patches)
        {
            var patchingFiles = new Dictionary<string, List<string>>();

            // var deepSearch = DeepSearchCheckbox.Checked;
            // if (deepSearch)
            // {
            //     var archives = archiveData.Split(',');
            //     foreach (var archive in archives)
            //     {
            //         var fileList = new List<string>();
            //         fileList.Add("sub_main.bin");
            // 
            //         patchingFiles.Add(archive, fileList);
            //     }
            // }

            for (var i = 0; i < patches.Length; ++i)
            {
                var patch = patches[i].Split(',');
                var patchArchive = patch[4];
                var patchFile = patch[5];
                
                // ignore null info 
                if (string.IsNullOrEmpty(patchArchive) || string.IsNullOrEmpty(patchFile))
                {
                    continue;
                }

                List<string> fileList;
                var found = patchingFiles.TryGetValue(patchArchive, out fileList);

                if (!found)
                {
                    fileList = new List<string>();
                    patchingFiles.Add(patchArchive, fileList);
                }

                if (!fileList.Contains(patchFile))
                {
                    fileList.Add(patchFile);
                }
            }

            return patchingFiles;
        }

        private void RecursivelyDeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return; 
            }

            try
            {
                var childDirectories = Directory.GetDirectories(directory);
                foreach(var childDirectory in childDirectories)
                {
                    RecursivelyDeleteDirectory(childDirectory); 
                }

                var files = Directory.GetFiles(directory);
                foreach(var file in files)
                {
                    File.Delete(file);
                }

                Directory.Delete(directory); 
            }
            catch(System.Exception e)
            {
                Log(e.StackTrace);
                Log(e.Message);
                Log($"Failed to delete {directory}");
            }
        }

        private void DoApplyPatchISO(string inputFile, string outputFile, string patchData)
        {
            var patches = GetPatches(patchData);

            DiscUtils.Complete.SetupHelper.SetupComplete();

            var tempFolder = "./temp";
            var tempFolderAFS = "./tempAFS";
            var deepSearch = DeepSearchCheckbox.Checked;

            // cleanup previous work 
            RecursivelyDeleteDirectory(tempFolder);
            RecursivelyDeleteDirectory(tempFolderAFS);

            // prepare directories 
            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(tempFolderAFS);
            
            var volumeLabel = string.Empty;
            ExtractISO(inputFile, tempFolder, out volumeLabel);
            volumeLabel += "_PATCHED";

            // which archive files contain the bin files we care about? 
            var patchingFiles = GetArchivePatchInfo(patches); 

            // loop through, unpacking and repacking the archives as we go
            foreach (var entry in patchingFiles)
            {
                var unpackAFS = entry.Key;
                var unpackExtensions = entry.Value;

                var inputArchiveFile = string.Format("{0}/{1}", tempFolder, unpackAFS);
                var outputArchiveFolder = string.Format("{0}/{1}", tempFolderAFS, unpackAFS);

                var archive = ExtractFromAFS(inputArchiveFile, outputArchiveFolder);
                if(archive == null)
                {
                    Log($"Couldn't find {inputArchiveFile}");
                    continue; 
                }
                
                // search for any archived files with the extensions we care about 
                var unpackFiles = new List<string>(); 
                for(var d = 0; d < archive.directory.Length; ++d)
                {
                    var dir_entry = archive.directory[d];
                    
                    for(var e = 0; e < unpackExtensions.Count; ++e)
                    {
                        var extension = unpackExtensions[e];

                        // not exactly strict
                        if (deepSearch || dir_entry.filename.Contains(extension)) 
                        {
                            unpackFiles.Add(dir_entry.filename);
                            break;
                        }
                    }
                }
                
                // uncompress, patch, recompress, reinject into AFS instance 
                var metadata = new List<TempFileMetadata>();
                
                // update progress bar 
                var max_progress = unpackFiles.Count * 3;
                var cur_progress = 0;
                
                // uncompress 
                Log("Decompressing packed files.");
                
                for(var i = 0; i < unpackFiles.Count; ++i)
                {
                    UpdateProress(cur_progress, 0, max_progress);
                    cur_progress += 1;
                
                    var unpackFile = unpackFiles[i];
                    var targetFilenameFull = string.Format("{0}/{1}", outputArchiveFolder, unpackFile);
                    var targetFilenameUnpackedFull = $"{targetFilenameFull}.unpacked";
                    
                    var meta = new TempFileMetadata();
                    meta.originalFilename = targetFilenameFull;
                    meta.unpackedFilename = targetFilenameUnpackedFull;
                    metadata.Add(meta);
                
                    meta.uncompressed = UnpackAFS_File(targetFilenameFull);
                    if (!meta.uncompressed)
                    {
                        Log($"Failed to uncompress {targetFilenameFull}! Did crappack fail?");
                        continue; 
                    }
                }
                
                Log("Patching uncompressed files.");
                for (var i = 0; i < unpackFiles.Count; ++i)
                {
                    UpdateProress(cur_progress, 0, max_progress);
                    cur_progress += 1;
                
                    var unpackFile = unpackFiles[i];
                    var meta = metadata[i];
                
                    if (!meta.uncompressed)
                    {
                        continue;
                    }
                
                    meta.patched = PatchFile(meta.unpackedFilename, patches);
                }
                
                Log("Recompressing patched files. This may take awhile.");
                for (var i = 0; i < unpackFiles.Count; ++i)
                {
                    UpdateProress(cur_progress, 0, max_progress);
                    cur_progress += 1;
                
                    var unpackFile = unpackFiles[i];
                    var meta = metadata[i];
                
                    if (!meta.uncompressed || !meta.patched)
                    {
                        continue;
                    }
                    
                    meta.recompressed = RepackAFS_File(meta.originalFilename, meta.unpackedFilename);
                    if (!meta.recompressed)
                    {
                        Log($"Failed to recompressed {meta.originalFilename}! Did crappack fail?");
                        continue;
                    }
                
                    meta.reinjected = ReinjectInAFS(archive, meta.originalFilename);
                    if (!meta.reinjected)
                    {
                        Log($"Failed to reinject {meta.originalFilename}");
                        continue; 
                    }
                }
                
                // possibly keep for reference 
                // note: unless deleted it will get put into the ISO
                var keepOldArchives = KeepOldArchivesCheckBox.Checked;
                if (keepOldArchives)
                {
                    Log("Cloning previous AFS.");
                    CloneOldAFS(inputArchiveFile); 
                }
                
                Log("Rebuilding fresh AFS.");
                RebuildAFS(archive, inputArchiveFile); // rebuilds from scratch, using the modified instance 
            }
            
            Log("Rebuilding fresh ISO.");
            RepackISO(tempFolder, outputFile, volumeLabel); 

            // if checked, delete all the temp stuff we made
            var cleanup = CleanupCheckbox.Checked;
            if (cleanup)
            {
                RecursivelyDeleteDirectory(tempFolder);
                RecursivelyDeleteDirectory(tempFolderAFS);
            }
        }
        
        private void ExtractISO(string inputFile, string outputFolder, out string volumeLabel)
        {
            using (FileStream isoStream = File.Open(inputFile, FileMode.Open))
            {
                var reader_cd = new CDReader(isoStream, true, true);
                volumeLabel = reader_cd.VolumeLabel;
                ExtractISO_Directory(reader_cd, outputFolder, reader_cd.Root.FullName);
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
                // Log($"extracting {cd_file} to {real_file}");

                using (var writerStream = File.OpenWrite(real_file))
                {
                    var max_chunk = 1024;
                    var buffer = new byte[max_chunk];

                    for(var b = 0; b < fileStream.Length; b += max_chunk)
                    {
                        var amount = (int) Math.Min(max_chunk, fileStream.Length - b);
                        fileStream.Read(buffer, 0, amount);
                        writerStream.Write(buffer, 0, amount); 
                    }
                }

                fileStream.Dispose(); 
            }
        }

        private void RepackISO(string inputFolder, string outputFile, string volumeLabel)
        {
            Log($"Rebuilding ISO with ImgBurn: ISO9660 + UDF. " +
                $"Volume Label: {volumeLabel}, input folder: {inputFolder}, output file: {outputFile}");

            var imgBurnPath = ImgBurnPathTextBox.Text;
            IsoBuildHelper.ImgBurnCreateISO(imgBurnPath, volumeLabel, inputFolder, outputFile);

            Log($"Finished rebulding ISO: {outputFile}");

            // PS2 doesnt read this ISO 
            // IsoBuildHelper.CreateISOImage(volumeLabel, inputFolder, outputFile);

            // PS2 doesn't read this ISO 
            // var writer_cd = new DiscUtils.Iso9660.CDBuilder();
            // writer_cd.UseJoliet = true;
            // writer_cd.UpdateIsolinuxBootTable = true; 
            // writer_cd.VolumeIdentifier = volumeLabel;
            // 
            // // repack boot image 
            // if (bootImageStream != null)
            // {
            //     Log("writing boot image"); 
            //     writer_cd.SetBootImage(bootImageStream, BootDeviceEmulation.NoEmulation, bootLoadSegment); 
            // }
            // 
            // // repack files 
            // RepackISO_Directory(writer_cd, inputFolder, inputFolder);
            // 
            // // save to disk 
            // var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite); 
            // writer_cd.Build(stream);
        }

        private void RepackISO_Directory(CDBuilder cd, string rootFolder, string inputFolder)
        {
            var directories = Directory.GetDirectories(inputFolder);
            foreach(var directory in directories)
            {
                var cd_directory = directory.Replace(rootFolder, string.Empty);
                var directoryInfo = cd.AddDirectory(cd_directory);

                RepackISO_Directory(cd, rootFolder, directory);
            }
            
            var files = Directory.GetFiles(inputFolder);
            foreach(var file in files)
            {
                byte[] buffer = null;
                using (var readStream = File.OpenRead(file))
                {
                    buffer = new byte[readStream.Length];
                    var max_chunk = 1024; 
                    
                    for(var b = 0; b < readStream.Length; b += max_chunk)
                    {
                        var amount = (int)Math.Min(max_chunk, readStream.Length - b);
                        readStream.Read(buffer, b, amount); 
                    }
                }
                
                var cd_file = file.Replace(rootFolder, string.Empty);
                var fileInfo = cd.AddFile(cd_file, buffer);
            }
        }
        
        private AFS ExtractFromAFS(string input, string output)
        {
            if (!File.Exists(input))
            {
                return null; 
            }

            using (var reader = File.OpenRead(input))
            {
                var archive = AFS.FromStream(reader);

                Log($"Extracted AFS!");
                Log($"header: {archive.header}"); 
                Log($"numFiles: {archive.numFiles}");

                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output); 
                }
                
                for (var i = 0; i < archive.numFiles; ++i)
                {
                    var toc_entry = archive.tableOfContents[i];
                    var directory_entry = archive.directory[i];
                    var file = archive.files[i];

                    var fullFilename = string.Format("{0}/{1}", output, directory_entry.filename);

                    using (var writer = File.OpenWrite(fullFilename))
                    {
                        writer.Write(file.data, 0, file.data.Length); 
                    }
                }

                return archive;
            }
        }
        
        private bool ReinjectInAFS(AFS afs, string input_file)
        {
            var shortFilename = input_file;
            while (shortFilename.IndexOf('/') >= 0)
            {
                var until = shortFilename.IndexOf('/');
                var start_index = until + 1;
                var delta = shortFilename.Length - start_index;
                shortFilename = shortFilename.Substring(start_index, delta); 
            }

            Log($"reinjecting into {shortFilename}");

            var directory_index = -1;
            for(var i = 0; i < afs.directory.Length; ++i)
            {
                var entry = afs.directory[i];
                if(entry.filename == shortFilename)
                {
                    directory_index = i;
                    break; 
                }
            }

            if(directory_index == -1)
            {
                return false; 
            }

            // read in file to the block at directory_index
            using (var reader = File.OpenRead(input_file))
            {
                var afs_file = afs.files[directory_index];
                afs_file.data = new byte[reader.Length];
                reader.Read(afs_file.data, 0, afs_file.data.Length);

                // update lengths 
                var toc_entry = afs.tableOfContents[directory_index];
                toc_entry.length = (uint) afs_file.data.Length;

                var directory_entry = afs.directory[directory_index];
                directory_entry.fileLength = (uint)afs_file.data.Length;
            }

            return true; 
        }

        private void CloneOldAFS(string input)
        {
            var copyName = $"{input}.old";

            if (File.Exists(copyName))
            {
                File.Delete(copyName);
            }

            File.Copy(input, copyName); 
        }

        private void RebuildAFS(AFS afs, string output_afs)
        {
            Log("Saving AFS.");

            using (var writer = File.OpenWrite(output_afs))
            {
                afs.WriteStream(writer);
            }

            Log("Saved AFS.");
        }

        // returns output filename 
        private bool UnpackAFS_File(string input)
        {
            var command = $"crappack-cmd.py -u {input}";
            //Log($"Decompressing {input}");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            var exitCode = process.ExitCode;
            return exitCode == 0;
        }

        private bool RepackAFS_File(string inputPacked, string inputUnpacked)
        {
            var original = inputPacked;
            var unpacked = inputUnpacked;

            var command = $"crappack-cmd.py -p {unpacked} -o {original}";
            //Log($"Recompressing {inputPacked}");
            
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "python.exe";
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            var exitCode = process.ExitCode;
            return exitCode == 0; 
        }

        private string[] GetPatches(string patchData)
        {
            var patches0 = patchData.Split('\n');

            // skip first line, its the header info 
            var patches1 = new string[patches0.Length - 1];
            for(var i = 0; i < patches1.Length; ++i)
            {
                patches1[i] = patches0[i + 1]; 
            }
            
            // order by longest raw name first, so we don't accidentally replace 
            // a smaller string within a bigger one
            var patches = patches1.OrderByDescending((a) => a.Length).ToArray();
            return patches;
        }

        private void DoApplyPatchRecursivelyForFolder(string inputFolder, string outputFile, string patchData)
        {
            var patches = GetPatches(patchData); 
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

            if (!File.Exists(file))
            {
                Log($"Couldn't find {file}. Did crappack fail to pack?");
                return false; 
            }

            using (var reader = File.OpenRead(file))
            {
                var totalBytes = reader.Length;
                buffer = new byte[totalBytes];
                
                var read_per = (int)Math.Min(totalBytes, 1024);
                for (var b = 0; b < totalBytes - read_per; b += read_per)
                {
                    reader.Read(buffer, b, read_per);
                }
                
                for (var p = 1; p < patches.Length; ++p)
                {
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

                    // 🔪 live on the edge 🔪
                    // don't do anything dangerous 
                    if(original.Length < 2)
                    {
                        continue; 
                    }

                    // enforce the translation to equal the number of bytes we're patching 
                    var encoding = Encoding.GetEncoding("shift_jis");
                    var searchTerm = encoding.GetBytes(original);
                    var patch = encoding.GetBytes(translation);
                    var spaces = encoding.GetBytes(" ");

                    var chunk_size = 8;
                    var original_padding = chunk_size - searchTerm.Length % chunk_size;
                    var original_full_length = searchTerm.Length + original_padding; 

                    if (original_full_length != patch.Length)
                    {
                        var paddedTranslation = new byte[searchTerm.Length];
                        for (var b = 0; b < searchTerm.Length; ++b)
                        {
                            if (b >= patch.Length)
                            {
                                paddedTranslation[b] = spaces[0];
                            }
                            else
                            {
                                paddedTranslation[b] = patch[b];
                            }
                        }

                        patch = paddedTranslation;
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
                
            }

            if(replaced_count > 0)
            {
                Log($"Updated {replaced_count} strings in \"{file}\"!");
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
            System.Diagnostics.Debug.WriteLine(value); 
        }
        
        public void UpdateProress(int progress, int min, int max)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int, int, int>(UpdateProress), new object[] { progress, min, max });
                return;
            }

            progress = Math.Max(Math.Min(progress, max), min);
            PatchProgressBar.Minimum = min;
            PatchProgressBar.Maximum = max; 
            PatchProgressBar.Value = progress;
        }

        // events 
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
            OpenISODialog.DefaultExt = ".iso";
            OpenISODialog.FileName = "original.iso";
            OpenISODialog.Filter = "ISO file (*.iso)|*.iso";

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
            OutputFileDialog.DefaultExt = ".iso";
            OutputFileDialog.FileName = "patched.iso";
            OutputFileDialog.Filter = "ISO file (*.iso)|*.iso";

            OutputFileDialog.ShowDialog();

            var path = OutputFileDialog.FileName;
            OutputFileText.Text = path;

            TryEnableApplyButton();
        }

        private void PatchProgressBar_Click(object sender, EventArgs e)
        {

        }

        private void ApplyPatchButton_Click(object sender, EventArgs e)
        {
            var inputFile = SelectISOText.Text;
            var outputPath = OutputFileText.Text;
            var patchData = PatchFileData.Text;
            
            UpdateProress(0, 0, 100); 

            OutputFileButton.Enabled = false;
            SelectFolderButton.Enabled = false;
            ApplyPatchButton.Enabled = false;

            EnableAllOptions(false); 

            var thread = new System.Threading.Thread(() =>
            {
                Log("STARTING! Go grab some coffee.");

                DoApplyPatchISO(inputFile, outputPath, patchData);
                
                Log("FINISHED! Was it worth the wait?");

                // run on main thread 
                this.Invoke(new Action(() =>
                {
                    EnableAllOptions(true);
                    PatchProgressBar.Value = PatchProgressBar.Maximum;
                }));
            });

            thread.Start();
        }

        private void SelectPatchFileButton_Click(object sender, EventArgs e)
        {
            SelectPatchFileDialog.DefaultExt = ".csv";
            SelectPatchFileDialog.FileName = "patch.csv";
            SelectPatchFileDialog.Filter = "Patch File (*.csv)|*.csv";

            SelectPatchFileDialog.ShowDialog();

            var patchFilePath = SelectPatchFileDialog.FileName;
            if (!string.IsNullOrEmpty(patchFilePath))
            {
                var patchData = System.IO.File.ReadAllText(patchFilePath);
                PatchFileData.Text = patchData;
            }

            TryEnableApplyButton();
        }

        private void OutputText_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

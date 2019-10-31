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
            SelectPatchFileDialog.DefaultExt = ".csv";
            SelectPatchFileDialog.FileName = "patch.csv";
            SelectPatchFileDialog.Filter = "Patch File (*.csv)|*.csv";

            SelectPatchFileDialog.ShowDialog();

            var patchFilePath = SelectPatchFileDialog.FileName;
            var patchData = System.IO.File.ReadAllText(patchFilePath);
            PatchFileData.Text = patchData;

            TryEnableApplyButton(); 
        }
        
        private void DoApplyPatchISO(string inputFile, string outputFile, string patchData)
        {
            DiscUtils.Complete.SetupHelper.SetupComplete();

            var tempFolder = "./temp";
            var tempFolderAFS = "./tempAFS";

            if (!Directory.Exists(tempFolderAFS))
            {
                Directory.CreateDirectory(tempFolderAFS);
            }

            //BootDeviceEmulation bootEmulation;
            Stream bootImageStream;
            int bootLoadSegment;
            string volumeLabel;

            // ExtractISO(inputFile, tempFolder, out bootImageStream, out bootLoadSegment, out volumeLabel);



            var files = new List<string>() { "AFS_DATA.AFS" }; 
            foreach(var file in files)
            {
                ExtractFromAFS(string.Format("{0}/{1}", tempFolder, file), string.Format("{0}/{1}", tempFolderAFS, file));
            }

            // todo: extract from AFS files
            // todo: decompress extracted AFS files
            // todo: patch decompressed file
            // todo: recompress patched extracted AFS files
            // todo: reinject AFS file

            // RepackISO(tempFolder, outputFile, bootImageStream, bootLoadSegment, volumeLabel); 
        }
        
        private void ExtractISO(string inputFile, string outputFolder, out Stream bootImageStream, out int bootLoadSegment,
            out string volumeLabel)
        {
            using (FileStream isoStream = File.Open(inputFile, FileMode.Open))
            {
                
                var reader_cd = new CDReader(isoStream, true, true);

                volumeLabel = reader_cd.VolumeLabel;
                bootLoadSegment = reader_cd.BootLoadSegment;
                bootImageStream = null;


                // reader_cd.Root

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

                var attributes = cd.GetAttributes(cd_file);


                var real_file = string.Format("{0}/{1}", outputFolder, cd_file);

                Log($"writing {cd_file} to {real_file}");

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

        private void RepackISO(string inputFolder, string outputFile, Stream bootImageStream, int bootLoadSegment,
            string volumeLabel)
        {
            var writer_cd = new DiscUtils.Iso9660.CDBuilder();
            writer_cd.UseJoliet = true;
            writer_cd.UpdateIsolinuxBootTable = true; 
            writer_cd.VolumeIdentifier = volumeLabel;

            // repack boot image 
            if (bootImageStream != null)
            {
                Log("writing boot image"); 
                writer_cd.SetBootImage(bootImageStream, BootDeviceEmulation.NoEmulation, bootLoadSegment); 
            }

            // repack files 
            RepackISO_Directory(writer_cd, inputFolder, inputFolder);
            
            // save to disk 
            var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite); 
            writer_cd.Build(stream);
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
        
        private class AFS_TOC_Entry
        {
            public uint offset;
            public uint length;

            public uint filenameDirectoryOffset;
            public uint filenameDirectoryLength;

            public static AFS_TOC_Entry FromStream(FileStream stream)
            {
                var entry = new AFS_TOC_Entry();

                entry.offset = BinaryHelper.ReadUInt32(stream);
                entry.length = BinaryHelper.ReadUInt32(stream);
                entry.filenameDirectoryOffset = BinaryHelper.ReadUInt32(stream);
                entry.filenameDirectoryLength = BinaryHelper.ReadUInt32(stream);

                return entry;
            }

            public void WriteStream(FileStream stream)
            {
                BinaryHelper.WriteUInt32(stream, offset);
                BinaryHelper.WriteUInt32(stream, length);
                BinaryHelper.WriteUInt32(stream, filenameDirectoryOffset);
                BinaryHelper.WriteUInt32(stream, filenameDirectoryLength);
            }
        }

        private class AFS_Directory_Entry
        {
            public string filename; // 32 bytes
            public ushort year;
            public ushort month;
            public ushort day;
            public ushort hour;
            public ushort minute;
            public ushort second;
            public uint fileLength;
            
            public static AFS_Directory_Entry FromStream(FileStream stream)
            {
                var entry = new AFS_Directory_Entry();

                entry.filename = BinaryHelper.ReadString(stream, 32);
                entry.year = BinaryHelper.ReadUInt16(stream);
                entry.month = BinaryHelper.ReadUInt16(stream);
                entry.day = BinaryHelper.ReadUInt16(stream);
                entry.hour = BinaryHelper.ReadUInt16(stream);
                entry.minute = BinaryHelper.ReadUInt16(stream);
                entry.second = BinaryHelper.ReadUInt16(stream);
                entry.fileLength = BinaryHelper.ReadUInt32(stream);

                return entry;
            }

            public void WriteStream(FileStream stream)
            {
                BinaryHelper.WriteString(stream, filename, 32);
                BinaryHelper.WriteUInt16(stream, year);
                BinaryHelper.WriteUInt16(stream, month);
                BinaryHelper.WriteUInt16(stream, day);
                BinaryHelper.WriteUInt16(stream, hour);
                BinaryHelper.WriteUInt16(stream, minute);
                BinaryHelper.WriteUInt16(stream, second);
                BinaryHelper.WriteUInt32(stream, fileLength); 
            }
        }

        private class AFS
        {
            public string header;
            public uint numFiles;

            public AFS_TOC_Entry[] tableOfContents;
            public AFS_Directory_Entry[] directory;
            
            public byte[] data;


            public static AFS FromStream(FileStream stream)
            {
                var afs = new AFS();

                afs.header = BinaryHelper.ReadString(stream, 4);
                afs.numFiles = BinaryHelper.ReadUInt32(stream);

                uint dataBlockLength = 0;

                afs.tableOfContents = new AFS_TOC_Entry[afs.numFiles];

                for (var i = 0; i < afs.numFiles; ++i)
                {
                    var toc_entry = AFS_TOC_Entry.FromStream(stream); 
                    afs.tableOfContents[i] = toc_entry;

                    var multiple = (toc_entry.length / 2048) + 1;
                    var blockSize = multiple * 2048;

                    dataBlockLength += blockSize; 
                }

                

                afs.data = new byte[dataBlockLength]; 

                // read all the files 
                // var dataIndex = 0u; 
                // for(var i = 0; i < afs.numFiles; ++i)
                // {
                //     var entry = afs.tableOfContents[i];
                // 
                // 
                // 
                //     // var multiple = (entry.length / 2048) + 1;
                //     // var blockSize = multiple * 2048;
                //     // 
                //     // stream.Read(afs.data, (int) dataIndex, (int) blockSize);
                //     // dataIndex += blockSize;
                // }
                

                afs.directory = new AFS_Directory_Entry[afs.numFiles];



                var dataBlockStart = afs.tableOfContents[0].offset;
                var directoryOffsetStreamStart = dataBlockStart - 8;
                stream.Position = directoryOffsetStreamStart;
                stream.Position = BinaryHelper.ReadUInt32(stream);

                for (var i = 0; i < afs.numFiles; ++i)
                {
                    var toc_entry = afs.tableOfContents[i];
                    toc_entry.filenameDirectoryOffset = (uint) stream.Position; // reference 
                    
                    var directory_entry = AFS_Directory_Entry.FromStream(stream);
                    afs.directory[i] = directory_entry;
                }

                return afs; 
            }
        }



        private void ExtractFromAFS(string input, string output)
        {
            using (var reader = File.OpenRead(input))
            {
                var archive = AFS.FromStream(reader);

                Log($"header: {archive.header}\r\n"); 
                Log($"numFiles: {archive.numFiles}\r\n");

                // for (var i = 0; i < archive.numFiles; ++i)
                // {
                //     var toc_entry = archive.tableOfContents[i];
                // 
                //     Log($"length: {toc_entry.length}");
                //     Log($"offset: {toc_entry.offset}");
                // 
                //     Log($"filenameDirectoryLength: {toc_entry.filenameDirectoryLength}");
                //     Log($"filenameDirectoryOffset: {toc_entry.filenameDirectoryOffset}");
                // }

                for (var i = 0; i < archive.directory.Length; ++i)
                {
                    var toc_entry = archive.tableOfContents[i];
                    var directory_entry = archive.directory[i];
                
                    if(toc_entry.length != directory_entry.fileLength)
                    {
                        Log("toc_entry and directory_entry lengths do not match!\r\n");
                    }

                    Log($"found AFS packed file: name: {directory_entry.filename}\r\n"); 
                    Log($"with length: {directory_entry.fileLength}\r\n");
                }
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
            System.Diagnostics.Debug.WriteLine(value); 
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

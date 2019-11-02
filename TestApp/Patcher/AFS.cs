using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{

    public class AFS_TOC_Entry
    {
        public uint offset;
        public uint length;

        public static AFS_TOC_Entry FromStream(FileStream stream)
        {
            var entry = new AFS_TOC_Entry();

            entry.offset = BinaryHelper.ReadUInt32(stream);
            entry.length = BinaryHelper.ReadUInt32(stream);

            return entry;
        }

        public void WriteStream(FileStream stream)
        {
            BinaryHelper.WriteUInt32(stream, offset);
            BinaryHelper.WriteUInt32(stream, length);
        }
    }

    public class AFS_Directory_Entry
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

    public class AFS_File
    {
        public byte[] data;

        public static AFS_File FromStream(FileStream stream, uint offset, uint length)
        {
            var file = new AFS_File();
            file.data = new byte[length];

            stream.Position = offset;
            stream.Read(file.data, 0, (int)length);

            return file;
        }

        public void WriteStream(FileStream stream)
        {
            stream.Write(data, 0, data.Length);
        }
    }

    public class AFS
    {
        public string header;
        public uint numFiles;

        public AFS_TOC_Entry[] tableOfContents;

        public uint filenameDirectoryOffset;
        public uint filenameDirectoryLength;

        public AFS_File[] files;
        public AFS_Directory_Entry[] directory;

        public static AFS FromStream(FileStream stream)
        {
            var afs = new AFS();

            afs.header = BinaryHelper.ReadString(stream, 4);
            afs.numFiles = BinaryHelper.ReadUInt32(stream);

            afs.tableOfContents = new AFS_TOC_Entry[afs.numFiles];

            for (var i = 0; i < afs.numFiles; ++i)
            {
                var toc_entry = AFS_TOC_Entry.FromStream(stream);
                afs.tableOfContents[i] = toc_entry;
            }

            afs.filenameDirectoryOffset = BinaryHelper.ReadUInt32(stream);
            afs.filenameDirectoryLength = BinaryHelper.ReadUInt32(stream);

            afs.files = new AFS_File[afs.numFiles];
            for (var i = 0; i < afs.numFiles; ++i)
            {
                var toc_entry = afs.tableOfContents[i];
                var file = AFS_File.FromStream(stream, toc_entry.offset, toc_entry.length);
                afs.files[i] = file;
            }

            afs.directory = new AFS_Directory_Entry[afs.numFiles];

            // hmm 
            var dataBlockStart = afs.tableOfContents[0].offset;
            var directoryOffsetStreamStart = dataBlockStart - 8;
            stream.Position = directoryOffsetStreamStart;
            stream.Position = BinaryHelper.ReadUInt32(stream);

            // HMMM 
            // stream.Position = afs.filenameDirectoryOffset;

            for (var i = 0; i < afs.numFiles; ++i)
            {
                var toc_entry = afs.tableOfContents[i];
                var directory_entry = AFS_Directory_Entry.FromStream(stream);
                afs.directory[i] = directory_entry;
            }

            return afs;
        }

        public void WriteStream(FileStream stream)
        {
            var padding_size = 2048;

            // header 
            BinaryHelper.WriteString(stream, header, 4);
            BinaryHelper.WriteUInt32(stream, numFiles);

            // table of contents 
            for (var i = 0; i < numFiles; ++i)
            {
                var toc_entry = tableOfContents[i];
                toc_entry.WriteStream(stream);
            }

            // padding (table of contents is 512 KB)
            var tableOfContentsSize = padding_size * 256;
            BinaryHelper.WriteAFSPadding(stream, stream.Position, tableOfContentsSize);
            
            // directory information is at the very end of the table of contents block, past the padding
            stream.Position -= 8; 

            BinaryHelper.WriteUInt32(stream, filenameDirectoryOffset);
            BinaryHelper.WriteUInt32(stream, filenameDirectoryLength);
            
            // data (will need to update ToC entry's from here)
            for (var i = 0; i < numFiles; ++i)
            {
                var toc_entry = tableOfContents[i];
                toc_entry.offset = (uint)stream.Position;

                var afs_file = files[i];
                afs_file.WriteStream(stream);

                // padding 
                BinaryHelper.WriteAFSPadding(stream, stream.Position, padding_size);
            }

            filenameDirectoryOffset = (uint)stream.Position;

            // directory 
            for (var i = 0; i < numFiles; ++i)
            {
                var dir_entry = directory[i];
                dir_entry.WriteStream(stream);
            }

            filenameDirectoryLength = (uint)(stream.Position - filenameDirectoryOffset);

            // padding 
            BinaryHelper.WriteAFSPadding(stream, stream.Position, padding_size);

            // go back and re-write the table of contents, with the updated information 
            stream.Position = 8; // header = 4, numfiles = 4

            // table of contents 
            for (var i = 0; i < numFiles; ++i)
            {
                var toc_entry = tableOfContents[i];
                toc_entry.WriteStream(stream);
            }

            BinaryHelper.WriteUInt32(stream, filenameDirectoryOffset);
            BinaryHelper.WriteUInt32(stream, filenameDirectoryLength);
        }
    }
}

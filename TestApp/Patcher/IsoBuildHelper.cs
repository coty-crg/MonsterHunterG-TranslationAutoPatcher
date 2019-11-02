using IMAPI2.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{


    public static class IsoBuildHelper
    {
        // [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        // static extern int SHCreateStreamOnFileA(string pszFile, uint grfMode, out IStream stream);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false, EntryPoint = "SHCreateStreamOnFileW")]
        static extern void SHCreateStreamOnFile(string fileName, uint grfMode, out System.Runtime.InteropServices.ComTypes.IStream stream);

        // note, these won't be read by PS2? 
        //Created by Joshua Bylotas
        //http://www.ivorymatter.com
        public static void CreateISOImage(string Name, string dir, string ISOPath)
        {
            //Create File System Object to write to and set properties
            IFileSystemImage ifsi = new MsftFileSystemImage();
            ifsi.ChooseImageDefaultsForMediaType(IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER);

            ifsi.FileSystemsToCreate = FsiFileSystems.FsiFileSystemISO9660
                |  FsiFileSystems.FsiFileSystemUDF;

            // ifsi.ISO9660InterchangeLevel = 1; 
            // ifsi.UDFRevision = 0x0102;

            ifsi.VolumeName = Name;

            //string[] folders = System.IO.Directory.GetDirectories(dir);
            //string[] files = System.IO.Directory.GetFiles(dir);
            //for (int i = 0; i < folders.Count; i++)
            //{
            //    ifsi.Root.Addd
            //}



            ifsi.Root.AddTree(dir, false);

            //this will implement the Write method for the formatter
            IStream imagestream = ifsi.CreateResultImage().ImageStream;

            if (imagestream != null)
            {
                System.Runtime.InteropServices.ComTypes.STATSTG stat;
                imagestream.Stat(out stat, 0x01);

                IStream newStream;
                SHCreateStreamOnFile(ISOPath, 0x00001001, out newStream); 
                if (newStream != null)
                {
                    IntPtr inBytes = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
                    IntPtr outBytes = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
                    try
                    {
                        imagestream.CopyTo(newStream, stat.cbSize, inBytes, outBytes);
                        Marshal.ReleaseComObject(imagestream);
                        imagestream = null;
                        newStream.Commit(0);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(newStream);
                        Marshal.FreeHGlobal(inBytes);
                        Marshal.FreeHGlobal(outBytes);
                        if (imagestream != null)
                            Marshal.ReleaseComObject(imagestream);
                    }
                }
            }
            Marshal.ReleaseComObject(ifsi);

        }
        
        // note, these ARE read by PS2!
        // imgburn command line 
        public static void ImgBurnCreateISO(string imgBurnPath, string volumeName, string directoryPath, string outputFile)
        {
            // imgburn doesn't seem to overwrite existing, even though we have /OVERWRITE YES passed in
            // so, just do it ourselves 
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile); 
            }

            directoryPath = directoryPath.Replace('/', '\\');
            outputFile = outputFile.Replace('/', '\\');
            
            var command = "/MODE BUILD /BUILDINPUTMODE STANDARD /OUTPUTMODE IMAGEFILE " +
                $"/SRC \"{directoryPath}\" /DEST \"{outputFile}\" " +
                $"/FILESYSTEM \"ISO9660 + UDF\" /UDFREVISION \"1.02\" " +
                $"/START /VOLUMELABEL \"{volumeName}\" /OVERWRITE YES /ROOTFOLDER YES /CLOSEINFO /NOIMAGEDETAILS /CLOSE";

            // var process = System.Diagnostics.Process.Start(imgBurnPath, command);
            // process.WaitForExit();

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = imgBurnPath;
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

    }
}

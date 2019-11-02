# What is this?!
This is a language patcher for Monter Hunter G (PS2)! The tool dumps the ISO to disk, unpacks the archive files, decompresses those files, patches them, recompresses those files, repacks the archive files, then rebuilds a bootable ISO! 

### Run Requirements: 
  - Python: https://www.python.org/downloads/release/python-2717/ - when installing, check on "Add to PATH" at the bottom!
  - Imgburn: http://download.imgburn.com/SetupImgBurn_2.5.8.0.exe 
  - PatchFile: https://docs.google.com/spreadsheets/d/1vExiik0_Gz9Gm0joQypoTBMqZHPsyQZXINSTQmz-xOs/ 

To get the patchfile, navigate to the Google Doc (PatchFile tab) -> File -> Download -> As Comma Separated Values (.csv, current sheet). 
The patcher comes with one, but it's probably already out of date, so I highly recommend downloading another .csv from the Google Doc.

### Compile Requirements
  - Visual Studio 2017 with .NET desktop development checked on during installation (or go to Tools -> Get Tools and Features)
  - Just open the .sln and hit Build!

### PatchFile Format
We're using a Google Doc (linked above) to generate an easy to parse .csv file. The file is simply a list of comma separated values, each row in the doc is a new line in the file. Each line represents a translation, the format is:
  - OriginalText
  - TranslationFull
  - TranslationShortened
  - ArchiveName
  - Filename
  - HexOffset

If TranslationShortened is not given, the patcher will try to use the TranslationFull (possibly truncated). If TranslationFull is not given, the text will be blank, in game. If ArchiveName is not given, the patcher will look through all archives the user chooses. If FileName is not listed, the patcher will look through all files within all significant archives. If the HexOffset is not provided, re-pointering the data is not as reasonable, and so translations will be limited to the byte size of the original text plus any available padding (For MHG, the text strings seem to be padded up to the nearest 8 bytes). 

### Archive Information 
I had to figure out the AFS format. Here's what I discovered.. 

- AFS
  - Header (4 bytes, always AFS + '\0')
  - Number of Files (4bytes, uint32)
    - Table of Contents (this is a list, size equaling the number of files above)
      - Data offset (4 bytes, uint32)
      - Data length (4 bytes, uint32)
    - Directory's offset (4 bytes, uint32)
    - Directory's length (4 bytes, uint32)
  - Data (this is another list, size equaling the numer of files above, containing the files themselves)
    - Note: The data is padded to the nearest 2048th byte with 0s PER FILE.
  - File Directory (this is another list, size equaling the number of files above. After the last file, it is also padded to the nearest 2048th byte AT THE END OF THE WHOLE DIRECTORY)
    - Filename (32 bytes, terminated and padded with 0s)
    - Year (2 bytes, uint16)
    - Month (2 bytes, uint16)
    - Day (2 bytes, uint16)
    - Hour (2 bytes, uint16)
    - Minute (2 bytes, uint16)
    - Second (2 bytes, uint16)
    - File Length (4 bytes, uint32, must match the file length in the table of contents)

### File Information
The files (and archives themselves) for MHG are using SHIFT_JIS encoding. All of the files seem to be compressed, with an unknown compression algorithm. Once decompressed, the strings within are all padded to the nearest 8th byte. 

### Credits:
- Patcher by Coty: https://twitter.com/cotycrg
- Translations by Dixdros (Dixdros#0268) and vicious (viciousShadow#5130) (nice job, guys!)
- Compression algorithm perfected by the_fog (thanks!)
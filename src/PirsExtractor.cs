// C# translation of the core logic from wxPirs.MainForm

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WxTools;

namespace Pirs
{
    public struct PirsEntry
    {
        public string Filename;
        public int Unknown;
        public int BlockLen;
        public int Cluster;
        public ushort Parent;
        public int Size;
        public DateTime Timestamp;
        public bool IsDirectory => (BlockLen == 0 && Size == 0);

        public override string ToString()
        {
            return $"{(IsDirectory ? "[DIR] " : "")}{Filename} ({Size} bytes)";
        }
    }

    public class PirsExtractor
    {
        private const int MAGIC_PIRS = 0x50495253;
        private const int MAGIC_LIVE = 0x4C495645;
        private const int MAGIC_CON_ = 0x434F4E20;
        private const int BLOCK_SIZE = 4096;

        private readonly WxReader wxReader = new WxReader();
        private long pirsOffset; // This is needed by GetOffset

        public List<PirsEntry> ListEntries(string filePath)
        {
            var entries = new List<PirsEntry>();
            long pirsStart = 0;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                int magic = wxReader.ReadInt32(br);

                if (magic != MAGIC_PIRS && magic != MAGIC_LIVE && magic != MAGIC_CON_)
                {
                    throw new Exception("Not a PIRS/LIVE/CON file!");
                }
                
                // For standalone CON/LIVE files, the directory seems to always start at 0xC000.
                // The original program had complex logic likely for full device images, which is not applicable here.
                pirsStart = 0xC000;
                
                // We still need to calculate the pirsOffset for GetOffset to work correctly.
                fs.Seek(0xC030, SeekOrigin.Begin);
                int typeCheck = wxReader.ReadInt32(br);
                pirsOffset = (typeCheck == 0xFFFF) ? 0x22A0000 : 0x22C0000;
                
                fs.Seek(pirsStart, SeekOrigin.Begin);

                while (entries.Count < 5000) // Safety break
                {
                    var entry = GetEntry(br);
                    if (string.IsNullOrWhiteSpace(entry.Filename))
                    {
                        break;
                    }
                    entries.Add(entry);
                }
            }
            return entries;
        }

        public void ExtractFile(string archivePath, PirsEntry entry, string outputFilePath)
        {
            if (entry.IsDirectory) return;

            string dir = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            using (FileStream outFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(outFile))
            {
                long remainingSize = entry.Size;
                long currentCluster = entry.Cluster;

                while (remainingSize > 0)
                {
                    long offset = GetOffset(currentCluster);
                    fs.Seek(offset, SeekOrigin.Begin);

                    int bytesToRead = (int)Math.Min(remainingSize, BLOCK_SIZE);
                    byte[] data = br.ReadBytes(bytesToRead);
                    bw.Write(data);

                    remainingSize -= bytesToRead;
                    currentCluster++;
                }
            }
        }

        private long GetOffset(long cluster)
        {
            long pirsStart = 0xC000;
            long offset = pirsStart + (cluster * BLOCK_SIZE);
            long v1 = cluster / 170;
            long v2 = v1 / 170;

            if (v1 > 0)
                offset += (v1 + 1) * pirsOffset;
            if (v2 > 0)
                offset += (v2 + 1) * pirsOffset;
            
            return offset;
        }

        private PirsEntry GetEntry(BinaryReader br)
        {
            PirsEntry entry = new PirsEntry();
            
            byte[] filenameBytes = br.ReadBytes(0x26);
            entry.Filename = Encoding.ASCII.GetString(filenameBytes).TrimEnd('\0');
            
            if (string.IsNullOrWhiteSpace(entry.Filename)) return entry;

            entry.Unknown = wxReader.ReadInt32(br);
            entry.BlockLen = wxReader.ReadInt32(br);
            
            int rawCluster = br.ReadInt32();
            entry.Cluster = rawCluster >> 8;

            entry.Parent = wxReader.ReadUInt16(br);
            entry.Size = wxReader.ReadInt32(br);
            
            int dosTime = wxReader.ReadInt32(br);
            entry.Timestamp = DosDateTimeToDateTime(dosTime);

            br.ReadBytes(4); // Skip second timestamp

            return entry;
        }

        private DateTime DosDateTimeToDateTime(int dosDateTime)
        {
            if (dosDateTime == 0) return new DateTime(1980, 1, 1);

            try
            {
                int date = (dosDateTime >> 16) & 0xFFFF;
                int time = dosDateTime & 0xFFFF;

                int year = 1980 + ((date >> 9) & 0x7F);
                int month = (date >> 5) & 0x0F;
                int day = date & 0x1F;

                int hour = (time >> 11) & 0x1F;
                int minute = (time >> 5) & 0x3F;
                int second = (time & 0x1F) * 2;
                
                if (month == 0 || day == 0) return new DateTime(1980, 1, 1);
                return new DateTime(year, month, day, hour, minute, second);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new DateTime(1980, 1, 1);
            }
        }
    }
}
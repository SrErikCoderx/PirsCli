// C# translation of the WxTools.WxReader IL code

using System;
using System.IO;
using System.Text;

namespace WxTools
{
    public class WxReader
    {
        // Replicates the logic of reading a big-endian int16
        public short ReadInt16(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        // Replicates the logic of reading a big-endian uint16
        public ushort ReadUInt16(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        // Replicates the logic of reading a big-endian int32
        public int ReadInt32(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        // Replicates the logic of reading a big-endian uint32
        public uint ReadUInt32(BinaryReader br)
        {
            byte[] bytes = br.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
        
        // Replicates the logic of reading a fixed-size, null-terminated string
        public string ReadString(BinaryReader br, uint length)
        {
            byte[] bytes = br.ReadBytes((int)length);
            string s = Encoding.ASCII.GetString(bytes);
            int nullPos = s.IndexOf('\0');
            if (nullPos != -1)
            {
                return s.Substring(0, nullPos);
            }
            return s;
        }

        // This is a direct translation of the quirky unicodeToStr IL
        public string UnicodeToStr(byte[] data, int start, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < start + length && i < data.Length; i += 2)
            {
                if (data[i] == 0 && (i + 1 < data.Length && data[i+1] == 0))
                {
                    break;
                }
                if (i + 1 >= data.Length) break;
                
                // This seems to be the intended logic: treat two bytes as a little-endian char
                char c = BitConverter.ToChar(data, i);
                if (c == '\0') break;

                sb.Append(c);
            }
            return sb.ToString();
        }

        public string ExtractFileName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;
            int lastSlash = fullName.LastIndexOf('\\');
            if (lastSlash == -1)
                return fullName;
            return fullName.Substring(lastSlash + 1);
        }

        public string ExtractFolderName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;
            int lastSlash = fullName.LastIndexOf('\\');
            if (lastSlash == -1)
                return string.Empty;
            return fullName.Substring(0, lastSlash);
        }
    }
}

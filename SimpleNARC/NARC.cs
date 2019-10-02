using Kermalis.EndianBinaryIO;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Kermalis.SimpleNARC
{
    public sealed class NARC : IReadOnlyList<byte[]>
    {
        private readonly byte[][] _files;

        public int Count => _files.Length;
        public byte[] this[int fileNum] => _files[fileNum];

        public NARC(string path) : this(File.OpenRead(path)) { }
        public NARC(Stream stream)
        {
            using (var r = new EndianBinaryReader(stream, Endianness.LittleEndian))
            {
                int numFiles = r.ReadInt32(0x18);
                _files = new byte[numFiles][];
                uint[] startOffsets = new uint[numFiles];
                uint[] endOffsets = new uint[numFiles];
                for (int i = 0; i < numFiles; i++)
                {
                    startOffsets[i] = r.ReadUInt32();
                    endOffsets[i] = r.ReadUInt32();
                }
                long BTNFOffset = r.BaseStream.Position;
                long GMIFOffset = r.ReadUInt32(BTNFOffset + 0x4) + BTNFOffset;
                for (int i = 0; i < numFiles; i++)
                {
                    _files[i] = r.ReadBytes((int)(endOffsets[i] - startOffsets[i]), GMIFOffset + startOffsets[i] + 0x8);
                }
            }
        }

        public void SaveFiles(string path, string extension = "")
        {
            Directory.CreateDirectory(path);
            for (int i = 0; i < _files.Length; i++)
            {
                string fileName = i.ToString();
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    fileName += "." + extension;
                }
                using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create, FileAccess.Write))
                {
                    byte[] file = _files[i];
                    fs.Write(file, 0, file.Length);
                }
            }
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            for (int i = 0; i < _files.Length; i++)
            {
                yield return _files[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

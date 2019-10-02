using Kermalis.EndianBinaryIO;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Kermalis.SimpleNARC
{
    public sealed class NARC : IDisposable
    {
        private bool _isDisposed = false;
        private readonly MemoryStream[] _files;
        public ReadOnlyCollection<MemoryStream> Files { get; }

        public NARC(string path) : this(File.OpenRead(path)) { }
        public NARC(Stream stream)
        {
            using (var r = new EndianBinaryReader(stream, Endianness.LittleEndian))
            {
                uint numFiles = r.ReadUInt32(0x18);
                _files = new MemoryStream[numFiles];
                uint[] startOffsets = new uint[numFiles];
                uint[] endOffsets = new uint[numFiles];
                for (uint i = 0; i < numFiles; i++)
                {
                    startOffsets[i] = r.ReadUInt32();
                    endOffsets[i] = r.ReadUInt32();
                }
                long BTNFOffset = r.BaseStream.Position;
                long GMIFOffset = r.ReadUInt32(BTNFOffset + 0x4) + BTNFOffset;
                for (uint i = 0; i < numFiles; i++)
                {
                    _files[i] = new MemoryStream(r.ReadBytes((int)(endOffsets[i] - startOffsets[i]), GMIFOffset + startOffsets[i] + 0x8));
                }
                Files = new ReadOnlyCollection<MemoryStream>(_files);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                for (int i = 0; i < _files.Length; i++)
                {
                    _files[i].Dispose();
                }
            }
        }

        public void SaveFilesToFolder(string path)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            Directory.CreateDirectory(path);
            for (int i = 0; i < _files.Length; i++)
            {
                using (var fs = new FileStream(path + Path.DirectorySeparatorChar + i, FileMode.Create, FileAccess.Write))
                {
                    _files[i].WriteTo(fs);
                }
            }
        }
    }
}

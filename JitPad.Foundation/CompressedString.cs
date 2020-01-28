using System;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;

namespace JitPad.Foundation
{
    public struct CompressedString : IEquatable<CompressedString>
    {
        private readonly byte[] _compressedSource;

        public bool Equals(CompressedString other) => _compressedSource.Equals(other._compressedSource);
        public override bool Equals(object? obj) => obj is CompressedString other && Equals(other);
        public override int GetHashCode() => _compressedSource.GetHashCode();

        public CompressedString(string source)
            => _compressedSource = LZ4Pickler.Pickle(MemoryMarshal.Cast<char, byte>(source.AsSpan()));

        public override string ToString()
            => new string(MemoryMarshal.Cast<byte, char>(LZ4Pickler.Unpickle(_compressedSource)));
    }
}
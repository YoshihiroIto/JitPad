using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace JitPad.Foundation
{
    public static class EmbeddedResourceReader
    {
        public static T Read<T>(Assembly asm, string name)
        {
            using var stream = asm.GetManifestResourceStream(name) ?? throw new NullReferenceException();
            using var reader = new BinaryReader(stream);

            var buffer = ArrayPool<byte>.Shared.Rent((int) stream.Length);

            try
            {
                reader.Read(buffer, 0, (int) stream.Length);

                return JsonSerializer.Deserialize<T>(buffer.AsSpan(0, (int) stream.Length));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
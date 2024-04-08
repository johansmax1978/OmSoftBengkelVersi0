#if JSON
namespace Newtonsoft.Json
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides forward only partials BASE-64 encoder for <see cref="JsonTextWriter"/> or <see cref="TextWriter"/> output. This class is <see langword="sealed"/>.
    /// </summary>
    public sealed partial class JsonBase64Encoder
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly FieldInfo FieldWriter;

        private static readonly MethodInfo ConvertBase64;

        /// <summary>
        /// <c>private unsafe static int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)</c>
        /// </summary>
        private static readonly unsafe delegate*<Char*, Byte*, Int32, Int32, Boolean, Int32> ConvertBase64Func;

        static JsonBase64Encoder()
        {
            if ((FieldWriter = typeof(JsonTextWriter).GetField("_writer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase)) is null)
            {
                foreach (var field in typeof(JsonTextWriter).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(TextWriter) || typeof(TextWriter).IsAssignableFrom(field.FieldType))
                    {
                        FieldWriter = field;
                        break;
                    }
                }
            }
            unsafe
            {
                ConvertBase64 = typeof(Convert).GetMethod("ConvertToBase64Array", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly, Type.DefaultBinder,
                new Type[] { typeof(Char*), typeof(Byte*), typeof(Int32), typeof(Int32), typeof(Boolean) }, null);
                ConvertBase64Func = (delegate*<Char*, Byte*, Int32, Int32, Boolean, Int32>)ConvertBase64.MethodHandle.GetFunctionPointer();
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly JsonTextWriter _jwriter;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TextWriter _writer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Char[] _chars;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Byte[] _lfbytes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Int32 _lfcount;

        public JsonBase64Encoder(JsonTextWriter writer)
        {
            const Int32 Base64LineSize = 76;
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer), $"The output {nameof(JsonTextWriter)} must not be null reference.");
            }
            this._jwriter = writer;
            this._writer = (TextWriter)FieldWriter.GetValueDirect(__makeref(writer));
            this._chars = new Char[Base64LineSize];
        }

        public JsonBase64Encoder(TextWriter writer)
        {
            const Int32 Base64LineSize = 76;
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer), $"The output {nameof(TextWriter)} must not be null reference.");
            }
            this._jwriter = null;
            this._writer = writer;
            this._chars = new Char[Base64LineSize];
        }

        public TextWriter Output => this._writer;

        private Boolean TakeLeftover(Byte[] buffer, Int32 index, ref Int32 count)
        {
            var leftover = this._lfcount;
            var bytes = this._lfbytes;
            while ((leftover < 3) && (count > 0))
            {
                bytes[leftover++] = buffer[index++];
                count--;
            }
            if ((count == 0) && (leftover < 3))
            {
                this._lfcount = leftover;
                return true;
            }
            return false;
        }

        private unsafe Boolean TakeLeftover(Byte* buffer, nint index, ref nint count)
        {
            var leftover = this._lfcount;
            var bytes = this._lfbytes;
            while ((leftover < 3) && (count > 0))
            {
                bytes[leftover++] = buffer[index++];
                count--;
            }
            if ((count == 0) && (leftover < 3))
            {
                this._lfcount = leftover;
                return true;
            }
            return false;
        }

        private void StoreLeftover(Byte[] buffer, Int32 index, ref Int32 count)
        {
            var modulo = count % 3;
            if (modulo > 0)
            {
                count -= modulo;
                var bytes = this._lfbytes ??= new Byte[3];
                for (var i = 0; i < modulo; i++)
                {
                    bytes[i] = buffer[(index + count) + i];
                }
            }
            this._lfcount = modulo;
        }

        private unsafe void StoreLeftover(Byte* buffer, nint index, ref nint count)
        {
            var modulo = count % 3;
            if (modulo > 0)
            {
                count -= modulo;
                var bytes = this._lfbytes ??= new Byte[3];
                for (var i = 0; i < modulo; i++)
                {
                    bytes[i] = buffer[(index + count) + i];
                }
            }
            this._lfcount = (Int32)modulo;
        }

        public void Encode(ArraySegment<Byte> segment)
            => this.Encode(segment.Array, segment.Offset, segment.Count);

        public void Encode(Byte[] buffer, Int32 index, Int32 count)
        {
            if (buffer is null || count < 1)
            {
                return;
            }
            const Int32 LineSizeInBytes = 57;
            var writer = this._writer;
            var chars = this._chars;
            Int32 number;
            if (this._lfcount > 0)
            {
                if (this.TakeLeftover(buffer, index, ref count))
                {
                    return;
                }
                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                writer.Write(chars, 0, number);
            }
            this.StoreLeftover(buffer, index, ref count);
            Int32 ubound = index + count, packet = LineSizeInBytes;
            while (index < ubound)
            {
                if ((index + packet) > ubound)
                {
                    packet = ubound - index;
                }
                number = Convert.ToBase64CharArray(buffer, index, packet, chars, 0);
                writer.Write(chars, 0, number);
                index += packet;
            }
        }

#if CLS
        [CLSCompliant(false)]
#endif
        public unsafe void Encode(Byte* buffer, nint count)
        {
            if (buffer is null || count < 1) return;
            const Int32 LineSizeInBytes = 57;
            var writer = this._writer;
            var chars = this._chars;
            Int32 number;
            if (this._lfcount > 0)
            {
                if (this.TakeLeftover(buffer, 0, ref count))
                {
                    return;
                }
                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0, Base64FormattingOptions.None);
                writer.Write(chars, 0, number);
            }
            this.StoreLeftover(buffer, 0, ref count);
            var packet = LineSizeInBytes;
            fixed (Char* charptr = chars)
            {
                var funcptr = ConvertBase64Func;
                var index = (nint)0;
                while (index < count)
                {
                    if ((index + packet) > count)
                    {
                        packet = (Int32)(count - index);
                    }
                    number = funcptr(charptr, buffer + index, 0, packet, false);
                    writer.Write(chars, 0, number);
                    index += packet;
                }
            }
        }

        public Task EncodeAsync(ArraySegment<Byte> segment)
            => this.EncodeAsync(segment.Array, segment.Offset, segment.Count, CancellationToken.None);

        public Task EncodeAsync(ArraySegment<Byte> segment, CancellationToken ctoken)
            => this.EncodeAsync(segment.Array, segment.Offset, segment.Count, ctoken);

        public Task EncodeAsync(Byte[] buffer, Int32 index, Int32 count)
            => this.EncodeAsync(buffer, index, count, CancellationToken.None);

        public async Task EncodeAsync(Byte[] buffer, Int32 index, Int32 count, CancellationToken ctoken)
        {
            if (buffer is null || count < 1)
            {
                return;
            }
            const Int32 LineSizeInBytes = 57;
            ctoken.ThrowIfCancellationRequested();
            var writer = this._writer;
            var chars = this._chars;
            Int32 number;
            if (this._lfcount > 0)
            {
                if (this.TakeLeftover(buffer, index, ref count))
                {
                    return;
                }
                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
            }
            this.StoreLeftover(buffer, index, ref count);
            Int32 ubound = index + count, packet = LineSizeInBytes;
            if (ctoken.CanBeCanceled)
            {
                while (index < ubound)
                {
                    ctoken.ThrowIfCancellationRequested();
                    if ((index + packet) > ubound)
                    {
                        packet = ubound - index;
                    }
                    number = Convert.ToBase64CharArray(buffer, index, packet, chars, 0);
                    await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                    index += packet;
                }
            }
            else
            {
                while (index < ubound)
                {
                    if ((index + packet) > ubound)
                    {
                        packet = ubound - index;
                    }
                    number = Convert.ToBase64CharArray(buffer, index, packet, chars, 0);
                    await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                    index += packet;
                }
            }
        }

        public Int64 Encode(Stream stream)
            => this.Encode(stream, null);

        public Int64 Encode(Stream stream, Int64? length)
        {
            Int64 acquired = 0;
            if (stream is not null && stream.CanRead)
            {
                var require = length.GetValueOrDefault(-1);
                if (require != 0)
                {
                    const Int32 LineSizeInBytes = 57, BufferSize = 3000;
                    Int32 readed, number;
                    var bucket = new Byte[8192];
                    var writer = this._writer;
                    var chars = this._chars;
                    if (require < 0)
                    {
                        while ((readed = stream.Read(bucket, 0, BufferSize)) > 0)
                        {
                            acquired += readed;
                            if (this._lfcount > 0)
                            {
                                if (this.TakeLeftover(bucket, 0, ref readed))
                                {
                                    continue;
                                }
                                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                                writer.Write(chars, 0, number);
                            }
                            this.StoreLeftover(bucket, 0, ref readed);
                            Int32 index = 0, ubound = readed, packet = LineSizeInBytes;
                            while (index < ubound)
                            {
                                if ((index + packet) > ubound)
                                {
                                    packet = ubound - index;
                                }
                                number = Convert.ToBase64CharArray(bucket, index, packet, chars, 0);
                                writer.Write(chars, 0, number);
                                index += packet;
                            }
                        }
                    }
                    else
                    {
                        while (require > 0)
                        {
                            if (require < BufferSize)
                            {
                                readed = stream.Read(bucket, 0, (Int32)require);
                                require = 0;
                            }
                            else
                            {
                                readed = stream.Read(bucket, 0, BufferSize);
                                require -= BufferSize;
                            }
                            if (readed < 1)
                            {
                                break;
                            }
                            acquired += readed;
                            if (this._lfcount > 0)
                            {
                                if (this.TakeLeftover(bucket, 0, ref readed))
                                {
                                    continue;
                                }
                                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                                writer.Write(chars, 0, number);
                            }
                            this.StoreLeftover(bucket, 0, ref readed);
                            Int32 index = 0, ubound = readed, packet = LineSizeInBytes;
                            while (index < ubound)
                            {
                                if ((index + packet) > ubound)
                                {
                                    packet = ubound - index;
                                }
                                number = Convert.ToBase64CharArray(bucket, index, packet, chars, 0);
                                writer.Write(chars, 0, number);
                                index += packet;
                            }
                        }
                    }
                }
            }
            return acquired;
        }

        public Task<Int64> EncodeAsync(Stream stream)
            => this.EncodeAsync(stream, null, CancellationToken.None);

        public Task<Int64> EncodeAsync(Stream stream, CancellationToken ctoken)
            => this.EncodeAsync(stream, null, ctoken);

        public Task<Int64> EncodeAsync(Stream stream, Int64? length)
            => this.EncodeAsync(stream, length, CancellationToken.None);

        public async Task<Int64> EncodeAsync(Stream stream, Int64? length, CancellationToken ctoken)
        {
            Int64 acquired = 0;
            if (stream is not null && stream.CanRead && !ctoken.IsCancellationRequested)
            {
                var require = length.GetValueOrDefault(-1);
                if (require != 0)
                {
                    const Int32 LineSizeInBytes = 57, BufferSize = 3000;
                    Int32 readed, number;
                    var bucket = new Byte[8192];
                    var writer = this._writer;
                    var chars = this._chars;
                    if (require < 0)
                    {
                        while (!ctoken.IsCancellationRequested && (readed = await stream.ReadAsync(bucket, 0, BufferSize, ctoken).ConfigureAwait(false)) > 0 && !ctoken.IsCancellationRequested)
                        {
                            acquired += readed;
                            if (this._lfcount > 0)
                            {
                                if (this.TakeLeftover(bucket, 0, ref readed))
                                {
                                    continue;
                                }
                                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                                await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                                if (ctoken.IsCancellationRequested) break;
                            }
                            this.StoreLeftover(bucket, 0, ref readed);
                            Int32 index = 0, ubound = readed, packet = LineSizeInBytes;
                            while (index < ubound)
                            {
                                if ((index + packet) > ubound)
                                {
                                    packet = ubound - index;
                                }
                                number = Convert.ToBase64CharArray(bucket, index, packet, chars, 0);
                                await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                                index += packet;
                                if (ctoken.IsCancellationRequested) break;
                            }
                        }
                    }
                    else
                    {
                        while (!ctoken.IsCancellationRequested && require > 0)
                        {
                            if (require < BufferSize)
                            {
                                readed = await stream.ReadAsync(bucket, 0, (Int32)require, ctoken).ConfigureAwait(false);
                                require = 0;
                            }
                            else
                            {
                                readed = await stream.ReadAsync(bucket, 0, BufferSize, ctoken).ConfigureAwait(false);
                                require -= BufferSize;
                            }
                            if (readed < 1)
                            {
                                break;
                            }
                            acquired += readed;
                            if (this._lfcount > 0)
                            {
                                if (this.TakeLeftover(bucket, 0, ref readed))
                                {
                                    continue;
                                }
                                number = Convert.ToBase64CharArray(this._lfbytes, 0, 3, chars, 0);
                                await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                            }
                            this.StoreLeftover(bucket, 0, ref readed);
                            Int32 index = 0, ubound = readed, packet = LineSizeInBytes;
                            while (index < ubound)
                            {
                                if ((index + packet) > ubound)
                                {
                                    packet = ubound - index;
                                }
                                number = Convert.ToBase64CharArray(bucket, index, packet, chars, 0);
                                await writer.WriteAsync(chars, 0, number).ConfigureAwait(false);
                                index += packet;
                                if (ctoken.IsCancellationRequested) break;
                            }
                        }
                    }
                }
            }
            return acquired;
        }

        public void Flush()
        {
            if (this._lfcount > 0)
            {
                var number = Convert.ToBase64CharArray(this._lfbytes, 0, this._lfcount, this._chars, 0);
                this._writer.Write(this._chars, 0, number);
                this._lfcount = 0;
            }
        }

        public async Task FlushAsync(CancellationToken ctoken)
        {
            ctoken.ThrowIfCancellationRequested();
            if (this._lfcount > 0)
            {
                var number = Convert.ToBase64CharArray(this._lfbytes, 0, this._lfcount, this._chars, 0);
                await this._writer.WriteAsync(this._chars, 0, number).ConfigureAwait(false);
                this._lfcount = 0;
            }
        }
    }
}
#endif
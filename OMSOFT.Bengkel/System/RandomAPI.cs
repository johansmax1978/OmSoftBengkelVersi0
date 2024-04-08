namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a random data or number generator engine that using cryptographic service, this class is <see langword="sealed"/>.
    /// </summary>
    public sealed partial class RandomAPI
    {
        /// <summary>
        /// Represents a vector of 32-bit integer number, allow you to access the data unsafely through memory.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed unsafe partial class IntVector
        {
            public readonly Byte[] Array;
            internal GCHandle Handle;
            public readonly Int32 Length;

#if CLS
            [CLSCompliant(false)]
#endif
            public readonly Byte* Buffer;

#if CLS
            [CLSCompliant(false)]
#endif
            public readonly UInt32* UInt32;

#if CLS
            [CLSCompliant(false)]
#endif
            public readonly Int32* Vector;

            public Boolean IsValid => this.Length > 0 && this.Handle.IsAllocated;

            public ref Int32 this[Int32 index] => ref this.Vector[index];

            public ref Byte this[Int32 index, Byte offset] => ref this.Buffer[(index * 4) + offset];

#if CLS
            [CLSCompliant(false)]
#endif
            public Int32 Copy(void* output)
            {
                if (output is not null)
                {
                    var length = this.Length * 4;
                    if (length != 0)
                    {
                        System.Buffer.MemoryCopy(this.Buffer, output, length, length);
                        return length / 4;
                    }
                }
                return 0;
            }

#if CLS
            [CLSCompliant(false)]
#endif
            public Int32 Copy(void* output, Int64 offset)
            {
                if (output is not null)
                {
                    var dstptr = ((Byte*)output) + offset;
                    var length = this.Length * 4;
                    if (length != 0)
                    {
                        System.Buffer.MemoryCopy(this.Buffer, dstptr, length, length);
                        return length / 4;
                    }
                }
                return 0;
            }

            public Int32 Copy(IntVector other)
            {
                if (other is not null)
                {
                    var length = this.Length;
                    if (length != 0)
                    {
                        if (ReferenceEquals(other, this)) return length;
                        var avail = other.Length;
                        if (avail < length) length = avail;
                        if (length != 0)
                        {
                            length *= 4;
                            System.Buffer.MemoryCopy(this.Buffer, other.Buffer, length, length);
                            return length / 4;
                        }
                    }
                }
                return 0;
            }

            public Int32 Copy(IntVector other, Int32 index)
            {
                if (other is not null)
                {
                    if (index < 0)
                    {
                        index = other.Length + index;
                    }
                    if (index < 0 || index >= other.Length) return 0;
                    var length = this.Length;
                    if (length != 0)
                    {
                        var avails = other.Length - index;
                        if (avails < length) length = avails;
                        if (length != 0)
                        {
                            length *= 4;
                            System.Buffer.MemoryCopy(this.Buffer, other.Buffer + (index * 4), length, length);
                            return length / 4;
                        }
                    }
                }
                return 0;
            }

            public Int32 Copy(Byte[] array)
                => this.Copy(array, 0);

            public Int32 Copy(Byte[] array, Int32 index)
            {
                if (array is not null)
                {
                    var avail = array.Length / 4;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 4;
                        fixed (Byte* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 4;
                    }
                }
                return 0;
            }

            public Int32 Copy(Int32[] array)
                => this.Copy(array, 0);

            public Int32 Copy(Int32[] array, Int32 index)
            {
                if (array is not null)
                {
                    var avail = array.Length;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 4;
                        fixed (Int32* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 4;
                    }
                }
                return 0;
            }

            public Int32 Copy<T>(T[] array) where T : unmanaged
                => this.Copy(array, 0);

            public Int32 Copy<T>(T[] array, Int32 index) where T : unmanaged
            {
                if (array is not null)
                {
                    var avail = (array.Length * sizeof(T)) / 4;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 4;
                        fixed (T* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 4;
                    }
                }
                return 0;
            }

            public IntVector Clone() => new(this, true);

            ~IntVector()
            {
                if (this.Handle.IsAllocated)
                {
                    this.Handle.Free();
                    this.Handle = new GCHandle();
                }
            }

            private IntVector(IntVector source, Boolean clone)
            {
                if (clone)
                {
                    if (source.Length == 0)
                    {
                        this.Array = source.Array;
                    }
                    else
                    {
                        this.Array = (Byte[])source.Array.Clone();
                        this.Length = source.Length;
                        this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                        this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                        this.UInt32 = (UInt32*)this.Buffer;
                        this.Vector = (Int32*)this.Buffer;
                    }
                }
                else
                {
                    this.Array = source.Array;
                    this.Buffer = source.Buffer;
                    this.Length = source.Length;
                    this.Handle = source.Handle;
                    this.UInt32 = source.UInt32;
                    this.Vector = source.Vector;
                }
            }

            public IntVector()
            {
                this.Array = new Byte[4];
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = 1;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.UInt32 = (UInt32*)this.Buffer;
                this.Vector = (Int32*)this.Buffer;
            }

            public IntVector(Int32 length)
            {
                if (length < 1) length = 1;
                this.Array = new Byte[length * 4];
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.UInt32 = (UInt32*)this.Buffer;
                this.Vector = (Int32*)this.Buffer;
            }

            public IntVector(Int32[] source)
            {
                var length = source is null ? 0 : source.Length;
                if (length < 1)
                    throw new ArgumentException("The source of integer array to copy must have at least one integer element.", nameof(source));
                var need = length * 4;
                var array = new Byte[need];
                fixed (Byte* dstptr = array)
                fixed (Int32* srcptr = source)
                    System.Buffer.MemoryCopy(srcptr, dstptr, need, need);
                this.Array = array;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.UInt32 = (UInt32*)this.Buffer;
                this.Vector = (Int32*)this.Buffer;
            }

            public IntVector(Byte[] bytes)
                : this(bytes, false) {; }

            public IntVector(Byte[] bytes, Boolean clone)
            {
                if (bytes is null)
                    throw new ArgumentNullException(nameof(bytes), clone ? "The source of bytes array to copy into the new integer vector must not be null reference." : "The source of bytes array to pin as integer vector must not be null reference.");
                var length = bytes.Length;
                if (length < 4)
                    throw new ArgumentException(clone ? "The source of bytes array to copy into the new integer vector must have at least 4-bytes data." : "The source of bytes array to pin as integer vector must have at least 4-bytes data.", nameof(bytes));
                this.Array = clone ? (Byte[])bytes.Clone() : bytes;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length / 4;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.UInt32 = (UInt32*)this.Buffer;
                this.Vector = (Int32*)this.Buffer;
            }

            public IntVector(IEnumerable<Int32> values)
            {
                var source = Commons.CastArray(values);
                var length = source.Length;
                if (length == 0) throw new ArgumentException("Cannot allocate a new integer vector to store empty sequence, the enumerable must have at least one element in the sequence.", nameof(values));
                var need = length * 4;
                var array = new Byte[need];
                fixed (Byte* dstptr = array)
                fixed (Int32* srcptr = source)
                    System.Buffer.MemoryCopy(srcptr, dstptr, need, need);
                this.Array = array;
                this.Length = array.Length;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.UInt32 = (UInt32*)this.Buffer;
                this.Vector = (Int32*)this.Buffer;
            }

            public ref Int32 Random()
            {
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Vector[0];
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var engine = Commons.DefaultEngine;
                        var bucket = Commons.DefaultBucket;
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Vector[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }

            internal ref Int32 Random(RandomNumberGenerator engine, IntVector bucket)
            {
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Vector[0];
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Vector[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Represents a vector of 16-bit characters code values, allow you to access the data unsafely through memory.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed unsafe partial class CharData
        {
            public readonly Array Array;
            internal GCHandle Handle;
            public readonly Int32 Length;

#if CLS
            [CLSCompliant(false)]
#endif
            public readonly Byte* Buffer;

#if CLS
            [CLSCompliant(false)]
#endif
            public readonly Char* Vector;

            public Boolean IsValid => this.Length > 0 && this.Handle.IsAllocated;

            public ref Char this[Int32 index] => ref this.Vector[index];

            public ref Byte this[Int32 index, Byte offset] => ref this.Buffer[(index * 2) + offset];

#if CLS
            [CLSCompliant(false)]
#endif
            public Int32 Copy(void* output)
            {
                if (output is not null)
                {
                    var length = this.Length * 2;
                    if (length != 0)
                    {
                        System.Buffer.MemoryCopy(this.Buffer, output, length, length);
                        return length / 2;
                    }
                }
                return 0;
            }

#if CLS
            [CLSCompliant(false)]
#endif
            public Int32 Copy(void* output, Int64 offset)
            {
                if (output is not null)
                {
                    var dstptr = ((Byte*)output) + offset;
                    var length = this.Length * 2;
                    if (length != 0)
                    {
                        System.Buffer.MemoryCopy(this.Buffer, dstptr, length, length);
                        return length / 2;
                    }
                }
                return 0;
            }

            public Int32 Copy(CharData other)
            {
                if (other is not null)
                {
                    var length = this.Length;
                    if (length != 0)
                    {
                        if (ReferenceEquals(other, this)) return length;
                        var avail = other.Length;
                        if (avail < length) length = avail;
                        if (length != 0)
                        {
                            length *= 2;
                            System.Buffer.MemoryCopy(this.Buffer, other.Buffer, length, length);
                            return length / 2;
                        }
                    }
                }
                return 0;
            }

            public Int32 Copy(CharData other, Int32 index)
            {
                if (other is not null)
                {
                    if (index < 0)
                    {
                        index = other.Length + index;
                    }
                    if (index < 0 || index >= other.Length) return 0;
                    var length = this.Length;
                    if (length != 0)
                    {
                        var avails = other.Length - index;
                        if (avails < length) length = avails;
                        if (length != 0)
                        {
                            length *= 2;
                            System.Buffer.MemoryCopy(this.Buffer, other.Buffer + (index * 2), length, length);
                            return length / 2;
                        }
                    }
                }
                return 0;
            }

            public Int32 Copy(Byte[] array)
                => this.Copy(array, 0);

            public Int32 Copy(Byte[] array, Int32 index)
            {
                if (array is not null)
                {
                    var avail = array.Length / 2;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 2;
                        fixed (Byte* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 2;
                    }
                }
                return 0;
            }

            public Int32 Copy(Char[] array)
                => this.Copy(array, 0);

            public Int32 Copy(Char[] array, Int32 index)
            {
                if (array is not null)
                {
                    var avail = array.Length;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 2;
                        fixed (Char* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 2;
                    }
                }
                return 0;
            }

            public Int32 Copy<T>(T[] array) where T : unmanaged
                => this.Copy(array, 0);

            public Int32 Copy<T>(T[] array, Int32 index) where T : unmanaged
            {
                if (array is not null)
                {
                    var avail = (array.Length * sizeof(T)) / 2;
                    if (index < 0) index = avail + index;
                    if (index < 0 || index >= avail) return 0;
                    avail -= index;
                    var length = this.Length;
                    if (avail < length) length = avail;
                    if (length != 0)
                    {
                        length *= 2;
                        fixed (T* intptr = &array[index])
                            System.Buffer.MemoryCopy(this.Buffer, intptr, length, length);
                        return length / 2;
                    }
                }
                return 0;
            }

            public CharData()
            {
                this.Array = new Byte[4];
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = 1;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public CharData(Int32 length)
            {
                if (length < 1) length = 1;
                this.Array = new Byte[length * 4];
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public CharData(Char[] source)
                : this(source, false) {; }

            public CharData(Char[] source, Boolean clone)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source), clone ? "The source of characters array to copy into the new unicode characters vector must not be null reference." : "The source of characters array to pin as unicode characters vector must not be null reference.");
                var length = source.Length;
                if (length == 0)
                    throw new ArgumentException(clone ? "The source of characters array to copy into the new unicode characters vector must not set with empty array." : "The source of characters array to pin as unicode characters vector must not set with empty array.", nameof(source));
                this.Array = clone ? (Int32[])source.Clone() : source;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public CharData(Byte[] bytes)
                : this(bytes, false) {; }

            public CharData(Byte[] bytes, Boolean clone)
            {
                if (bytes is null)
                    throw new ArgumentNullException(nameof(bytes), clone ? "The source of bytes array to copy into the new unicode characters vector must not be null reference." : "The source of bytes array to pin as unicode characters vector must not be null reference.");
                var length = bytes.Length;
                if (length < 2)
                    throw new ArgumentException(clone ? "The source of bytes array to copy into the new unicode characters vector must have at least 2-bytes data." : "The source of bytes array to pin as unicode characters vector must have at least 4-bytes data.", nameof(bytes));
                this.Array = clone ? (Byte[])bytes.Clone() : bytes;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = length / 2;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public CharData(IEnumerable<Char> values)
            {
                var array = Commons.CastArray(values);
                if (array.Length == 0) throw new ArgumentException("Cannot allocate a new unicode characters vector to store empty sequence, the enumerable must have at least one element in the sequence.", nameof(values));
                this.Array = array;
                this.Length = array.Length;
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public CharData(String source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source), "The source of string to copy into the new unicode characters vector must not be null reference.");
                var length = source.Length;
                if (length == 0)
                    throw new ArgumentException("The source of string to copy into the new unicode characters vector must not set with empty string .", nameof(source));

                this.Array = source.ToCharArray();
                this.Handle = GCHandle.Alloc(this.Array, GCHandleType.Pinned);
                this.Length = source.Length;
                this.Buffer = (Byte*)this.Handle.AddrOfPinnedObject();
                this.Vector = (Char*)this.Buffer;
            }

            public ref Char Random()
            {
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Vector[0];
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var engine = Commons.DefaultEngine;
                        var bucket = Commons.DefaultBucket;
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Vector[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }

            internal ref Char Random(RandomNumberGenerator engine, IntVector bucket)
            {
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Vector[0];
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Vector[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Represents a set of <see cref="CharData"/> which support to get the <see cref="Char"/> element randomly from each sets.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed unsafe partial class CharMaps
        {
            /// <summary>
            /// Represents the array of <see cref="CharData"/> that stored in the current map.
            /// </summary>
            public readonly CharData[] Entries;

            /// <summary>
            /// Represents the total count or length of the <see cref="Entries"/> array in this map.
            /// </summary>
            public readonly Int32 Length;

            public CharMaps(CharData[] entries)
            {
                var length = entries is null ? 0 : entries.Length;
                if (length == 0) throw new ArgumentException("The characters data entries to store in the characters mapping must not be null or empty.", nameof(entries));
                this.Entries = entries;
                this.Length = entries.Length;
            }

            public CharData NextSet()
            {
                var length = this.Length;
                if (length == 1)
                {
                    return this.Entries[0];
                }
                else
                {
                    var engine = Commons.DefaultEngine;
                    var bucket = Commons.DefaultBucket;
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return this.Entries[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }

            public CharData NextSet(RandomNumberGenerator engine, IntVector bucket)
            {
                var length = this.Length;
                if (length == 1)
                {
                    return this.Entries[0];
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return this.Entries[(Int32)(next % diff)];
                            }
                        }
                    }
                }
            }

            public ref Char Random()
            {
                var engine = Commons.DefaultEngine;
                var bucket = Commons.DefaultBucket;
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Entries[0].Random(engine, bucket);
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Entries[(Int32)(next % diff)].Random(engine, bucket);
                            }
                        }
                    }
                }
            }

            internal ref Char Random(RandomNumberGenerator engine, IntVector bucket)
            {
                var length = this.Length;
                if (length == 1)
                {
                    return ref this.Entries[0].Random(engine, bucket);
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = 0L - length;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return ref this.Entries[(Int32)(next % diff)].Random(engine, bucket);
                            }
                        }
                    }
                }
            }

            public static CharMaps Extract(MixedChars chars) => Extract(chars, true);

            public static CharMaps Extract(MixedChars chars, Boolean forKey)
            {
                if (chars <= MixedChars.Default)
                    chars = forKey ? MixedChars.Lowers | MixedChars.Uppers | MixedChars.Number : MixedChars.Uppers | MixedChars.Number;
                else if (chars > MixedChars.AllParts)
                    chars = MixedChars.AllParts;
                if (!ExtractCaches.TryGetValue(chars, out var maps))
                {
                    var buffer = ExtractBuffer;
                    if ((chars & MixedChars.Lowers) != MixedChars.Default)
                        buffer.Add(Commons.CharLowers);
                    if ((chars & MixedChars.Uppers) != MixedChars.Default)
                        buffer.Add(Commons.CharUppers);
                    if ((chars & MixedChars.Number) != MixedChars.Default)
                        buffer.Add(Commons.CharNumber);
                    if ((chars & MixedChars.Symbols) != MixedChars.Default)
                        buffer.Add(Commons.CharSymbol);
                    ExtractCaches[chars] = maps = new CharMaps(buffer.ToArray());
                    buffer.Clear();
                }
                return maps;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static readonly Dictionary<MixedChars, CharMaps> ExtractCaches;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static List<CharData> ExtractBuffer => ExtractBuffer_ ??= new List<CharData>(4);

            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static List<CharData> ExtractBuffer_;

            static CharMaps() => ExtractCaches = new Dictionary<MixedChars, CharMaps>(64);
        }

        /// <summary>
        /// Contains common shared variables that used on random data generation, this class is <see langword="static"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe partial class Commons
        {
            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static RandomNumberGenerator _DefaultEngine;
            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static IntVector _DefaultBucket;
            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static Byte[] _DefaultBuffer;
            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static Char[] _DefaultChars;
            [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private static StringBuilder _SBPassword1, _SBPassword2, _SBSerialNum1, _SBSerialNum2;

            /// <summary>
            /// The default length of cached buffer data, which is also the length of <see cref="DefaultBuffer"/> and <see cref="DefaultChars"/> arrays.
            /// </summary>
            public const Int32 CacheLength = 8192;

            /// <summary>
            /// A characters vector that containing 26 lower case alphabet characters from 'a' to 'z', used on random data generation.
            /// </summary>
            public static readonly CharData CharLowers;

            /// <summary>
            /// A characters vector that containing 26 upper case alphabet characters from 'A' to 'Z', used on random data generation.
            /// </summary>
            public static readonly CharData CharUppers;

            /// <summary>
            /// A characters vector that containing 10 numerical characters from '0' to '9', used on random data generation.
            /// </summary>
            public static readonly CharData CharNumber;

            /// <summary>
            /// A characters vector that containing 22 common symbol characters, it is used on random data generation.
            /// </summary>
            public static readonly CharData CharSymbol;

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default shared random number generator engine for the current thread.
            /// </summary>
            public static RandomNumberGenerator DefaultEngine
            {
                get
                {
                    var value = _DefaultEngine;
                    if (value is null)
                    {
#if NETFX
                        _DefaultEngine = value = new RNGCryptoServiceProvider();
#else
                        _DefaultEngine = value = RandomNumberGenerator.Create();
#endif
                    }
                    return value;
                }
            }

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default integer vector that used on generating random integer number in the current thread.
            /// </summary>
            public static IntVector DefaultBucket => _DefaultBucket ??= new IntVector();

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the shared array of bytes that used on streaming or buffer in the current thread.
            /// </summary>
            public static Byte[] DefaultBuffer => _DefaultBuffer ??= new Byte[CacheLength];

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the shared array of characters that used on streaming or buffer in the current thread.
            /// </summary>
            public static Char[] DefaultChars => _DefaultChars ??= new Char[CacheLength];

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default <see cref="StringBuilder"/> object that used on generating password (type 1).
            /// </summary>
            public static StringBuilder SBPassword1
            {
                get
                {
                    var value = _SBPassword1;
                    if (value is null)
                    {
                        _SBPassword1 = value = new StringBuilder(CacheLength);
                    }
                    else if (value.Length != 0)
                    {
                        value.Length = 0;
                    }
                    return value;
                }
            }

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default <see cref="StringBuilder"/> object that used on generating password (type 2).
            /// </summary>
            public static StringBuilder SBPassword2
            {
                get
                {
                    var value = _SBPassword2;
                    if (value is null)
                    {
                        _SBPassword2 = value = new StringBuilder(CacheLength);
                    }
                    else if (value.Length != 0)
                    {
                        value.Length = 0;
                    }
                    return value;
                }
            }

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default <see cref="StringBuilder"/> object that used on generating serial number (type 1).
            /// </summary>
            public static StringBuilder SBSerialNum1
            {
                get
                {
                    var value = _SBSerialNum1;
                    if (value is null)
                    {
                        _SBSerialNum1 = value = new StringBuilder(CacheLength);
                    }
                    else if (value.Length != 0)
                    {
                        value.Length = 0;
                    }
                    return value;
                }
            }

            /// <summary>
            /// (<b><see langword="thread-static"/></b>) Represents the default <see cref="StringBuilder"/> object that used on generating serial number (type 2).
            /// </summary>
            public static StringBuilder SBSerialNum2
            {
                get
                {
                    var value = _SBSerialNum2;
                    if (value is null)
                    {
                        _SBSerialNum2 = value = new StringBuilder(CacheLength);
                    }
                    else if (value.Length != 0)
                    {
                        value.Length = 0;
                    }
                    return value;
                }
            }

            /// <summary>
            /// Copy the entire contents in the given <paramref name="source"/> into a new array of <typeparamref name="T"/>, or return the <paramref name="source"/> if the <paramref name="source"/> is array of <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The <see cref="Type"/> of element that contained in the <paramref name="source"/> enumerable object sequences.</typeparam>
            /// <param name="source">Required, specify the <see cref="IEnumerable{T}"/> to copy the contents into array, or the array of <typeparamref name="T"/> to convert.</param>
            /// <returns>The <see cref="Array"/> of <typeparamref name="T"/> that containing the elements from <paramref name="source"/>, or the same <see cref="Array"/> of <typeparamref name="T"/> if the <paramref name="source"/> is array.</returns>
            public static T[] CastArray<T>(IEnumerable<T> source)
            {
                if (source is not null)
                {
                    T[] array = source as T[];
                    if (array is not null) return array;
                    Int32 count;
                    if (source is ICollection<T> icol1)
                    {
                        if ((count = icol1.Count) != 0)
                        {
                            array = new T[count];
                            icol1.CopyTo(array, 0);
                        }
                        else array = Array.Empty<T>();
                    }
                    else if (source is ICollection icol2)
                    {
                        if ((count = icol2.Count) != 0)
                        {
                            array = new T[count];
                            icol2.CopyTo(array, 0);
                        }
                        else array = Array.Empty<T>();
                    }
                    else if (source is IReadOnlyCollection<T> iroc)
                    {
                        if ((count = iroc.Count) != 0)
                        {
                            array = new T[count];
                            var index = 0;
                            if (iroc is IReadOnlyList<T> irol)
                            {
                                while (count-- > 0) array[index] = irol[index++];
                            }
                            else
                            {
                                using var iterator = iroc.GetEnumerator();
                                while (iterator.MoveNext())
                                    array[index++] = iterator.Current;
                            }
                        }
                        else array = Array.Empty<T>();
                    }
                    else
                    {
                        var packet = 8192;
                        count = 0;
                        array = new T[packet];
                        using (var iterator = source.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                if (count + 1 > packet)
                                {
                                    packet *= 2;
                                    var holder = new T[packet];
                                    Array.Copy(array, 0, holder, 0, count);
                                    array = holder;
                                }
                                array[count++] = iterator.Current;
                            }
                        }
                        if (count != packet)
                        {
                            var holder = new T[count];
                            Array.Copy(array, 0, holder, 0, count);
                            array = holder;
                        }

                    }
                    return array;
                }
                return Array.Empty<T>();
            }

            public const String ErrorNoCache = "The current set of random data is not created with persistence option, that mean the each of elements is always random not persisted. To support the indexer, make sure you have to initialize this class using " + nameof(RandomMode) + "." + nameof(RandomMode.KeepAfterDone) + " option.";

            public const String ErrorZeroSets = "Failed to retrieve the random data because the current set is empty.";

            public const String ErrorReadOnly = "This member is not supported and never implemented because the current set (collection) is read-only.";

            public const String ErrorFixedSize = "This member is not supported and never implemented because the current set (collection) is fixed-size.";

            public static void CheckIndex(Int32 space, ref Int32 index, String sourceName = "collection", String indexName = "index")
            {
                if (index < 0)
                {
                    var next = space + index;
                    if (next < 0 || next >= space)
                    {
                        throw new ArgumentOutOfRangeException(indexName, index, $"The specified {indexName} is outside the valid range of the {sourceName}.");
                    }
                    index = next;
                }
                else if (index >= space)
                {
                    throw new ArgumentOutOfRangeException(indexName, index, $"The specified {indexName} is cannot greater than or equals to the length (count) of {sourceName}.");
                }
            }

            public static void CheckRange(Int32 space, ref Int32 index, ref Int32 count, String sourceName = "collection", String indexName = "index", String countName = "count")
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(countName, count, $"The specified {countName} of data to process must not less than zero.");
                }
                if (index < 0)
                {
                    var next = space + index;
                    if (next < 0 || next >= space)
                    {
                        throw new ArgumentOutOfRangeException(indexName, index, $"The specified {indexName} is outside the valid range of the {sourceName}.");
                    }
                    index = next;
                }
                if (index + count > space)
                {
                    throw new ArgumentOutOfRangeException($"{indexName}, {countName}", $"{index}, {count}", $"The specified range ({indexName} and {countName}) is outside the valid range of {sourceName} that only have {space} elements or spaces.");
                }
            }

            public static void CheckNullRef<T>(T value, String name = "value") where T : class
            {
                if (value is null)
                {
                    throw new ArgumentNullException(name, $"The {name} must the valid instance of {typeof(T).Name}, but currently it is null.");
                }
            }

            static Commons()
            {
                CharLowers = new CharData("abcdefghijklmnopqrstuvwxyz");
                CharUppers = new CharData("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                CharNumber = new CharData("1234567890");
                CharSymbol = new CharData("~@#$%^&*()+=-<>{}:;?[]");
            }


        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe partial class Internals
        {
            /// <summary>
            /// Retrieve random 32-bit signed integer number in the given range using specified random engine and use existing data bucket.
            /// </summary>
            /// <param name="lbound">The minimum number of random data to retrieve, called as "lower bound", must smaller than or equal to <paramref name="ubound"/>.</param>
            /// <param name="ubound">The maximum number of random data to retrieve, called as "upper bound", must larger than or equal to <paramref name="lbound"/>.</param>
            /// <param name="engine">Required, the pre-initialized <see cref="RandomNumberGenerator"/> that should be used as cryptographic random data engine.</param>
            /// <param name="bucket">Required, set the existing or cached <see cref="IntVector"/> instance that should be used to temporarly store integer number.</param>
            /// <returns>The <see cref="Int32"/> as random number which generated between <paramref name="lbound"/> to <paramref name="ubound"/> range, the returned number is cannot predicted.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Int32 NextRange(Int32 lbound, Int32 ubound, RandomNumberGenerator engine, IntVector bucket)
            {
                if (ubound <= lbound)
                {
                    return lbound;
                }
                else
                {
                    const Int64 max = 1 + 4294967295L;
                    unchecked
                    {
                        var diff = (Int64)lbound - ubound;
                        UInt32 next;
                        while (true)
                        {
                            engine.GetBytes(bucket.Array, 0, 4);
                            if ((next = *bucket.UInt32) < max - (max % diff))
                            {
                                return (Int32)(lbound + (next % diff));
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Retrieve multiple random 32-bit signed integer numbers in the given range using specified random engine and use existing data bucket.
            /// </summary>
            /// <param name="count">Specify the <see cref="Int32"/> value as total count of random numbers to generate using this method, the minimum count to generate is one.</param>
            /// <param name="lbound">Specify the minimum number of random data to retrieve, called as "lower bound", must smaller than or equal to <paramref name="ubound"/> number.</param>
            /// <param name="ubound">Specify tThe maximum number of random data to retrieve, called as "upper bound", must larger than or equal to <paramref name="lbound"/> number.</param>
            /// <param name="engine">Required, specify the pre-initialized <see cref="RandomNumberGenerator"/> instance that should be used as cryptographic random data engine.</param>
            /// <param name="bucket">Required, the existing or cached <see cref="IntVector"/> instance that should be used to temporarly store integer number while production.</param>
            /// <returns>The array of <see cref="Int32"/> values as the generated random numbers between <paramref name="lbound"/> to <paramref name="ubound"/> range, the returned number is cannot predicted.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Int32[] NextRange(Int32 count, Int32 lbound, Int32 ubound, RandomNumberGenerator engine, IntVector bucket)
            {
                if (count < 1)
                {
                    return Array.Empty<Int32>();
                }
                else
                {
                    var array = new Int32[count];
                    var index = 0;
                    if (ubound < lbound)
                    {
                        while (count-- > 0)
                        {
                            array[index++] = lbound;
                        }
                    }
                    else
                    {
                        const Int64 max = 1 + 4294967295L;
                        unchecked
                        {
                            var diff = (Int64)lbound - ubound;
                            UInt32 next;
                            while (count > 0)
                            {
                                engine.GetBytes(bucket.Array, 0, 4);
                                if ((next = *bucket.UInt32) < max - (max % diff))
                                {
                                    array[index++] = (Int32)(lbound + (next % diff));
                                    count--;
                                }
                            }
                        }
                    }
                    return array;
                }
            }

            /// <summary>
            /// Retrieve the next random 32-bit unsigned integer number using the given cryptographic random engine.
            /// </summary>
            /// <param name="engine">The <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">The <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The <see cref="UInt32"/> data type that represents the next generated random 32-bit unsigned integer number.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
#if CLS
            [CLSCompliant(false)]
#endif
            public static UInt32 NextUInt32(RandomNumberGenerator engine, IntVector bucket)
            {
                engine.GetBytes(bucket.Array, 0, 4);
                return *bucket.UInt32;
            }

            /// <summary>
            /// Retrieve multiple random 32-bit unsigned integer numbers using the given cryptographic random engine.
            /// </summary>
            /// <param name="count">How many count of <see cref="UInt32"/> random numbers is should generated using this method? Minimum is one.</param>
            /// <param name="engine">The <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">The <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The array of <see cref="UInt32"/> data type that represents the next generated random 32-bit unsigned integer numbers.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
#if CLS
            [CLSCompliant(false)]
#endif
            public static UInt32[] NextUInt32(Int32 count, RandomNumberGenerator engine, IntVector bucket)
            {
                if (count < 1)
                {
                    return Array.Empty<UInt32>();
                }
                else
                {
                    var array = new UInt32[count];
                    var index = 0;
                    while (count-- > 0)
                    {
                        engine.GetBytes(bucket.Array, 0, 4);
                        array[index++] = *bucket.UInt32;
                    }
                    return array;
                }
            }

            /// <summary>
            /// Retrieve the next random 32-bit signed integer number using the given cryptographic random engine.<br/>
            /// <b>Note</b>: The returned random number is ranged from <b>0</b> to <see cref="Int32.MaxValue"/> (equal to 2,147,483,647).
            /// </summary>
            /// <param name="engine">The <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">The <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The <see cref="Int32"/> data type that represents the next generated random 32-bit signed integer number.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Int32 NextInt32(RandomNumberGenerator engine, IntVector bucket)
            {
                const Int64 max = 1 + 4294967295L;
                unchecked
                {
                    var diff = 0L - Int32.MaxValue;
                    UInt32 next;
                    while (true)
                    {
                        engine.GetBytes(bucket.Array, 0, 4);
                        if ((next = *bucket.UInt32) < max - (max % diff))
                        {
                            return (Int32)((next % diff));
                        }
                    }
                }
            }

            /// <summary>
            /// Retrieve the next random 32-bit signed integer number using the given cryptographic random engine.
            /// </summary>
            /// <param name="ubound">The upper bound number to generate, if set with positive number then the lower bound is zero.<br/>
            /// If set with negative number, then the lower bound number is always set to <see cref="Int32.MinValue"/> number.</param>
            /// <param name="engine">The <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">The <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The <see cref="Int32"/> data type that represents the next generated random 32-bit signed integer number.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Int32 NextInt32(Int32 ubound, RandomNumberGenerator engine, IntVector bucket)
            {
                const Int64 max = 1 + 4294967295L;
                unchecked
                {
                    var lbound = ubound < 0 ? Int32.MinValue : 0;
                    var diff = (Int64)lbound - ubound;
                    UInt32 next;
                    while (true)
                    {
                        engine.GetBytes(bucket.Array, 0, 4);
                        if ((next = *bucket.UInt32) < max - (max % diff))
                        {
                            return (Int32)(lbound + (next % diff));
                        }
                    }
                }
            }

            /// <summary>
            /// Retrieve the next random <see cref="Double"/> precision floating point number between 0.0 to 1.0 using existing random engine.
            /// </summary>
            /// <param name="engine">Required, the <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">Required, set the <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The <see cref="Double"/> number between 0.0 to 1.0, the exact number which returned is always random and cannot predicted.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Double NextDouble(RandomNumberGenerator engine, IntVector bucket)
            {
                engine.GetBytes(bucket.Array, 0, 4);
                return *bucket.UInt32 / (1.0 + UInt32.MaxValue);
            }

            /// <summary>
            /// Retrieve multiple random <see cref="Double"/> precision floating point numbers between 0.0 to 1.0 using existing random engine.
            /// </summary>
            /// <param name="count">How many count of random <see cref="Double"/> numbers is should generated using this method? The minimum is one.</param>
            /// <param name="engine">Required, the <see cref="RandomNumberGenerator"/> that used as cryptographic random generator for current production.</param>
            /// <param name="bucket">Required, set the <see cref="IntVector"/> that used to hold the generated random bytes data and cast as integer number.</param>
            /// <returns>The array of <see cref="Double"/> between 0.0 to 1.0, the numbers which returned is always random and cannot predicted.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static Double[] NextDouble(Int32 count, RandomNumberGenerator engine, IntVector bucket)
            {
                if (count < 1)
                {
                    return Array.Empty<Double>();
                }
                else
                {
                    var array = new Double[count];
                    var index = 0;
                    while (count-- > 0)
                    {
                        engine.GetBytes(bucket.Array, 0, 4);
                        array[index++] = *bucket.UInt32 / (1.0 + UInt32.MaxValue);
                    }
                    return array;
                }
            }

            /// <summary>
            /// Generate the next random keys (password) for specified <paramref name="length"/> of characters with specified combination of characters.
            /// </summary>
            /// <param name="length">The <see cref="Int32"/> number that represents the total length of characters in the random password (keys) to generate.</param>
            /// <param name="chars">The combination of <see cref="MixedChars"/> flags as component of characters that should present in the random password.</param>
            /// <param name="engine">The pre-initialized <see cref="RandomNumberGenerator"/> instance that should be used to generate random data in this method.</param>
            /// <param name="bucket">Needed, the pre-allocated <see cref="IntVector"/> that used to temporarly hold generated random data while production.</param>
            /// <returns>A <see cref="String"/> that have specified <paramref name="length"/> of random characters as the next generated random password (keys).</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static String NextKeys(Int32 length, MixedChars chars, RandomNumberGenerator engine, IntVector bucket)
            {
                if (length < 1)
                {
                    return "";
                }
                else if (length == 1)
                {
                    return CharMaps.Extract(chars, true).Random(engine, bucket).ToString();
                }
                else
                {
                    var mapping = CharMaps.Extract(chars, true);
                    var buffer = Commons.SBPassword1;
                    while (length > 2)
                    {
                        _ = buffer.Append(mapping.Random(engine, bucket));
                        _ = buffer.Append(mapping.Random(engine, bucket));
                        length -= 2;
                    }
                    if (length > 0)
                        _ = buffer.Append(mapping.Random(engine, bucket));
                    var result = buffer.ToString();
                    buffer.Length = 0;
                    return result;
                }
            }

            /// <summary>
            /// Generate the multiple random keys (password) for specified <paramref name="length"/> of characters with specified combination of characters.
            /// </summary>
            /// <param name="count">Specify the total count of the random keys (password) that should producted using specified random configurations.</param>
            /// <param name="length">The <see cref="Int32"/> number that represents the total length of characters in each random password (keys) to generate.</param>
            /// <param name="chars">The combination of <see cref="MixedChars"/> flags as component of characters that should present in each random password.</param>
            /// <param name="engine">The pre-initialized <see cref="RandomNumberGenerator"/> instance that should be used to generate random data in this method.</param>
            /// <param name="bucket">Needed, the pre-allocated <see cref="IntVector"/> that used to temporarly hold generated random data while production.</param>
            /// <returns>The array of <see cref="String"/> as multiple generated random password (keys), each of string have specified <paramref name="length"/> of characters.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static String[] NextKeys(Int32 count, Int32 length, MixedChars chars, RandomNumberGenerator engine, IntVector bucket)
            {
                if (count < 1)
                {
                    return Array.Empty<String>();
                }
                var array = new String[count];
                var index = 0;
                if (length < 1)
                {
                    while (count-- > 0)
                        array[index++] = "";
                }
                else
                {
                    var mapping = CharMaps.Extract(chars, true);
                    if (length == 1)
                    {
                        while (count-- > 0)
                            array[index++] = mapping.Random(engine, bucket).ToString();
                    }
                    else
                    {
                        var buffer = Commons.SBPassword1;
                        while (count-- > 0)
                        {
                            var needed = length;
                            while (needed > 2)
                            {
                                _ = buffer.Append(mapping.Random(engine, bucket));
                                _ = buffer.Append(mapping.Random(engine, bucket));
                                needed -= 2;
                            }
                            if (needed > 0)
                                _ = buffer.Append(mapping.Random(engine, bucket));
                            array[index++] = buffer.ToString();
                            buffer.Length = 0;
                        }
                    }
                }
                return array;
            }

            /// <summary>
            /// Generate the next random serial number that have specified length of characters and divided into specified group size using specified <paramref name="separator"/> and characters <paramref name="components"/>.
            /// </summary>
            /// <param name="serialLength">Specify the positive <see cref="Int32"/> number that represents the total length of characters (without separator) that should contained in the generated random serial number.<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="groupLength">Specify the <see cref="Int32"/> number that represents the total number of characters that should contained in each serial number groups, should be less than <paramref name="serialLength"/>.<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="separator">Specify any of <see cref="String"/> that should be used as separator to split each of groups in the generated random password, commonly it is a "-" (dash) or " " (singe space).<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="components">Specify one or more combinations of the <see cref="MixedChars"/> flags (except <see cref="MixedChars.Symbols"/>) as type of characters components in the random serial number.<br/>
            /// If passed with <see cref="MixedChars.Default"/> then the combinations of <see cref="MixedChars.Uppers"/> and <see cref="MixedChars.Number"/> will be used as the default character components.</param>
            /// <param name="engine">Required, specify the pre-initialized <see cref="RandomNumberGenerator"/> instance that should be used to generate cryptographic random data while in the random production.</param>
            /// <param name="bucket">Required, specify the pre-allocated <see cref="IntVector"/> class instance that should be used to temporarly hold the generated random data while in the random production.</param>
            /// <returns>A <see cref="String"/> as next generated random serial number which have specified length of characters, divided into specified group size, and use the given <paramref name="separator"/> and <paramref name="components"/>.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static String NextSerial(Int32 serialLength, Int32 groupLength, String separator, MixedChars components, RandomNumberGenerator engine, IntVector bucket)
            {
                if (serialLength < 1)
                {
                    return "";
                }
                var split = separator is not null && separator.Length != 0;
                if (groupLength < 1)
                {
                    if (split)
                    {
                        var builder = Commons.SBSerialNum1;
                        if (separator.Length == 1)
                        {
                            builder.Append(separator[0], serialLength);
                        }
                        else
                        {
                            while (serialLength-- > 0)
                            {
                                _ = builder.Append(separator);
                            }
                        }
                        var result = builder.ToString();
                        builder.Length = 0;
                        return result;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    components &= ~MixedChars.Symbols;
                    var mapping = CharMaps.Extract(components, false);
                    var builder = Commons.SBSerialNum1;
                    while (serialLength > 0)
                    {
                        var needed = groupLength > serialLength ? serialLength : groupLength;
                        serialLength -= needed;
                        while (needed-- > 0)
                            _ = builder.Append(mapping.Random(engine, bucket));
                        if (split && serialLength > 0)
                            _ = builder.Append(separator);
                    }
                    var result = builder.ToString();
                    builder.Length = 0;
                    return result;
                }
            }

            /// <summary>
            /// Generate multiple random serial numbers that have specified length of characters and divided into specified group size using specified <paramref name="separator"/> and characters <paramref name="components"/>.
            /// </summary>
            /// <param name="totalSerials">Specify the total count of random serial numbers to generate at once, require positive number, at least one. If zero or less then return the empty <see cref="String"/> array.</param>
            /// <param name="serialLength">Specify the positive <see cref="Int32"/> number that represents the total length of characters (without separator) that should contained in the generated random serial number.<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="groupLength">Specify the <see cref="Int32"/> number that represents the total number of characters that should contained in each serial number groups, should be less than <paramref name="serialLength"/>.<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="separator">Specify any of <see cref="String"/> that should be used as separator to split each of groups in the generated random password, commonly it is a "-" (dash) or " " (singe space).<br/>
            /// eg. NextSerial(20, 5, "-") will generate: "xxxxx-xxxxx-xxxxx-xxxxx" (each of group have 5 characters for total is 20 characters length with '-' as separator).</param>
            /// <param name="components">Specify one or more combinations of the <see cref="MixedChars"/> flags (except <see cref="MixedChars.Symbols"/>) as type of characters components in the random serial number.<br/>
            /// If passed with <see cref="MixedChars.Default"/> then the combinations of <see cref="MixedChars.Uppers"/> and <see cref="MixedChars.Number"/> will be used as the default character components.</param>
            /// <param name="engine">Required, specify the pre-initialized <see cref="RandomNumberGenerator"/> instance that should be used to generate cryptographic random data while in the random production.</param>
            /// <param name="bucket">Required, specify the pre-allocated <see cref="IntVector"/> class instance that should be used to temporarly hold the generated random data while in the random production.</param>
            /// <returns>The array of <see cref="String"/> as multiple generated serial numbers which have specified length of characters, divided into given group size with desired <paramref name="separator"/> and <paramref name="components"/>.</returns>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static String[] NextSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength, String separator, MixedChars components, RandomNumberGenerator engine, IntVector bucket)
            {
                if (totalSerials < 1)
                {
                    return Array.Empty<String>();
                }
                else
                {
                    var array = new String[totalSerials];
                    var index = 0;
                    if (serialLength < 1)
                    {
                        while (totalSerials-- > 0)
                            array[index++] = "";
                    }
                    else
                    {
                        var split = separator is not null && separator.Length != 0;
                        if (groupLength < 1)
                        {
                            if (split)
                            {
                                if (separator.Length == 1)
                                {
                                    while (totalSerials-- > 0)
                                    {
                                        array[index++] = separator;
                                    }
                                }
                                else
                                {
                                    var builder = Commons.SBSerialNum2;
                                    while (totalSerials-- > 0)
                                    {
                                        var needed = serialLength;
                                        while (needed-- > 0)
                                            _ = builder.Append(separator);
                                        array[index++] = builder.ToString();
                                        builder.Length = 0;
                                    }
                                }
                            }
                            else
                            {
                                while (totalSerials-- > 0)
                                    array[index++] = "";
                            }
                        }
                        else
                        {
                            components &= ~MixedChars.Symbols;
                            var mapping = CharMaps.Extract(components, false);
                            var builder = Commons.SBSerialNum2;
                            while (totalSerials-- > 0)
                            {
                                var length = serialLength;
                                while (length > 0)
                                {
                                    var needed = groupLength > length ? length : groupLength;
                                    length -= needed;
                                    while (needed-- > 0)
                                        _ = builder.Append(mapping.Random(engine, bucket));
                                    if (length > 0 && split)
                                        _ = builder.Append(separator);
                                }
                                array[index++] = builder.ToString();
                                builder.Length = 0;
                            }
                        }
                    }
                    return array;
                }
            }

            /// <summary>
            /// Shuffle the position of elements that contained in the given <paramref name="array"/> with the next random generated indices.
            /// </summary>
            /// <typeparam name="T">Specify the <see cref="Type"/> of element that contained in the given <paramref name="array"/> to shuffle (this method is support any type).</typeparam>
            /// <param name="array">Specify the target array of <typeparamref name="T"/> that containing at least two elements to shuffle the elements position.</param>
            /// <param name="engine">The pre-initialized <see cref="RandomNumberGenerator"/> that internally used to generate the random numbers.</param>
            /// <param name="bucket">The pre-allocated <see cref="IntVector"/> instance that should internally used to generate the random numbers.</param>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static void NextShuffle<T>(T[] array, RandomNumberGenerator engine, IntVector bucket)
            {
                var length = array is null ? 0 : array.Length;
                if (length > 1)
                {
                    Int32 target, index = 0;
                    T value;
                    while (length > 0)
                    {
                        value = array[target = NextRange(0, length, engine, bucket)];
                        array[target] = array[index];
                        array[index++] = value;
                        length--;
                    }
                }
            }

            /// <summary>
            /// Shuffle the position of elements that contained in the given <paramref name="list"/> with the next random generated indices.
            /// </summary>
            /// <typeparam name="T">Specify the <see cref="Type"/> of element that contained in the given <paramref name="list"/> to shuffle (this method is support any type).</typeparam>
            /// <param name="list">Specify the <see cref="IList{T}"/> instance that containing at least two elements to shuffle the elements position.</param>
            /// <param name="engine">The pre-initialized <see cref="RandomNumberGenerator"/> that internally used to generate the random numbers.</param>
            /// <param name="bucket">The pre-allocated <see cref="IntVector"/> instance that should internally used to generate the random numbers.</param>
            /// <exception cref="NullReferenceException">The <paramref name="engine"/> or <paramref name="bucket"/> parameters is passed by <see langword="null"/>.</exception>
            public static void NextShuffle<T>(IList<T> list, RandomNumberGenerator engine, IntVector bucket)
            {
                var length = list is null || list.IsReadOnly ? 0 : list.Count;
                if (length > 1)
                {
                    Int32 target, index = 0;
                    T value;
                    while (length > 0)
                    {
                        value = list[target = NextRange(0, length, engine, bucket)];
                        list[target] = list[index];
                        list[index++] = value;
                        length--;
                    }
                }
            }
        }

        /// <summary>
        /// Represents a sequence of the random number (type <see cref="Int32"/>) in specified range, this class is not inheritable.
        /// </summary>
        public sealed partial class PushEnum : IRandomSet<Int32>, IReadOnlyList<Int32>, IList<Int32>, ICollection, IList
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Int32 _lbound, _ubound, _length;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomNumberGenerator _engine;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly IntVector _bucket;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomMode _option;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomFilter<Int32> _filter;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Int32[] _caches;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private volatile Int32 _cached;

            public PushEnum(Int32 lbound, Int32 ubound, Int32 length, RandomMode option)
                : this(lbound, ubound, length, option, null, null) {; }

            public PushEnum(Int32 lbound, Int32 ubound, Int32 length, RandomMode option, RandomFilter<Int32> filter)
                : this(lbound, ubound, length, option, filter, null) {; }

            internal PushEnum(Int32 lbound, Int32 ubound, Int32 length, RandomMode option, RandomFilter<Int32> filter, RandomNumberGenerator engine)
            {
                this._lbound = lbound;
                this._ubound = ubound < lbound ? lbound : ubound;
                this._length = length < 1 ? 0 : length;
                this._option = option;
                this._engine = engine ?? Commons.DefaultEngine;
                this._bucket = new IntVector();
                this._caches = option == RandomMode.KeepAfterDone ? this._length == 0 ? Array.Empty<Int32>() : new Int32[this._length] : null;
                this._cached = 0;
                this._filter = filter;
            }

            /// <summary>
            /// Represents the "lower bound" of the random number to generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 LBound => this._lbound;

            /// <summary>
            /// Represents the "upper bound" of the random number to generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 UBound => this._ubound;

            /// <summary>
            /// Represents the total count (length) of the random numbers to generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 Count => this._length;

            /// <summary>
            /// Represents the persistence option of the current <see cref="PushEnum"/> instance.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public RandomMode Option => this._option;

            /// <summary>
            /// Retrieve how many random numbers that has been cached in this <see cref="PushEnum"/>, only used if the <see cref="Option"/> is <see cref="RandomMode.KeepAfterDone"/>.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 Cached => this._cached;

            /// <summary>
            /// Represents a custom function that used to filter the random data while production, or <see langword="null"/> if not defined.
            /// </summary>
            public RandomFilter<Int32> Filter => this._filter;

            /// <summary>
            /// Retireve the reference (read only) of random number that cached in this <see cref="PushEnum"/> at specified <paramref name="index"/>.<br/>
            /// This indexer is only supported if the current <see cref="Option"/> is set with <see cref="RandomMode.KeepAfterDone"/>.
            /// </summary>
            /// <param name="index">The index of cached random number to retrieve, the negative value mean the tailing index.</param>
            /// <returns>The read only reference of the cached random number at specified <paramref name="index"/>, represented as <see cref="Int32"/> data type.</returns>
            /// <exception cref="NotSupportedException">Always thrown if the current <see cref="Option"/> is not <see cref="RandomMode.KeepAfterDone"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is outside the valid range of the current <see cref="PushEnum"/>.</exception>
            /// <filterpriority>2</filterpriority>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public ref readonly Int32 this[Int32 index]
            {
                get
                {
                    if (this._option == RandomMode.KeepAfterDone)
                    {
                        if (this._length < 1)
                        {
                            throw new InvalidOperationException(Commons.ErrorZeroSets);
                        }
                        if (index < 0)
                        {
                            var next = this._length + index;
                            if (next < 0 || next >= this._length)
                            {
                                throw new ArgumentOutOfRangeException(nameof(index), index, "The index of random data is outside the valid range of the current set.");
                            }
                            index = next;
                        }
                        else if (index >= this._length)
                        {
                            throw new ArgumentOutOfRangeException(nameof(index), index, "The index of random data is outside the valid range of the current set.");
                        }
                        if (this._cached < index)
                        {
                            const Int64 max = 1 + 4294967295L;
                            var array = this._caches;
                            Int32 offset = this._cached, length = this._length - offset;
                            unchecked
                            {
                                var engine = this._engine;
                                var bucket = this._bucket;
                                var lbound = this._lbound;
                                var diff = (Int64)lbound - this._ubound;
                                UInt32 next;
                                unsafe
                                {
                                    if (this._filter is RandomFilter<Int32> filter)
                                    {
                                        while (length > 0)
                                        {
                                            engine.GetBytes(bucket.Array, 0, 4);
                                            if ((next = *bucket.UInt32) < max - (max % diff))
                                            {
                                                var current = (Int32)(lbound + (next % diff));
                                                if (filter.Invoke(offset, current))
                                                {
                                                    array[offset++] = current;
                                                    length--;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (length > 0)
                                        {
                                            engine.GetBytes(bucket.Array, 0, 4);
                                            if ((next = *bucket.UInt32) < max - (max % diff))
                                            {
                                                array[offset++] = (Int32)(lbound + (next % diff));
                                                length--;
                                            }
                                        }
                                    }
                                }
                            }
                            this._cached = offset;
                        }
                        return ref this._caches[index];
                    }
                    throw new NotSupportedException(Commons.ErrorNoCache);
                }
            }

            /// <summary>
            /// Create a new <see cref="Enumerator"/> object that supported to enumerate the random data using the current configured settings.
            /// </summary>
            /// <returns>The <see cref="Enumerator"/> class that supported to iterate over all generated random numbers in the current <see cref="PushEnum"/> instance.</returns>
            public Enumerator Iterator() => new(this, this._filter);

            /// <summary>
            /// Writes the random data in the current set into the destination <paramref name="array"/>, starting at specified <paramref name="arrayIndex"/>.
            /// </summary>
            /// <param name="array">Specify the destination array of <see cref="Int32"/> that should writen by the random data from the current set.</param>
            /// <param name="arrayIndex">Set the starting index of the element in the <paramref name="array"/> to begin writen by copied random data.</param>
            /// <exception cref="ArgumentNullException">Thrown if the destination <paramref name="array"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="arrayIndex"/> is outside the valid range of <paramref name="array"/>, -or the <paramref name="array"/> is not have sufficient spaces to store the random data for the current <see cref="Count"/> number, starting at specified <paramref name="arrayIndex"/>.</exception>
            public void CopyTo(Int32[] array, Int32 arrayIndex)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (arrayIndex < 0)
                {
                    var next = array.Length + arrayIndex;
                    if (next < 0 || next >= array.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "The index of element in array to begin writen is outside the valid range of array.");
                    }
                }
                if (arrayIndex + this._length > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, $"The destination array is not have enough spaces (currently have {array.Length}) to fill with the copied elements from this set ({this._length}) when starting at index {arrayIndex}.");
                }
                var iterator = new Enumerator(this);
                while (iterator.MoveNext()) array[arrayIndex++] = iterator.Current;
            }

            /// <summary>
            /// Search the index of specified element in the current set, valid only if <see cref="Option"/> is <see cref="RandomMode.KeepAfterDone"/>.
            /// </summary>
            /// <param name="value">Specify the <see cref="Int32"/> value that is the random number to search over the current cached random data.</param>
            /// <returns>The <see cref="Int32"/> value that represents zero based index of the searched <paramref name="value"/>, or -1 if not found.</returns>
            public Int32 IndexOf(Int32 value)
            {
                if (this._option == RandomMode.KeepAfterDone)
                {
                    if (this._length == 0)
                        return -1;
                    if (this._cached != this._length)
                    {
                        if (value == this[this._length - 1])
                            return this._length - 1;
                    }
                    return Array.IndexOf(this._caches, value);
                }
                return -1;
            }

            /// <summary>
            /// Provides iteration over all random generated data in the <see cref="PushEnum"/> instance, this class is <see langword="sealed"/>.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public sealed partial class Enumerator : IEnumerator<Int32>
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly PushEnum _source;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Int32 _offset;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Int32? _current;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly Boolean _caching;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly RandomFilter<Int32> _filter;

                /// <summary>
                /// Initializes a new instance of the <see cref="Enumerator"/> class.
                /// </summary>
                /// <param name="source">The source of <see cref="PushEnum"/> to enumerate the data.</param>
                /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is passed with <see langword="null"/>.</exception>
                public Enumerator(PushEnum source)
                {
                    this._source = source ?? throw new ArgumentNullException(nameof(source));
                    this._offset = -1;
                    this._caching = source._option == RandomMode.KeepAfterDone;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="Enumerator"/> class.
                /// </summary>
                /// <param name="source">The source of <see cref="PushEnum"/> to enumerate the data.</param>
                /// <param name="filter">The custom function to filter out the generated random data.</param>
                /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is passed with <see langword="null"/>.</exception>
                public Enumerator(PushEnum source, RandomFilter<Int32> filter)
                {
                    this._source = source ?? throw new ArgumentNullException(nameof(source));
                    this._offset = -1;
                    this._caching = source._option == RandomMode.KeepAfterDone;
                    this._filter = filter;
                }

                /// <summary>
                /// Represents the random <see cref="Int32"/> number at the current iterator position.
                /// </summary>
                /// <value>The <see cref="Int32"/> value as random number at the current enumerator position.</value>
                public Int32 Current
                {
                    get
                    {
                        var value = this._current;
                        if (!value.HasValue)
                        {
                            if (this._offset < 0 || this._source._length == 0)
                            {
                                return 0;
                            }
                            if (this._caching)
                            {
                                if (this._offset < this._source._cached)
                                {
                                    var number = this._source._caches[this._offset];
                                    this._current = number;
                                    return number;
                                }
                            }
                            var integer = Internals.NextRange(this._source._lbound, this._source.UBound, this._source._engine, this._source._bucket);
                            if (this._filter is RandomFilter<Int32> filter)
                            {
                                while (!filter.Invoke(this._offset, integer))
                                    integer = Internals.NextRange(this._source._lbound, this._source.UBound, this._source._engine, this._source._bucket);
                            }
                            this._current = integer;
                            if (this._caching)
                            {
                                this._source._caches[this._source._cached++] = integer;
                            }
                            return integer;
                        }
                        return value.Value;
                    }
                }

                /// <summary>
                /// Represents the current actual position of the random number in sequence.
                /// </summary>
                /// <value>The <see cref="Int32"/> value as index of the current random number in sequence.</value>
                public Int32 Offset => this._offset;

                /// <summary>
                /// Represents the total length of random numbers that should be enumerated using this enumerator.
                /// </summary>
                /// <value>The <see cref="Int32"/> value as total count of random number data that should enumerated by this enumerator.</value>
                public Int32 Count => this._source._length;

                /// <summary>
                /// Represents the source of <see cref="PushEnum"/> that owning the current <see cref="Enumerator"/>.
                /// </summary>
                /// <value>The parent of <see cref="PushEnum"/> which create this <see cref="Enumerator"/> object instance.</value>
                public PushEnum Source => this._source;

                /// <summary>
                /// Advance the current position of <see cref="Enumerator"/> to retrieve the next random data in sequence, if any.
                /// </summary>
                /// <returns><see langword="true"/> if the next random data in the sequence is available; otherwise, <see langword="false"/> when reaching peak position.</returns>
                public Boolean MoveNext()
                {
                    if (++this._offset >= this._source._length)
                    {
                        this._offset = this._source._length;
                        return false;
                    }
                    this._current = null;
                    return true;
                }

                /// <summary>
                /// Decrement the current position of <see cref="Enumerator"/> to retrieve the previous random data in sequence, if any.
                /// </summary>
                /// <returns><see langword="true"/> if the previous random data in the sequence is available; otherwise, <see langword="false"/> when reaching bottom position.</returns>
                public Boolean MovePrev()
                {
                    if (--this._offset < 0)
                    {
                        this._offset = -1;
                        return false;
                    }
                    this._current = null;
                    return true;
                }

                /// <summary>
                /// Reset the position of the current <see cref="Enumerator"/> into it's initial state.
                /// </summary>
                public void Reset()
                {
                    this._offset = -1;
                    this._current = null;
                }

                /// <summary>
                /// Clears all values that used in the current <see cref="Enumerator"/>.
                /// </summary>
                public void Dispose()
                {
                    this._offset = this._source._length;
                    this._current = null;
                }

                /// <inheritdoc/>
                public override String ToString() => this.Current.ToString();

                /// <inheritdoc/>
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                Object IEnumerator.Current => this.Current;
            }

            #region ... explicit members

            /// <inheritdoc/>
            /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is outside the valid range of this set.</exception>
            /// <exception cref="NotSupportedException">When setting value through this indexer due to read only collection behavior.</exception>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Int32 IList<Int32>.this[Int32 index] { get => this[index]; set => throw new NotSupportedException(Commons.ErrorReadOnly); }

            /// <inheritdoc/>
            /// <remarks>Do not use if the <see cref="Option"/> is not <see cref="RandomMode.KeepAfterDone"/> because it always return <see langword="false"/>.</remarks>
            Boolean ICollection<Int32>.Contains(Int32 item)
            {
                if (this._option == RandomMode.KeepAfterDone)
                {
                    if (this._length == 0) return false;
                    if (this._cached < this._length)
                    {
                        _ = this[this._length - 1];
                    }
                    return Array.IndexOf(this._caches, item) != -1;
                }
                return false;
            }

            /// <inheritdoc/>
            /// <remarks>Do not use if the <see cref="Option"/> is not <see cref="RandomMode.KeepAfterDone"/> because it always return -1.</remarks>
            Int32 IList<Int32>.IndexOf(Int32 item)
            {
                if (this._option == RandomMode.KeepAfterDone)
                {
                    if (this._length == 0) return -1;
                    if (this._cached < this._length)
                    {
                        _ = this[this._length - 1];
                    }
                    return Array.IndexOf(this._caches, item);
                }
                return -1;
            }

            /// <inheritdoc/>
            /// <exception cref="ArgumentNullException">The <paramref name="array"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not valid, or the <paramref name="array"/> is not have sufficient spaces to store the copied data.</exception>
            void ICollection.CopyTo(Array array, Int32 index)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (array.Rank != 1)
                {
                    throw new RankException($"The destination array is {array.Rank} dimensions array but this method is only supported to write into the one dimensional array.");
                }
                if (index < 0)
                {
                    var next = array.Length + index;
                    if (next < 0 || next >= array.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index, "The index of element in array to begin writen is outside the valid range of array.");
                    }
                }
                if (index + this._length > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, $"The destination array is not have enough spaces (currently have {array.Length}) to fill with the copied elements from this set ({this._length}) when starting at index {index}.");
                }
                var iterator = new Enumerator(this);
                while (iterator.MoveNext())
                {
                    array.SetValue(iterator.Current, index++);
                }
            }

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Int32 ICollection<Int32>.Count => this._length;

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Boolean ICollection<Int32>.IsReadOnly => true;

            /// <inheritdoc/>
            IEnumerator<Int32> IEnumerable<Int32>.GetEnumerator() => this.Iterator();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => this.Iterator();

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Int32 ICollection.Count => this._length;

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Object ICollection.SyncRoot => this;

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Boolean ICollection.IsSynchronized => false;

            /// <inheritdoc/>
            /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is outside the valid range of this set.</exception>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Int32 IReadOnlyList<Int32>.this[Int32 index] => this[index];

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Int32 IReadOnlyCollection<Int32>.Count => this._length;

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void ICollection<Int32>.Add(Int32 item) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void ICollection<Int32>.Clear() => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            Boolean ICollection<Int32>.Remove(Int32 item) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList<Int32>.Insert(Int32 index, Int32 item) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList<Int32>.RemoveAt(Int32 index) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            Int32 IList.Add(Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            Boolean IList.Contains(Object value) => value is not null && this._option == RandomMode.KeepAfterDone && ((ICollection<Int32>)this).Contains(Convert.ToInt32(value, CultureInfo.CurrentCulture));

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList.Clear() => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            Int32 IList.IndexOf(Object value) => value is null || this._option != RandomMode.KeepAfterDone ? -1 : ((IList<Int32>)this).IndexOf(Convert.ToInt32(value, CultureInfo.CurrentCulture));

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList.Insert(Int32 index, Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList.Remove(Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="NotSupportedException">Always thrown because this set is read-only.</exception>
            void IList.RemoveAt(Int32 index) => throw new NotSupportedException(Commons.ErrorReadOnly);

            /// <inheritdoc/>
            /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is outside the valid range of this set.</exception>
            /// <exception cref="NotSupportedException">Always thrown on setter methdod because this set is read-only.</exception>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Object IList.this[Int32 index] { get => this[index]; set => throw new NotSupportedException(Commons.ErrorReadOnly); }

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Boolean IList.IsReadOnly => true;

            /// <inheritdoc/>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Boolean IList.IsFixedSize => true;

            #endregion
        }

        /// <summary>
        /// Represents a sequence of the multple random number arrays (type <see cref="Int32"/>[]) in specified range, this class is not inheritable.
        /// </summary>
        public sealed partial class PushBlocks : IRandomSet<Int32[]>, IReadOnlyList<Int32[]>, ICollection<Int32[]>, IList<Int32[]>, ICollection, IList
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Int32 _lbound, _ubound, _length, _count;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomNumberGenerator _engine;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly IntVector _bucket;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomMode _option;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RandomFilter<Int32[]> _filter;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly Int32[][] _caches;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private volatile Int32 _cached;

            public PushBlocks(Int32 count, Int32 lbound, Int32 ubound, Int32 length, RandomMode option)
                : this(count, lbound, ubound, length, option, null, null) {; }

            public PushBlocks(Int32 count, Int32 lbound, Int32 ubound, Int32 length, RandomMode option, RandomFilter<Int32[]> filter)
                : this(count, lbound, ubound, length, option, filter, null) {; }

            internal PushBlocks(Int32 count, Int32 lbound, Int32 ubound, Int32 length, RandomMode option, RandomFilter<Int32[]> filter, RandomNumberGenerator engine)
            {
                this._count = count < 1 ? 0 : count;
                this._lbound = lbound;
                this._ubound = ubound < lbound ? lbound : ubound;
                this._length = length < 1 ? 0 : length;
                this._option = option;
                this._filter = filter;
                this._engine = engine ?? Commons.DefaultEngine;
                this._bucket = new IntVector();
                if (option == RandomMode.KeepAfterDone) this._caches = new Int32[this._count][];
            }

            /// <summary>
            /// Represents the "lower bound" of the random number in each blocks to generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 LBound => this._lbound;

            /// <summary>
            /// Represents the "upper bound" of the random number in each blocksto generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 UBound => this._ubound;

            /// <summary>
            /// Represents the total count of the random number blocks to generate.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 Count => this._count;

            /// <summary>
            /// Represents the length of random numbers that should contained in the each blocks.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 Length => this._length;

            /// <summary>
            /// Represents the persistence option of the current <see cref="PushBlocks"/> instance.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public RandomMode Option => this._option;

            /// <summary>
            /// Retrieve how many random number blocks that has been cached in this <see cref="PushBlocks"/>, only used if the <see cref="Option"/> is <see cref="RandomMode.KeepAfterDone"/>.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public Int32 Cached => this._cached;

            /// <inheritdoc/>
            public RandomFilter<Int32[]> Filter => this._filter;

            /// <summary>
            /// Create a new <see cref="Enumerator"/> object that supported to enumerate the random data using the current configured settings.
            /// </summary>
            /// <returns>The <see cref="Enumerator"/> class that supported to iterate over all generated random numbers in the current <see cref="PushBlocks"/> instance.</returns>
            public Enumerator Iterator() => new(this);

            /// <summary>
            /// Writes the random data in the current set into the destination <paramref name="array"/>, starting at specified <paramref name="arrayIndex"/>.
            /// </summary>
            /// <param name="array">Specify the destination array of <see cref="Int32"/> arrays that should writen by the random data from the current set.</param>
            /// <param name="arrayIndex">Set the starting index of the element in the <paramref name="array"/> to begin writen by copied random data.</param>
            /// <exception cref="ArgumentNullException">Thrown if the destination <paramref name="array"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="arrayIndex"/> is outside the valid range of <paramref name="array"/>, -or the <paramref name="array"/> is not have sufficient spaces to store the random data for the current <see cref="Count"/> number, starting at specified <paramref name="arrayIndex"/>.</exception>
            public void CopyTo(Int32[][] array, Int32 arrayIndex)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (arrayIndex < 0)
                {
                    var next = array.Length + arrayIndex;
                    if (next < 0 || next >= array.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "The index of element in array to begin writen is outside the valid range of array.");
                    }
                }
                if (arrayIndex + this._length > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, $"The destination array is not have enough spaces (currently have {array.Length}) to fill with the copied elements from this set ({this._length}) when starting at index {arrayIndex}.");
                }
                var iterator = new Enumerator(this);
                while (iterator.MoveNext()) array[arrayIndex++] = iterator.Current;
            }

            /// <summary>
            /// Search the index of specified block data in the current set. This method is supported only if the <see cref="Option"/> is <see cref="RandomMode.KeepAfterDone"/>.
            /// </summary>
            /// <param name="array">Specify the desired block data to search over the current set, the length of searched block must equal to the length of generated block.</param>
            /// <returns>If found, return the <see cref="Int32"/> value as zero based index of the searched block; otheriwse, return -1 if not found.</returns>
            public Int32 IndexOf(Int32[] array)
            {
                if (this._option != RandomMode.KeepAfterDone || array is null || array.Length != this._length)
                {
                    return -1;
                }
                else if (this._cached == this._count)
                {
                    var index = 0;
                    var length = this._cached;
                    while (length-- > 0)
                    {
                        if (IsEquals(this._caches[index], array, this._length))
                            return index;
                        index++;
                    }
                    return -1;
                }
                else
                {
                    var index = 0;
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                    {
                        if (IsEquals(iterator.Current, array, this._length))
                        {
                            return index;
                        }
                        index++;
                    }
                    return -1;
                }
            }

            /// <summary>
            /// Retireve the reference (read only) of random number blocks that cached in this <see cref="PushEnum"/> at specified <paramref name="index"/>.<br/>
            /// This indexer is only supported if the current <see cref="Option"/> is set with <see cref="RandomMode.KeepAfterDone"/> mode.
            /// </summary>
            /// <param name="index">The index of cached random number blocks to retrieve, the negative value mean the tailing index.</param>
            /// <returns>The read only reference of the cached random number blocks at specified <paramref name="index"/>, represented as <see cref="Int32"/> array type.</returns>
            /// <exception cref="NotSupportedException">Always thrown if the current <see cref="Option"/> is not <see cref="RandomMode.KeepAfterDone"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is outside the valid range of the current <see cref="PushEnum"/>.</exception>
            /// <filterpriority>2</filterpriority>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public ref readonly Int32[] this[Int32 index]
            {
                get
                {
                    if (this._option == RandomMode.KeepAfterDone)
                    {
                        if (this._length < 1)
                        {
                            throw new InvalidOperationException(Commons.ErrorZeroSets);
                        }
                        if (index < 0)
                        {
                            var next = this._length + index;
                            if (next < 0 || next >= this._length)
                            {
                                throw new ArgumentOutOfRangeException(nameof(index), index, "The index of random data is outside the valid range of the current set.");
                            }
                            index = next;
                        }
                        else if (index >= this._length)
                        {
                            throw new ArgumentOutOfRangeException(nameof(index), index, "The index of random data is outside the valid range of the current set.");
                        }
                        if (this._cached < index)
                        {
                            var array = this._caches;
                            Int32 offset = this._cached, length = this._count - offset;
                            if (this._filter is RandomFilter<Int32[]> filter)
                            {
                                while (length-- > 0)
                                {
                                    var current = Internals.NextRange(this._length, this._lbound, this._ubound, this._engine, this._bucket);
                                    while (!filter.Invoke(offset, current))
                                        current = Internals.NextRange(this._length, this._lbound, this._ubound, this._engine, this._bucket);
                                    array[offset++] = current;
                                }
                            }
                            else
                            {
                                while (length-- > 0)
                                    array[offset++] = Internals.NextRange(this._length, this._lbound, this._ubound, this._engine, this._bucket);
                            }
                            this._cached = offset;
                        }
                        return ref this._caches[index];
                    }
                    throw new NotSupportedException(Commons.ErrorNoCache);
                }
            }

            /// <summary>
            /// Provides iteration over all random generated data in the <see cref="PushEnum"/> instance, this class is <see langword="sealed"/>.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public sealed partial class Enumerator : IEnumerator<Int32[]>
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly PushBlocks _source;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly Boolean _caching;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Int32 _offset;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Int32[] _current;

                public Enumerator(PushBlocks source)
                {
                    this._source = source ?? throw new ArgumentNullException(nameof(source));
                    this._offset = -1;
                    this._caching = source._option == RandomMode.KeepAfterDone;
                }

                public Int32[] Current
                {
                    get
                    {
                        var value = this._current;
                        if (value is null)
                        {
                            if (this._offset == -1 || this._offset >= this._source._count)
                            {
                                return Array.Empty<Int32>();
                            }
                            if (this._caching && this._offset < this._source._cached)
                            {
                                this._current = value = this._source._caches[this._offset];
                                return value;
                            }
                            value = Internals.NextRange(this._source._length, this._source._lbound, this._source._ubound, this._source._engine, this._source._bucket);
                            if (this._source._filter is RandomFilter<Int32[]> filter)
                            {
                                while (!filter.Invoke(this._offset, value))
                                {
                                    value = Internals.NextRange(this._source._length, this._source._lbound, this._source._ubound, this._source._engine, this._source._bucket);
                                }
                            }
                            if (this._caching)
                            {
                                this._source._caches[this._source._cached++] = value;
                            }
                            this._current = value;
                        }
                        return value;
                    }
                }

                public Int32 Offset => this._offset;

                public Int32 Count => this._source._count;

                public PushBlocks Source => this._source;

                public Boolean MoveNext()
                {
                    if (++this._offset >= this._source._count)
                    {
                        this._offset = this._source._count;
                        return false;
                    }
                    this._current = null;
                    return true;
                }

                public Boolean MovePrev()
                {
                    if (--this._offset < 0)
                    {
                        this._offset = -1;
                        return false;
                    }
                    this._current = null;
                    return true;
                }

                public void Reset()
                {
                    this._offset = -1;
                    this._current = null;
                }

                public void Dispose()
                {
                    this._offset = this._source._length;
                    this._current = null;
                }

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                Object IEnumerator.Current => this.Current;

                public override String ToString() => this.Current is Int32[] array && array.Length != 0 ? $"[{String.Join(", ", array)}]" : "[]";

            }

            private static unsafe Boolean IsEquals(Int32[] a, Int32[] b, Int32 c)
            {
                fixed (Int32* aptr = a, bptr = b)
                {
                    Int32* left = aptr, right = bptr;
                    var equal = true;
                    while (c-- > 0 && (equal = *left++ == *right++)) ;
                    return equal;
                }
            }

            Int32[] IReadOnlyList<Int32[]>.this[Int32 index] => this[index];
            Int32[] IList<Int32[]>.this[Int32 index] { get => this[index]; set => throw new NotSupportedException(Commons.ErrorReadOnly); }
            Object IList.this[Int32 index] { get => this[index]; set => throw new NotSupportedException(Commons.ErrorReadOnly); }
            IEnumerator<Int32[]> IEnumerable<Int32[]>.GetEnumerator() => this.Iterator();
            IEnumerator IEnumerable.GetEnumerator() => this.Iterator();
            Boolean ICollection<Int32[]>.Contains(Int32[] item) => this.IndexOf(item) != -1;
            Boolean IList.Contains(Object value) => value is not null && this.IndexOf((Int32[])Convert.ChangeType(value, typeof(Int32[]), CultureInfo.CurrentCulture)) != -1;
            Int32 IList<Int32[]>.IndexOf(Int32[] item) => this.IndexOf(item);
            Int32 IList.IndexOf(Object value) => value is null || this._option != RandomMode.KeepAfterDone ? -1 : this.IndexOf((Int32[])Convert.ChangeType(value, typeof(Int32[]), CultureInfo.CurrentCulture));
            void ICollection.CopyTo(Array array, Int32 index)
            {
                if (array is null)
                {
                    throw new ArgumentNullException(nameof(array));
                }
                if (array.Rank != 1)
                {
                    throw new RankException("The destination array must be one dimensional array.");
                }
                if (index < 0)
                {
                    var next = array.Length + index;
                    if (next < 0 || next >= array.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index, "The index of element in array to begin writen is outside the valid range of array.");
                    }
                }
                if (index + this._length > array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, $"The destination array is not have enough spaces (currently have {array.Length}) to fill with the copied elements from this set ({this._length}) when starting at index {index}.");
                }
                if (array is Int32[][] blocks)
                {
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                        blocks[index++] = iterator.Current;
                }
                else if (array is Int64[][] longs)
                {
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                    {
                        var next = new Int64[this._length];
                        iterator.Current.CopyTo(next, 0);
                        longs[index++] = next;
                    }
                }
                else if (array is Object[] objects)
                {
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                        objects[index++] = iterator.Current;
                }
                else if (array is Object[][] jagged)
                {
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                    {
                        var bucket = new Object[this._length];
                        iterator.Current.CopyTo(bucket, 0);
                        jagged[index++] = bucket;
                    }
                }
                else if (array is String[] strings)
                {
                    var iterator = new Enumerator(this);
                    while (iterator.MoveNext())
                        strings[index++] = iterator.ToString();
                }
                else
                {
                    throw new ArrayTypeMismatchException($"The destination array type ({array.GetType().GetElementType()}) is not compatible with the current block set element type.");
                }

            }
            Boolean ICollection<Int32[]>.IsReadOnly => true;
            Object ICollection.SyncRoot => this;
            Boolean ICollection.IsSynchronized => false;
            Boolean IList.IsReadOnly => true;
            Boolean IList.IsFixedSize => true;

            void ICollection<Int32[]>.Add(Int32[] item) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void ICollection<Int32[]>.Clear() => throw new NotSupportedException(Commons.ErrorReadOnly);
            Boolean ICollection<Int32[]>.Remove(Int32[] item) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList<Int32[]>.Insert(Int32 index, Int32[] item) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList<Int32[]>.RemoveAt(Int32 index) => throw new NotSupportedException(Commons.ErrorReadOnly);
            Int32 IList.Add(Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList.Clear() => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList.Insert(Int32 index, Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList.Remove(Object value) => throw new NotSupportedException(Commons.ErrorReadOnly);
            void IList.RemoveAt(Int32 index) => throw new NotSupportedException(Commons.ErrorReadOnly);


        }

        /// <summary>
        /// Represents the internal configuration set of randomness token generation. This API is not intended to be used directly from user code.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed partial class TokenData
        {
            public Boolean Lined;
            public Int32 Count;
            public Int32 Length;
            public Int32 Group;
            public String Prefix;
            public String Suffix;
            public String Divisor;
            public CharMaps Chars;
            public CancellationToken Cancel;
            public StringBuilder Buffer;
            public RandomNumberGenerator Engine;
            public IntVector Bucket;

            public TokenData()
            {
                this.Prefix = "";
                this.Suffix = "";
                this.Divisor = "";
            }

            public TokenData Commit()
            {
                if (this.Length < 1) throw new InvalidOperationException("The length of each random tokens to generate must not zero or less than zero.");
                if (this.Group < 1) this.Group = 0; else if (this.Group > this.Length) this.Group = this.Length;
                if (this.Count < 0) this.Count = 0;
                if (this.Prefix is null) this.Prefix = "";
                if (this.Suffix is null) this.Suffix = "";
                if (this.Divisor is null) this.Divisor = "";
                if (this.Chars is null || this.Chars.Length == 0) this.Chars = CharMaps.Extract(MixedChars.Uppers | MixedChars.Number, false);
                if (this.Buffer is null) this.Buffer = new StringBuilder(8192); else if (this.Buffer.Length != 0) this.Buffer.Length = 0;
                if (this.Engine is null) this.Engine = Commons.DefaultEngine;
                if (this.Bucket is null) this.Bucket = new IntVector();
                return this;
            }
        }

        /// <summary>
        /// Represents the configuration set of randomness token generation, such as serial number blocks. This class is not inheritable.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public sealed partial class TokenInfo
        {
            public static TokenInfo Create() => new TokenInfo();

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TokenData config;

            public TokenInfo() => this.config = new TokenData();

            public TokenData GetConfig() => this.config;

            public TokenInfo SetLined(Boolean lined = true)
            {
                this.config.Lined = lined;
                return this;
            }

            public TokenInfo SetCount(Int32 count)
            {
                this.config.Count = count < 1 ? 0 : count;
                return this;
            }

            public TokenInfo SetLength(Int32 length)
            {
                this.config.Length = length < 1 ? 0 : length;
                return this;
            }

            public TokenInfo SetGroup(Int32 size)
            {
                this.config.Group = size < 1 ? 0 : size;
                return this;
            }

            public TokenInfo SetPrefix(String prefix)
            {
                this.config.Prefix = prefix is null || prefix.Length == 0 ? "" : prefix;
                return this;
            }

            public TokenInfo SetSuffix(String suffix)
            {
                this.config.Suffix = suffix is null || suffix.Length == 0 ? "" : suffix;
                return this;
            }

            public TokenInfo SetDivisor(String divisor = "-")
            {
                this.config.Divisor = divisor is null || divisor.Length == 0 ? "" : divisor;
                return this;
            }

            public TokenInfo SetChars(MixedChars chars = MixedChars.Default)
            {
                this.config.Chars = CharMaps.Extract(chars, false);
                return this;
            }

            public TokenInfo SetCancel(CancellationToken ctoken)
            {
                this.config.Cancel = ctoken;
                return this;
            }

            public TokenInfo SetBuffer(StringBuilder buffer)
            {
                this.config.Buffer = buffer;
                return this;
            }

            public TokenInfo SetPattern(String pattern)
            {
                this.config.Chars = pattern is null || pattern.Length == 0 ? null : new CharMaps(new CharData[] { new CharData(pattern) });
                return this;
            }

            public TokenInfo SetPattern(String pattern, params String[] patterns)
            {
                var has1 = pattern is not null && pattern.Length != 0;
                var has2 = patterns is not null && patterns.Length != 0;
                if (has1 || has2)
                {
                    var buffer = new List<CharData>((has1 ? 1 : 0) + (has2 ? patterns.Length : 0));
                    if (has1)
                        buffer.Add(new CharData(pattern));
                    if (has2)
                    {
                        for (var i = 0; i < patterns.Length; i++)
                        {
                            var next = patterns[i];
                            if (next is not null && next.Length != 0)
                                buffer.Add(new CharData(next));
                        }
                    }
                    this.config.Chars = new CharMaps(buffer.ToArray());
                    buffer.Clear();
                }
                else
                {
                    this.config.Chars = null;
                }
                return this;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public TokenInfo SetEngine(RandomNumberGenerator engine)
            {
                this.config.Engine = engine;
                return this;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public TokenInfo SetBucket(IntVector bucket)
            {
                this.config.Bucket = bucket;
                return this;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public TokenInfo SetConfig(TokenData config)
            {
                if (config is null)
                {
                    this.config = new TokenData();
                }
                else if (!ReferenceEquals(this.config, config))
                {
                    this.config = config;
                }
                return this;
            }

        }

        private sealed partial class RandomProxy : Random
        {
            private readonly RandomAPI _engine;

            public RandomProxy() => this._engine = new RandomAPI();

            public RandomProxy(Int32 seed) : base(seed) => this._engine = new RandomAPI();

            public override Int32 Next() => this._engine.Next();

            public override Int32 Next(Int32 maxValue) => this._engine.Next(maxValue);

            public override Int32 Next(Int32 minValue, Int32 maxValue) => this._engine.Next(minValue, maxValue);

            public override void NextBytes(Byte[] buffer)
            {
                if (buffer is not null && buffer.Length != 0)
                {
                    this._engine._engine.GetBytes(buffer);
                }
            }

            public override Double NextDouble()
                => this._engine.NextDouble();
        }

        public static Random AsRandom() => new RandomProxy();

        public static RandomAPI NewClass() => new();

        public static Int32 PushRange(Int32 lbound, Int32 ubound)
            => Internals.NextRange(lbound, ubound, Commons.DefaultEngine, Commons.DefaultBucket);

        public static Int32[] PushRange(Int32 count, Int32 lbound, Int32 ubound)
            => Internals.NextRange(count, lbound, ubound, Commons.DefaultEngine, Commons.DefaultBucket);

#if CLS
        [CLSCompliant(false)]
#endif
        public static UInt32 PushUInt32()
            => Internals.NextUInt32(Commons.DefaultEngine, Commons.DefaultBucket);

#if CLS
        [CLSCompliant(false)]
#endif
        public static UInt32[] PushUInt32(Int32 count)
            => Internals.NextUInt32(count, Commons.DefaultEngine, Commons.DefaultBucket);

        public static Double PushDouble()
            => Internals.NextDouble(Commons.DefaultEngine, Commons.DefaultBucket);

        public static Double[] PushDouble(Int32 count)
            => Internals.NextDouble(count, Commons.DefaultEngine, Commons.DefaultBucket);

        public static Double PushDouble(Double minimum, Double maximum)
            => minimum + PushDouble() * (maximum - minimum);

        public static String PushKeys(Int32 length)
            => Internals.NextKeys(length, MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String PushKeys(Int32 length, MixedChars chars)
            => Internals.NextKeys(length, chars, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String[] PushKeys(Int32 count, Int32 length)
            => Internals.NextKeys(count, length, MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String[] PushKeys(Int32 count, Int32 length, MixedChars chars)
            => Internals.NextKeys(count, length, chars, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String PushSerial(Int32 serialLength, Int32 groupLength)
            => Internals.NextSerial(serialLength, groupLength, "-", MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String PushSerial(Int32 serialLength, Int32 groupLength, String separator)
            => Internals.NextSerial(serialLength, groupLength, separator, MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String PushSerial(Int32 serialLength, Int32 groupLength, String separator, MixedChars components)
            => Internals.NextSerial(serialLength, groupLength, separator, components, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String[] PushSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, "-", MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String[] PushSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength, String separator)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, separator, MixedChars.Default, Commons.DefaultEngine, Commons.DefaultBucket);

        public static String[] PushSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength, String separator, MixedChars components)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, separator, components, Commons.DefaultEngine, Commons.DefaultBucket);

        public static Byte[] PushBytes(Int32 length)
        {
            if (length < 1)
            {
                return Array.Empty<Byte>();
            }
            else
            {
                var array = new Byte[length];
                Commons.DefaultEngine.GetBytes(array);
                return array;
            }
        }

        public static Int32 PushBytes(Byte[] bytes)
        {
            var length = bytes is null ? 0 : bytes.Length;
            if (length != 0)
            {
                Commons.DefaultEngine.GetBytes(bytes);
                return length;
            }
            return 0;
        }

        public static Int32 PushBytes(Byte[] bytes, Int32 offset, Int32 length)
        {
            if (bytes is not null && length > 0)
            {
                Commons.CheckRange(bytes.Length, ref offset, ref length, nameof(bytes), nameof(offset), nameof(length));
                Commons.DefaultEngine.GetBytes(bytes, offset, length);
                return length;
            }
            return 0;
        }

        public static Int32 PushBytes(Stream stream, Int32 length)
        {
            if (stream is not null && stream.CanWrite && length > 0)
            {
                var packet = Commons.CacheLength;
                var buffer = Commons.DefaultBuffer;
                var engine = Commons.DefaultEngine;
                var needed = length;
                while (needed > 0)
                {
                    if (packet > needed) packet = needed;
                    engine.GetBytes(buffer, 0, packet);
                    stream.Write(buffer, 0, packet);
                    needed -= packet;
                }
                return length;
            }
            return 0;
        }

        public static Int64 PushBytes(Stream stream, Int64 length)
        {
            if (stream is not null && stream.CanWrite && length > 0)
            {
                var packet = Commons.CacheLength;
                var buffer = Commons.DefaultBuffer;
                var engine = Commons.DefaultEngine;
                var needed = length;
                while (needed > 0)
                {
                    if (packet > needed) packet = (Int32)needed;
                    engine.GetBytes(buffer, 0, packet);
                    stream.Write(buffer, 0, packet);
                    needed -= packet;
                }
                return length;
            }
            return 0;
        }

        public static async Task<Int32> PushBytes(Stream stream, Int32 length, CancellationToken ctoken)
        {
            if (stream is not null && stream.CanWrite && length > 0 && !ctoken.IsCancellationRequested)
            {
                var packet = Commons.CacheLength;
                var buffer = Commons.DefaultBuffer;
                var engine = Commons.DefaultEngine;
                var needed = length;
                while (needed > 0 && !ctoken.IsCancellationRequested)
                {
                    if (packet > needed) packet = needed;
                    engine.GetBytes(buffer, 0, packet);
                    await stream.WriteAsync(buffer, 0, packet, ctoken);
                    needed -= packet;
                }
                return length - needed;
            }
            return 0;
        }

        public static async Task<Int64> PushBytes(Stream stream, Int64 length, CancellationToken ctoken)
        {
            if (stream is not null && stream.CanWrite && length > 0 && !ctoken.IsCancellationRequested)
            {
                var packet = Commons.CacheLength;
                var buffer = Commons.DefaultBuffer;
                var engine = Commons.DefaultEngine;
                var needed = length;
                while (needed > 0 && !ctoken.IsCancellationRequested)
                {
                    if (packet > needed) packet = (Int32)needed;
                    engine.GetBytes(buffer, 0, packet);
                    await stream.WriteAsync(buffer, 0, packet, ctoken);
                    needed -= packet;
                }
                return length - needed;
            }
            return 0;
        }

        public static Int32 PushToken(TextWriter output, TokenInfo config)
        {
            if (output is not null)
            {
                Commons.CheckNullRef(config, nameof(config));
                var args = config.GetConfig().Commit();
                var count = args.Count;
                if (count != 0 && !args.Cancel.IsCancellationRequested)
                {
                    const Int32 HAS_LINE = 0x1, HAS_PREF = 0x2, HAS_SUFX = 0x4, HAS_DIVS = 0x8;
                    var flags = 0x0;
                    var ctoken = args.Cancel;
                    var async = ctoken.CanBeCanceled;
                    var suffix = args.Suffix;
                    var prefix = args.Prefix;
                    var divisor = args.Divisor;
                    var buffer = args.Buffer;
                    if (args.Lined) flags |= HAS_LINE;
                    if (prefix.Length != 0) flags |= HAS_PREF;
                    if (suffix.Length != 0) flags |= HAS_SUFX;
                    if (divisor.Length != 0) flags |= HAS_DIVS;
                    if (args.Length == 0)
                    {
                        if (flags != 0)
                        {
                            if ((flags & HAS_PREF) != 0x0) _ = buffer.Append(prefix);
                            if ((flags & HAS_SUFX) != 0x0) _ = buffer.Append(suffix);
                            if ((flags & HAS_DIVS) != 0x0) _ = buffer.Append(divisor);
                            if ((flags & HAS_LINE) != 0x0) _ = buffer.AppendLine();
                            var content = buffer.ToString();
                            buffer.Length = 0;
                            if (async)
                            {
                                while (!ctoken.IsCancellationRequested && count-- > 0)
                                    output.WriteAsync(content).Wait();
                            }
                            else
                            {
                                while (count-- > 0)
                                    output.Write(content);
                            }
                        }
                    }
                    else
                    {
                        var mapping = args.Chars;
                        if (args.Group < 1)
                        {
                            while ((!async || !ctoken.IsCancellationRequested) && count-- > 0)
                            {
                                var need = args.Length;
                                if ((flags & HAS_PREF) != 0x0)
                                    _ = buffer.Append(prefix);
                                while (need-- > 0)
                                    _ = buffer.Append(mapping.Random(args.Engine, args.Bucket));
                                if ((flags & HAS_SUFX) != 0x0)
                                    _ = buffer.Append(suffix);
                                if (async)
                                    output.WriteAsync((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString()).Wait();
                                else
                                    output.Write((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString());
                                buffer.Length = 0;
                            }
                        }
                        else
                        {
                            while ((!async || !ctoken.IsCancellationRequested) && count-- > 0)
                            {
                                var need = args.Length;
                                while (need > 0)
                                {
                                    var group = args.Group;
                                    if (group > need) group = need;
                                    need -= group;
                                    if ((flags & HAS_PREF) != 0x0)
                                        _ = buffer.Append(prefix);
                                    while (group-- > 0)
                                    {
                                        _ = buffer.Append(mapping.Random(args.Engine, args.Bucket));
                                    }
                                    if ((flags & HAS_SUFX) != 0x0)
                                        _ = buffer.Append(suffix);
                                    if (need > 0 && (flags & HAS_DIVS) != 0x0)
                                        _ = buffer.Append(divisor);
                                }
                                if (async)
                                    output.WriteAsync((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString()).Wait();
                                else
                                    output.Write((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString());
                                buffer.Length = 0;
                            }
                        }

                    }
                }
                return args.Count;
            }
            return 0;
        }

        public static Int32 PushToken(TextWriter output, Action<TokenInfo> config)
        {
            if (output is not null)
            {
                Commons.CheckNullRef(config, nameof(config));
                var info = new TokenInfo();
                config.Invoke(info);
                return PushToken(output, info);
            }
            return 0;
        }

        public static Int32 PushToken(StringBuilder output, TokenInfo config)
        {
            if (output is not null)
            {
                Commons.CheckNullRef(config, nameof(config));
                var args = config.GetConfig().Commit();
                var count = args.Count;
                if (count != 0 && !args.Cancel.IsCancellationRequested)
                {
                    const Int32 HAS_LINE = 0x1, HAS_PREF = 0x2, HAS_SUFX = 0x4, HAS_DIVS = 0x8;
                    var flags = 0x0;
                    var ctoken = args.Cancel;
                    var async = ctoken.CanBeCanceled;
                    var suffix = args.Suffix;
                    var prefix = args.Prefix;
                    var divisor = args.Divisor;
                    var buffer = args.Buffer;
                    if (ReferenceEquals(output, buffer)) buffer = new StringBuilder(8192);
                    if (args.Lined) flags |= HAS_LINE;
                    if (prefix.Length != 0) flags |= HAS_PREF;
                    if (suffix.Length != 0) flags |= HAS_SUFX;
                    if (divisor.Length != 0) flags |= HAS_DIVS;
                    if (args.Length == 0)
                    {
                        if (flags != 0)
                        {
                            if ((flags & HAS_PREF) != 0x0) _ = buffer.Append(prefix);
                            if ((flags & HAS_SUFX) != 0x0) _ = buffer.Append(suffix);
                            if ((flags & HAS_DIVS) != 0x0) _ = buffer.Append(divisor);
                            if ((flags & HAS_LINE) != 0x0) _ = buffer.AppendLine();
                            var content = buffer.ToString();
                            buffer.Length = 0;
                            while (count-- > 0) _ = output.Append(content);
                        }
                    }
                    else
                    {
                        var mapping = args.Chars;
                        if (args.Group < 1)
                        {
                            while ((!async || !ctoken.IsCancellationRequested) && count-- > 0)
                            {
                                var need = args.Length;
                                if ((flags & HAS_PREF) != 0x0)
                                    _ = buffer.Append(prefix);
                                while (need-- > 0)
                                    _ = buffer.Append(mapping.Random(args.Engine, args.Bucket));
                                if ((flags & HAS_SUFX) != 0x0)
                                    _ = buffer.Append(suffix);
                                _ = output.Append((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString());
                                buffer.Length = 0;
                            }
                        }
                        else
                        {
                            while ((!async || !ctoken.IsCancellationRequested) && count-- > 0)
                            {
                                var need = args.Length;
                                while (need > 0)
                                {
                                    var group = args.Group;
                                    if (group > need) group = need;
                                    need -= group;
                                    if ((flags & HAS_PREF) != 0x0)
                                        _ = buffer.Append(prefix);
                                    while (group-- > 0)
                                    {
                                        _ = buffer.Append(mapping.Random(args.Engine, args.Bucket));
                                    }
                                    if ((flags & HAS_SUFX) != 0x0)
                                        _ = buffer.Append(suffix);
                                    if (need > 0 && (flags & HAS_DIVS) != 0x0)
                                        _ = buffer.Append(divisor);
                                }
                                _ = output.Append((flags & HAS_LINE) != 0x0 && count > 0 ? buffer.AppendLine().ToString() : buffer.ToString());
                                buffer.Length = 0;
                            }
                        }
                    }
                }
                return args.Count;
            }
            return 0;
        }

        public static Int32 PushToken(StringBuilder output, Action<TokenInfo> config)
        {
            if (output is not null)
            {
                Commons.CheckNullRef(config, nameof(config));
                var info = new TokenInfo();
                config.Invoke(info);
                return PushToken(output, info);
            }
            return 0;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly RandomNumberGenerator _engine;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IntVector _bucket;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Byte[] _buffer;

        public RandomAPI()
        {
#if NETFX
            this._engine = new RNGCryptoServiceProvider();
#else
            this._engine = RandomNumberGenerator.Create();
#endif
            this._bucket = new IntVector();
            this._buffer = new Byte[Commons.CacheLength];
        }

        public RandomAPI(RandomNumberGenerator engine)
        {
            if (engine is null)
            {
#if NETFX
                this._engine = new RNGCryptoServiceProvider();
#else
                this._engine = RandomNumberGenerator.Create();
#endif
            }
            else
            {
                this._engine = engine;
            }
            this._bucket = new IntVector();
            this._buffer = new Byte[Commons.CacheLength];
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never), EditorBrowsable(EditorBrowsableState.Never)]
        public RandomNumberGenerator Engine => this._engine;

        public Int32 Next() => Internals.NextInt32(this._engine, this._bucket);

        public Int32 Next(Int32 ubound) => Internals.NextInt32(ubound, this._engine, this._bucket);

        public Int32 Next(Int32 lbound, Int32 ubound)
            => Internals.NextRange(lbound, ubound, this._engine, this._bucket);

        public Int32 NextRange(Int32 lbound, Int32 ubound)
            => Internals.NextRange(lbound, ubound, this._engine, this._bucket);

        public Int32[] NextRange(Int32 count, Int32 lbound, Int32 ubound)
            => Internals.NextRange(count, lbound, ubound, this._engine, this._bucket);

        public Double NextDouble()
            => Internals.NextDouble(this._engine, this._bucket);

        public Double[] NextDouble(Int32 count)
            => Internals.NextDouble(count, this._engine, this._bucket);

        public Double NextDouble(Double minimum, Double maximum)
            => minimum + this.NextDouble() * (maximum - minimum);

        public Byte[] NextBytes(Int32 length)
        {
            if (length < 1)
            {
                return Array.Empty<Byte>();
            }
            else
            {
                var array = new Byte[length];
                this._engine.GetBytes(array);
                return array;
            }
        }

        public Int32 NextBytes(Byte[] bytes)
        {
            var length = bytes is null ? 0 : bytes.Length;
            if (length != 0)
            {
                this._engine.GetBytes(bytes);
                return length;
            }
            return 0;
        }

        public Int32 NextBytes(Byte[] bytes, Int32 offset, Int32 length)
        {
            if (bytes is not null && length > 0)
            {
                Commons.CheckRange(bytes.Length, ref offset, ref length, nameof(bytes), nameof(offset), nameof(length));
                this._engine.GetBytes(bytes, offset, length);
                return length;
            }
            return 0;
        }

        public Int32 NextBytes(Stream stream, Int32 length)
        {
            if (stream is not null && stream.CanWrite && length > 0)
            {
                var packet = Commons.CacheLength;
                var buffer = this._buffer;
                var engine = this._engine;
                var needed = length;
                while (needed > 0)
                {
                    if (packet > needed) packet = needed;
                    engine.GetBytes(buffer, 0, packet);
                    stream.Write(buffer, 0, packet);
                    needed -= packet;
                }
                return length;
            }
            return 0;
        }

        public Int64 NextBytes(Stream stream, Int64 length)
        {
            if (stream is not null && stream.CanWrite && length > 0)
            {
                var packet = Commons.CacheLength;
                var buffer = this._buffer;
                var engine = this._engine;
                var needed = length;
                while (needed > 0)
                {
                    if (packet > needed) packet = (Int32)needed;
                    engine.GetBytes(buffer, 0, packet);
                    stream.Write(buffer, 0, packet);
                    needed -= packet;
                }
                return length;
            }
            return 0;
        }

        public async Task<Int32> NextBytes(Stream stream, Int32 length, CancellationToken ctoken)
        {
            if (stream is not null && stream.CanWrite && length > 0 && !ctoken.IsCancellationRequested)
            {
                var packet = Commons.CacheLength;
                var buffer = this._buffer;
                var engine = this._engine;
                var needed = length;
                while (needed > 0 && !ctoken.IsCancellationRequested)
                {
                    if (packet > needed) packet = needed;
                    engine.GetBytes(buffer, 0, packet);
                    await stream.WriteAsync(buffer, 0, packet, ctoken);
                    needed -= packet;
                }
                return length - needed;
            }
            return 0;
        }

        public async Task<Int64> NextBytes(Stream stream, Int64 length, CancellationToken ctoken)
        {
            if (stream is not null && stream.CanWrite && length > 0 && !ctoken.IsCancellationRequested)
            {
                var packet = Commons.CacheLength;
                var buffer = this._buffer;
                var engine = this._engine;
                var needed = length;
                while (needed > 0 && !ctoken.IsCancellationRequested)
                {
                    if (packet > needed) packet = (Int32)needed;
                    engine.GetBytes(buffer, 0, packet);
                    await stream.WriteAsync(buffer, 0, packet, ctoken);
                    needed -= packet;
                }
                return length - needed;
            }
            return 0;
        }

#if CLS
        [CLSCompliant(false)]
#endif
        public UInt32 NextUInt32() => Internals.NextUInt32(this._engine, this._bucket);

#if CLS
        [CLSCompliant(false)]
#endif
        public UInt32[] NextUInt32(Int32 count) => Internals.NextUInt32(count, this._engine, this._bucket);

        public String NextKeys(Int32 length)
            => Internals.NextKeys(length, MixedChars.Default, this._engine, this._bucket);

        public String NextKeys(Int32 length, MixedChars chars)
            => Internals.NextKeys(length, chars, this._engine, this._bucket);

        public String[] NextKeys(Int32 count, Int32 length)
            => Internals.NextKeys(count, length, MixedChars.Default, this._engine, this._bucket);

        public String[] NextKeys(Int32 count, Int32 length, MixedChars chars)
            => Internals.NextKeys(count, length, chars, this._engine, this._bucket);

        public String NextSerial(Int32 serialLength, Int32 groupLength)
            => Internals.NextSerial(serialLength, groupLength, "-", MixedChars.Default, this._engine, this._bucket);

        public String NextSerial(Int32 serialLength, Int32 groupLength, String separator)
            => Internals.NextSerial(serialLength, groupLength, separator, MixedChars.Default, this._engine, this._bucket);

        public String NextSerial(Int32 serialLength, Int32 groupLength, String separator, MixedChars components)
            => Internals.NextSerial(serialLength, groupLength, separator, components, this._engine, this._bucket);

        public String[] NextSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, "-", MixedChars.Default, this._engine, this._bucket);

        public String[] NextSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength, String separator)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, separator, MixedChars.Default, this._engine, this._bucket);

        public String[] NextSerial(Int32 totalSerials, Int32 serialLength, Int32 groupLength, String separator, MixedChars components)
            => Internals.NextSerial(totalSerials, serialLength, groupLength, separator, components, this._engine, this._bucket);
    }
}
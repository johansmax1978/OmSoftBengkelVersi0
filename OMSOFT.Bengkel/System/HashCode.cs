namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides the default FNV-a1 implementation of the non cryptographic hash code generator.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = sizeof(UInt32), Size = sizeof(UInt32)), SkipLocalsInit]
    public unsafe partial struct HashCode
    {
        #region >>> private members

        private sealed partial class EnumerableHashes<T> : IEnumerable<Int32>
        {
            private static readonly Boolean IsValueType = typeof(T).IsValueType;

            public readonly IEnumerable<T> Sequences;

            public EnumerableHashes(IEnumerable<T> sequences) => this.Sequences = sequences;

            private struct EnumeratorValueType : IEnumerator<Int32>
            {
                private readonly EnumerableHashes<T> parent;
                private IEnumerator<T> iterator;
                private Int32? current;

                public EnumeratorValueType(EnumerableHashes<T> parent)
                {
                    this.parent = parent;
                    this.iterator = parent.Sequences.GetEnumerator();
                    this.current = null;
                }

                public Int32 Current
                {
                    get
                    {
                        var value = this.current;
                        if (!value.HasValue)
                        {
                            var entry = this.iterator.Current;
                            var hash = entry.GetHashCode();
                            this.current = hash;
                            return hash;
                        }
                        return value.Value;
                    }
                }

                public readonly void Dispose() => this.iterator.Dispose();

                public Boolean MoveNext()
                {
                    this.current = null;
                    return this.iterator.MoveNext();
                }

                public void Reset()
                {
                    this.iterator?.Dispose();
                    this.iterator = this.parent.Sequences.GetEnumerator();
                    this.current = null;
                }

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                Object IEnumerator.Current => this.Current;
            }

            private struct EnumeratorClassType : IEnumerator<Int32>
            {
                private readonly EnumerableHashes<T> parent;
                private IEnumerator<T> iterator;
                private Int32? current;

                public EnumeratorClassType(EnumerableHashes<T> parent)
                {
                    this.parent = parent;
                    this.iterator = parent.Sequences.GetEnumerator();
                    this.current = null;
                }

                public Int32 Current
                {
                    get
                    {
                        var value = this.current;
                        if (!value.HasValue)
                        {
                            var entry = this.iterator.Current;
                            var hash = entry is null ? 0 : entry.GetHashCode();
                            this.current = hash;
                            return hash;
                        }
                        return value.Value;
                    }
                }

                public readonly void Dispose() => this.iterator.Dispose();

                public Boolean MoveNext()
                {
                    this.current = null;
                    return this.iterator.MoveNext();
                }

                public void Reset()
                {
                    this.iterator?.Dispose();
                    this.iterator = this.parent.Sequences.GetEnumerator();
                    this.current = null;
                }

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                Object IEnumerator.Current => this.Current;
            }

            public IEnumerator<Int32> GetEnumerator() => IsValueType ? new EnumeratorValueType(this) : new EnumeratorClassType(this);

            IEnumerator IEnumerable.GetEnumerator() => IsValueType ? new EnumeratorValueType(this) : new EnumeratorClassType(this);
        }

        private const UInt32 FnvBasis = 0x811C9DC5, FnvPrime = 0x01000193;

        private static UInt32 InternalCombine(UInt32 hash, Int32 value)
        {
            var hv = hash;
            var ps = (Byte*)&value;
            hv = (hv ^ ps[0]) * FnvPrime;
            hv = (hv ^ ps[1]) * FnvPrime;
            hv = (hv ^ ps[2]) * FnvPrime;
            return (hv ^ ps[3]) * FnvPrime;
        }

        private static UInt32 InternalCombine(UInt32 hash, UInt32 value)
        {
            var hv = hash;
            var ps = (Byte*)&value;
            hv = (hv ^ ps[0]) * FnvPrime;
            hv = (hv ^ ps[1]) * FnvPrime;
            hv = (hv ^ ps[2]) * FnvPrime;
            return (hv ^ ps[3]) * FnvPrime;
        }

        private static UInt32 InternalCombine(UInt32 hash, void* data, Int32 size)
        {
            var hv = hash;
            var cb = size;
            var ps = (Byte*)data;
            while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            return hv;
        }

        private static Int32 FastMixHash(params Int32[] array)
        {
            unchecked
            {
                var hv = FnvBasis;
                var cb = array.Length * sizeof(Int32);
                fixed (Int32* pointer = array)
                {
                    var ps = (Byte*)pointer;
                    while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
                }
                return (Int32)hv;
            }
        }

        #endregion

        #region >>> MixHash methods

        /// <summary>
        /// Create a new empty <see cref="HashCode"/> instance for hash code combinations.
        /// </summary>
        /// <returns>A new empty instance of the <see cref="HashCode"/> for hashes combination.</returns>
        public static HashCode Create() => default;

        /// <summary>
        /// Combine one hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash">Specify the 1st hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine two hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine three hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine four hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <param name="hash4">Specify the 4th hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash4;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine five hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <param name="hash4">Specify the 4th hash code value that should be combined with the others.</param>
        /// <param name="hash5">Specify the 5th hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4, Int32 hash5)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash4;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash5;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine six hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <param name="hash4">Specify the 4th hash code value that should be combined with the others.</param>
        /// <param name="hash5">Specify the 5th hash code value that should be combined with the others.</param>
        /// <param name="hash6">Specify the 6th hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4, Int32 hash5, Int32 hash6)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash4;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash5;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash6;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine seven hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <param name="hash4">Specify the 4th hash code value that should be combined with the others.</param>
        /// <param name="hash5">Specify the 5th hash code value that should be combined with the others.</param>
        /// <param name="hash6">Specify the 6th hash code value that should be combined with the others.</param>
        /// <param name="hash7">Specify the 7th hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4, Int32 hash5, Int32 hash6, Int32 hash7)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash4;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash5;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash6;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash7;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine eight hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hash1">Specify the 1st hash code value that should be combined with the others.</param>
        /// <param name="hash2">Specify the 2nd hash code value that should be combined with the others.</param>
        /// <param name="hash3">Specify the 3rd hash code value that should be combined with the others.</param>
        /// <param name="hash4">Specify the 4th hash code value that should be combined with the others.</param>
        /// <param name="hash5">Specify the 5th hash code value that should be combined with the others.</param>
        /// <param name="hash6">Specify the 6th hash code value that should be combined with the others.</param>
        /// <param name="hash7">Specify the 7th hash code value that should be combined with the others.</param>
        /// <param name="hash8">Specify the 8th hash code value that should be combined with the others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code of the 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4, Int32 hash5, Int32 hash6, Int32 hash7, Int32 hash8)
        {
            unchecked
            {
                var hv = FnvBasis;
                var ps = (Byte*)&hash1;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash2;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash3;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash4;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash5;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash6;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash7;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;

                ps = (Byte*)&hash8;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine arbitarily number of hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hashes">Specify one or more hash code values that should combined into a single hash code value.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final combined of the hash code values using 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(params Int32[] hashes)
        {
            unchecked
            {
                var hv = FnvBasis;
                var cb = hashes is null ? 0 : hashes.Length;
                if (cb != 0)
                {
                    fixed (Int32* pointer = hashes)
                    {
                        var ps = (Byte*)pointer;
                        cb *= sizeof(Int32);
                        while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
                    }
                }
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine arbitarily number of hash code values into a single hash code using 32-bit FNV-1a algorithm.
        /// </summary>
        /// <param name="hashes">Set the sequence of <see cref="Int32"/> value that is the hash codes to be combined with the each others.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final combined of the hash code values using 32-bit FNV-1a algorithm.</returns>
        public static Int32 MixHash(IEnumerable<Int32> hashes)
        {
            unchecked
            {
                var hv = FnvBasis;
                if (hashes is not null)
                {
                    Int32 iv;
                    Byte* ps;
                    using var iterator = hashes.GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        iv = iterator.Current;
                        ps = (Byte*)&iv;
                        hv = (hv ^ ps[0]) * FnvPrime;
                        hv = (hv ^ ps[1]) * FnvPrime;
                        hv = (hv ^ ps[2]) * FnvPrime;
                        hv = (hv ^ ps[3]) * FnvPrime;
                    }
                }
                return (Int32)hv;
            }
        }

        #endregion

        #region >>> Declaration members

        /// <summary>
        /// Declare the current actual hash code value. The initialization is needed if this field is still zero.
        /// </summary>
        [FieldOffset(0)]
        internal UInt32 hashvalue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCode"/> structure by supplying the initial seed for hash code.
        /// </summary>
        /// <param name="seed">Set the unique seed value, that is the initial data to seed, or zero to skip the initial seed.</param>
        /// <filterpriority>2</filterpriority>
        public HashCode(Int32 seed)
        {
            if (seed != 0)
            {
                var hv = FnvBasis;
                var ps = (Byte*)&seed;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                this.hashvalue = hv;
            }
            else
            {
                this.hashvalue = FnvBasis;
            }
        }

        #endregion

        #region >>> Add methods

        /// <summary>
        /// Combine the current hash code with the specified 8-bit unsigned integer <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Set the <see cref="Byte"/> value that will be combined with the calculated hash code.</param>
        public void Add(Byte value)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            hv = (hv ^ value) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the specified 32-bit signed integer <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Set the <see cref="Int32"/> value that will be combined with the calculated hash code.</param>
        public void Add(Int32 value)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var ps = (Byte*)&value;
            hv = (hv ^ ps[0]) * FnvPrime;
            hv = (hv ^ ps[1]) * FnvPrime;
            hv = (hv ^ ps[2]) * FnvPrime;
            hv = (hv ^ ps[3]) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the specified 32-bit unsigned integer <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Set the <see cref="UInt32"/> value that will be combined with the calculated hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void Add(UInt32 value)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var ps = (Byte*)&value;
            hv = (hv ^ ps[0]) * FnvPrime;
            hv = (hv ^ ps[1]) * FnvPrime;
            hv = (hv ^ ps[2]) * FnvPrime;
            hv = (hv ^ ps[3]) * FnvPrime;
            this.hashvalue = hv;
        }

        #endregion

        #region >>> AddHash methods

        /// <summary>
        /// Combine the current hash code with the hash code value of the specified <see cref="Object"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> instance to get the hash code value and combine with the current hash.</param>
        public void AddHash(Object value) => this.Add(value is null ? 0 : value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <typeparamref name="T"/> structure value.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the structure to calculate the hash code and combine with the current.</typeparam>
        /// <param name="value">The <typeparamref name="T"/> value to call the <see cref="ValueType.GetHashCode"/> implementation and combine.</param>
        public void AddHash<T>(T value) where T : struct => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="SByte"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to get the hash code and combine with the current hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddHash(SByte value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Int16"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Int16 value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Int32"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Int32 value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Int64"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Int64 value) => this.Add((Int32)value ^ (Int32)(value >> 32));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="IntPtr"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="IntPtr"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(nint value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Byte"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Byte value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="UInt16"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to get the hash code and combine with the current hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddHash(UInt16 value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="UInt32"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to get the hash code and combine with the current hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddHash(UInt32 value) => this.Add(value);

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="UInt64"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to get the hash code and combine with the current hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddHash(UInt64 value) => this.Add((UInt32)value ^ (UInt32)(value >> 32));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="UIntPtr"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="UIntPtr"/> value to get the hash code and combine with the current hash code.</param>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddHash(nuint value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Double"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Double value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Single"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Single value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Decimal"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Decimal value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="DateTime"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(DateTime value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="DateTimeOffset"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(DateTimeOffset value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="TimeSpan"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(TimeSpan value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="Guid"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(Guid value) => this.Add(value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="String"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to get the hash code and combine with the current hash code.</param>
        public void AddHash(String value) => this.Add(value is null ? 0 : value.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="String"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to get the hash code and combine with the current hash code.</param>
        /// <param name="comparer">Set the <see cref="StringComparer"/> to compute the hash code of the given <see cref="String"/>.</param>
        public void AddHash(String value, StringComparer comparer) => this.Add(value is null ? 0 : comparer is null ? value.GetHashCode() : comparer.GetHashCode(value));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="String"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to get the hash code and combine with the current hash code.</param>
        /// <param name="comparison">Choose the <see cref="StringComparison"/> to get the hash producer for the given <see cref="String"/>.</param>
        public void AddHash(String value, StringComparison comparison) => this.Add(value is null ? 0 : (comparison switch { StringComparison.Ordinal => StringComparer.Ordinal, StringComparison.CurrentCulture => StringComparer.CurrentCulture, StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase, StringComparison.InvariantCulture => StringComparer.InvariantCulture, StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase, StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase, _ => StringComparer.Ordinal }).GetHashCode(value));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified <see cref="String"/> data type.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to get the hash code and combine with the current hash code.</param>
        /// <param name="equality">Set the <see cref="IEqualityComparer{T}"/> to compute the hash code of the given <see cref="String"/>.</param>
        public void AddHash(String value, IEqualityComparer<String> equality) => this.Add(value is null ? 0 : equality is null ? value.GetHashCode() : equality.GetHashCode(value));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified runtime <see cref="Type"/> object.
        /// </summary>
        /// <param name="type">The runtime <see cref="Type"/> to get the hash code and combine with the current hash code.</param>
        public void AddHash(Type type) => this.Add(RuntimeHelpers.GetHashCode(type));

        /// <summary>
        /// Combine the current hash code with the hash code of the specified runtime <see cref="MemberInfo"/> object.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> object to get the hash code and combine with the current hash code.</param>
        public void AddHash(MemberInfo member) => this.Add(member is null ? 0 : member.GetHashCode());

        /// <summary>
        /// Combine the current hash code with the hash code values from one or more of specified runtime <paramref name="types"/>.
        /// </summary>
        /// <param name="types">Set The array of the runtime <see cref="Type"/> objects to combine each of hashes with the current hash code.</param>
        public void AddHash(Type[] types)
        {
            if (types is not null)
            {
                var hv = this.hashvalue;
                if (hv == 0) hv = FnvBasis;
                for (var i = 0; i < types.Length; i++)
                {
                    var hash = RuntimeHelpers.GetHashCode(types[i]);
                    var ps = (Byte*)&hash;
                    hv = (hv ^ ps[0]) * FnvPrime;
                    hv = (hv ^ ps[1]) * FnvPrime;
                    hv = (hv ^ ps[2]) * FnvPrime;
                    hv = (hv ^ ps[3]) * FnvPrime;
                }
                this.hashvalue = hv;
            }
        }

        /// <summary>
        /// Combine the current hash code with the hash code values from one or more of specified runtime <paramref name="types"/>.
        /// </summary>
        /// <param name="types">The sequence of the runtime <see cref="Type"/> objects to combine each of hashes with the current hash code.</param>
        public void AddHash(IEnumerable<Type> types)
        {
            if (types is not null)
            {
                var hv = this.hashvalue;
                if (hv == 0) hv = FnvBasis;
                using var iterator = types.GetEnumerator();
                while (iterator.MoveNext())
                {
                    var type = iterator.Current;
                    var hash = RuntimeHelpers.GetHashCode(type);
                    var ps = (Byte*)&hash;
                    hv = (hv ^ ps[0]) * FnvPrime;
                    hv = (hv ^ ps[1]) * FnvPrime;
                    hv = (hv ^ ps[2]) * FnvPrime;
                    hv = (hv ^ ps[3]) * FnvPrime;
                }
                this.hashvalue = hv;
            }
        }

        #endregion

        #region >>> AddBytes methods

        /// <summary>
        /// Combine the current hash code with the contents that pointed in the specified memory <paramref name="buffer"/> for specified <paramref name="length"/> of bytes.
        /// </summary>
        /// <param name="buffer">The pointer address of the valid region (block) in the memory to read and combine with the current hash code value.</param>
        /// <param name="length">The <see cref="IntPtr">native integer</see> value that represents the total number of the valid bytes data to read from the given <paramref name="buffer"/>.</param>
        /// <exception cref="AccessViolationException">Thrown if the <paramref name="buffer"/> is represnets the invalid memory address, or point to the inaccessible memory location, or when the specified <paramref name="length"/> of bytes to read is out of the <paramref name="buffer"/> allocated capacity.</exception>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddBytes(void* buffer, nint length)
        {
            if (buffer is null || length < 1) return;
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var cb = length;
            var ps = (Byte*)buffer;
            while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the contents that pointed in the specified memory <paramref name="buffer"/> for specified <paramref name="length"/> of bytes.
        /// </summary>
        /// <param name="buffer">The pointer address of the valid region (block) in the memory to read and combine with the current hash code value.</param>
        /// <param name="length">Specify the <see cref="Int32"/> number that represents the total number of the valid bytes data to read from the given <paramref name="buffer"/>.</param>
        /// <exception cref="AccessViolationException">Thrown if the <paramref name="buffer"/> is represnets the invalid memory address, or point to the inaccessible memory location, or when the specified <paramref name="length"/> of bytes to read is out of the <paramref name="buffer"/> allocated capacity.</exception>
#if CLS
        [CLSCompliant(false)]
#endif
        public void AddBytes(void* buffer, Int32 length)
        {
            if (buffer is null || length < 1) return;
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var cb = length;
            var ps = (Byte*)buffer;
            while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the contents that pointed in the specified memory <paramref name="buffer"/> for specified <paramref name="length"/> of bytes.
        /// </summary>
        /// <param name="buffer">The pointer address of the valid region (block) in the memory to read and combine with the current hash code value.</param>
        /// <param name="length">The <see cref="IntPtr">native integer</see> value that represents the total number of the valid bytes data to read from the given <paramref name="buffer"/>.</param>
        /// <exception cref="AccessViolationException">Thrown if the <paramref name="buffer"/> is represnets the invalid memory address, or point to the inaccessible memory location, or when the specified <paramref name="length"/> of bytes to read is out of the <paramref name="buffer"/> allocated capacity.</exception>
        public void AddBytes(nint buffer, nint length) => this.AddBytes((void*)buffer, length);

        /// <summary>
        /// Combine the current hash code with the contents that pointed in the specified memory <paramref name="buffer"/> for specified <paramref name="length"/> of bytes.
        /// </summary>
        /// <param name="buffer">The pointer address of the valid region (block) in the memory to read and combine with the current hash code value.</param>
        /// <param name="length">Specify the <see cref="Int32"/> number that represents the total number of the valid bytes data to read from the given <paramref name="buffer"/>.</param>
        /// <exception cref="AccessViolationException">Thrown if the <paramref name="buffer"/> is represnets the invalid memory address, or point to the inaccessible memory location, or when the specified <paramref name="length"/> of bytes to read is out of the <paramref name="buffer"/> allocated capacity.</exception>
        public void AddBytes(nint buffer, Int32 length) => this.AddBytes((void*)buffer, length);

        /// <summary>
        /// Combine the current hash code with the contents that contained in the specified <paramref name="bytes"/> array.
        /// </summary>
        /// <param name="bytes">The <see cref="Array"/> of <see cref="Byte"/> that containing source data to combine with the current hash code.</param>
        public void AddBytes(Byte[] bytes)
        {
            Int64 length;
            try { length = bytes is null ? 0 : bytes.Length; }
            catch { length = bytes.LongLength; }
            if (length != 0)
            {
                var hv = this.hashvalue;
                if (hv == 0) hv = FnvBasis;
                fixed (Byte* pointer = bytes)
                {
                    var cb = (nint)length;
                    var ps = pointer;
                    while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
                }
                this.hashvalue = hv;
            }
        }

        /// <summary>
        /// Combine the current hash code with the contents that contained in the specified <paramref name="bytes"/> array.
        /// </summary>
        /// <param name="bytes">The <see cref="Array"/> of <see cref="Byte"/> that containing source data to combine with the current hash code.</param>
        /// <param name="start">The starting index of the <see cref="Byte"/> element in the given array to begin readed and combine.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="bytes"/> is <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="start"/> position is out of range.</exception>
        public void AddBytes(Byte[] bytes, Int32 start)
        {
            if (bytes is null) throw new ArgumentNullException(nameof(bytes), "The bytes array to combine the hash code must not be null reference.");
            var length = bytes.Length;
            if ((start < 0 && (start = length + start) < 0) || start >= length) throw new ArgumentOutOfRangeException(nameof(start), start, "The starting index of element in the specified array is out of range.");
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            fixed (Byte* pointer = &bytes[start])
            {
                var cb = length - start;
                var ps = pointer;
                while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            }
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the contents that contained in the specified <paramref name="bytes"/> array.
        /// </summary>
        /// <param name="bytes">The <see cref="Array"/> of <see cref="Byte"/> that containing source data to combine with the current hash code.</param>
        /// <param name="start">The starting index of the <see cref="Byte"/> element in the given array to begin readed and combine.</param>
        /// <param name="count">The requested number of <see cref="Byte"/> elements in the array to read and combine the hash.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="bytes"/> is <see langword="null"/> reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="start"/> or <paramref name="count"/> arguments is out of range.</exception>
        public void AddBytes(Byte[] bytes, Int32 start, Int32 count)
        {
            if (count == 0) return;
            if (bytes is null) throw new ArgumentNullException(nameof(bytes), "The bytes array to combine the hash code must not be null reference.");
            var length = bytes.Length;
            if (start < 0 && (start = length + start) < 0) throw new ArgumentOutOfRangeException(nameof(start), start, "The starting index of element in the specified array is out of range.");
            if (count < 0 && (count = length + count) < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "The count of the elements in the specified array to access is invalid.");
            if (start + count > length) throw new ArgumentOutOfRangeException($"{nameof(start)}, {nameof(count)}", $"{start}, {count}", "The requested range of the array elements to access is invalid.");
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            fixed (Byte* pointer = &bytes[start])
            {
                var cb = count;
                var ps = pointer;
                while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            }
            this.hashvalue = hv;
        }

        /// <summary>
        /// Combine the current hash code with the contents that contained in the specified <paramref name="bytes"/> segment.
        /// </summary>
        /// <param name="bytes">The <see cref="ArraySegment{T}"/> of <see cref="Byte"/> as the segmented data to combine with the current hash.</param>
        public void AddBytes(ArraySegment<Byte> bytes)
        {
            var count = bytes.Count;
            if (count != 0)
            {
                var hv = this.hashvalue;
                if (hv == 0) hv = FnvBasis;
                fixed (Byte* pointer = &bytes.Array[bytes.Offset])
                {
                    var ps = pointer;
                    while (count-- != 0) hv = (hv ^ *ps++) * FnvPrime;
                }
                this.hashvalue = hv;
            }
        }

        /// <summary>
        /// Combine the current hash code with the raw contents of the elements of specified <paramref name="array"/>.<br/>
        /// The element type of the <paramref name="array"/> must be a blittable type or the error will thrown in runtime.
        /// </summary>
        /// <param name="array">The <see cref="Array"/> of a blittable element types to pin and then access the raw bytes data.<br/>
        /// - After pinned, this method will combine the raw data with the current hash code.<br/>
        /// - Soon after completed, this method will release the pin and free the pin handle.</param>
        /// <exception cref="ArgumentException">Thrown if the type of the <paramref name="array"/> element is not a blittable type.</exception>
        public void AddBytes(Array array)
        {
            Int64 length;
            try { length = array is null ? 0 : array.Length; }
            catch { length = array.LongLength; }
            if (length == 0) return;
            var width = Marshal.SizeOf(array.GetType().GetElementType());
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            try
            {
                var hv = this.hashvalue;
                if (hv == 0) hv = FnvBasis;
                var cb = (nint)length * width;
                var ps = (Byte*)handle.AddrOfPinnedObject();
                while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
                this.hashvalue = hv;
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the specified bytes <paramref name="array"/>.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <param name="array">The <see cref="Array"/> of <see cref="Byte"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="start">The starting index of the <see cref="Byte"/> element in the array to access. Cannot negative or out of range.</param>
        /// <param name="count">The count of <see cref="Byte"/> elements in the array to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">Usualy caused by the range of elements is out the <paramref name="array"/> array range, this is often indicating the critical error.</exception>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw(Byte[] array, Int32 start, Int32 count)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            fixed (Byte* pointer = &array[start])
            {
                var cb = count;
                var ps = pointer;
                while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            }
            this.hashvalue = hv;
        }

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the specified <typeparamref name="T"/> <paramref name="array"/>.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the element that contained in the specified managed array to access byte-per-byte.</typeparam>
        /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="start">The starting index of the <typeparamref name="T"/> element in the array to access. Cannot negative or out of range.</param>
        /// <param name="count">The count of <typeparamref name="T"/> elements in the array to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">Usualy caused by the range of elements is out the <paramref name="array"/> array range, this is often indicating the critical error.</exception>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw<T>(T[] array, Int32 start, Int32 count) where T : unmanaged
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            fixed (T* pointer = &array[start])
            {
                var cb = (nint)count * sizeof(T);
                var ps = (Byte*)pointer;
                while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            }
            this.hashvalue = hv;
        }

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the given <paramref name="block"/> memory.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <param name="block">The memory of <see cref="Byte"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="count">The count of <see cref="Byte"/> elements in the memory to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">When the <paramref name="block"/> is represents an invalid memory address, or point to the protected memory location, or the <paramref name="count"/> is not valid (ie. negative, or too large than the allocated capacity of the <paramref name="block"/> memory).</exception>
        /// <filterpriority>2</filterpriority>
#if CLS
        [CLSCompliant(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw(Byte* block, Int32 count)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var cb = count;
            var ps = block;
            while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the given <paramref name="block"/> memory.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <param name="block">The memory of <see cref="Byte"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="count">The count of <see cref="Byte"/> elements in the memory to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">When the <paramref name="block"/> is represents an invalid memory address, or point to the protected memory location, or the <paramref name="count"/> is not valid (ie. negative, or too large than the allocated capacity of the <paramref name="block"/> memory).</exception>
        /// <filterpriority>2</filterpriority>
#if CLS
        [CLSCompliant(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw(Byte* block, nint count)
        {
            var hv = this.hashvalue;
            if (hv == 0) hv = FnvBasis;
            var cb = count;
            var ps = block;
            while (cb-- != 0) hv = (hv ^ *ps++) * FnvPrime;
            this.hashvalue = hv;
        }

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the given <typeparamref name="T"/> memory.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the element that contained in the specified memory <paramref name="block"/> to access byte-per-byte.</typeparam>
        /// <param name="block">The memory of <typeparamref name="T"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="count">The count of <typeparamref name="T"/> elements in the memory to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">When the <paramref name="block"/> is represents an invalid memory address, or point to the protected memory location, or the <paramref name="count"/> is not valid (ie. negative, or too large than the allocated capacity of the <paramref name="block"/> memory).</exception>
        /// <filterpriority>2</filterpriority>
#if CLS
        [CLSCompliant(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw<T>(T* block, Int32 count) where T : unmanaged => this.AddRaw((Byte*)block, (nint)count * sizeof(T));

        /// <summary>
        /// Unsafely combine the current hash code with the contents that contained in the given <typeparamref name="T"/> memory.<br/>
        /// This method is expect all of the parameters are valid, because no arguments validation are performed!
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the element that contained in the specified memory <paramref name="block"/> to access byte-per-byte.</typeparam>
        /// <param name="block">The memory of <typeparamref name="T"/> that containing source data to combine with the current hash. Cannot be <see langword="null"/>.</param>
        /// <param name="count">The count of <typeparamref name="T"/> elements in the memory to read and combine. Cannot zero, negative, or invalid.</param>
        /// <exception cref="AccessViolationException">When the <paramref name="block"/> is represents an invalid memory address, or point to the protected memory location, or the <paramref name="count"/> is not valid (ie. negative, or too large than the allocated capacity of the <paramref name="block"/> memory).</exception>
        /// <filterpriority>2</filterpriority>
#if CLS
        [CLSCompliant(false)]
#endif
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddRaw<T>(T* block, nint count) where T : unmanaged => this.AddRaw((Byte*)block, count * sizeof(T));

        #endregion

        #region >>> AddHelp methods

        /// <summary>
        /// Infrastructure, intenral only. Purposed to log the member search (lookup) hash.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of where the member search is occured.</param>
        /// <param name="name">The requested name of the member to search.</param>
        /// <param name="flags">The <see cref="BindingFlags"/> set as the behaviors of the search.</param>
        /// <param name="args">The set of the method parameter types to match the overloaded methods.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Int32 MixHelp(Type type, String name, BindingFlags flags, Type[] args)
        {
            unchecked
            {
                var hv = FnvBasis;
                var hash = RuntimeHelpers.GetHashCode(type);
                var ps = (Byte*)&hash;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                hash = (flags & BindingFlags.IgnoreCase) != BindingFlags.Default ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(name) : name.GetHashCode();
                ps = (Byte*)&hash;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                hash = (Int32)flags;
                ps = (Byte*)&hash;
                hv = (hv ^ ps[0]) * FnvPrime;
                hv = (hv ^ ps[1]) * FnvPrime;
                hv = (hv ^ ps[2]) * FnvPrime;
                hv = (hv ^ ps[3]) * FnvPrime;
                if (args is not null)
                {
                    for (var i = 0; i < args.Length; i++)
                    {
                        hash = RuntimeHelpers.GetHashCode(args[i]);
                        ps = (Byte*)&hash;
                        hv = (hv ^ ps[0]) * FnvPrime;
                        hv = (hv ^ ps[1]) * FnvPrime;
                        hv = (hv ^ ps[2]) * FnvPrime;
                        hv = (hv ^ ps[3]) * FnvPrime;
                    }
                }
                return (Int32)hv;
            }
        }

        #endregion

        #region >>> Clear method

        /// <summary>
        /// Reset the current <see cref="HashCode"/> back to its initial state.
        /// </summary>
        public void Clear() => this.hashvalue = 0;

        #endregion

        #region >>> FinalHash methods

        /// <summary>
        /// Finalize the hash combination and get the final hash code. The <see cref="HashCode"/> structure will reseted to the initial state.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value that represents the final 32-bit hash code value that combined in the current <see cref="HashCode"/> instance.</returns>
        public Int32 FinalHash()
        {
            var value = this.hashvalue;
            this.hashvalue = 0;
            return unchecked((Int32)value);
        }

        #endregion

        #region >>> HashValue property

        /// <summary>
        /// Gets the current raw hash value that managed by the current <see cref="HashCode"/> instance.
        /// </summary>
        /// <value>The <see cref="UInt32"/> number that represents the raw hash value in the current <see cref="HashCode"/> instance.</value>
        /// <filterpriority>2</filterpriority>
#if CLS
        [CLSCompliant(false)]
#endif
        public readonly UInt32 HashValue => this.hashvalue;

        /// <summary>
        /// Gets the 4-bytes raw hash value that managed by the current <see cref="HashCode"/> instance.
        /// </summary>
        /// <value>The <see cref="Byte"/>[] that contianing the raw hash value in the current <see cref="HashCode"/> instance.</value>
        /// <filterpriority>2</filterpriority>
        public readonly Byte[] HashData
        {
            get
            {
                var bytes = new Byte[sizeof(Int32)];
                fixed (Byte* pointer = bytes) *(UInt32*)pointer = this.hashvalue;
                return bytes;
            }
        }

        #endregion

        #region >>> object methods

        /// <summary>
        /// Determines whether the current <see cref="HashCode"/> value is equals with the specified <see cref="Object"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> instance that should compared with the current <see cref="HashCode"/> instance.</param>
        /// <returns><see langword="true"/> if the current <see cref="HashCode"/> instance is equal with the specified <paramref name="obj"/>; otherwise, <see langword="false"/>.</returns>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override Boolean Equals(Object obj) => obj is HashCode other && this.hashvalue == other.hashvalue;

        /// <summary>
        /// Calculate the hash code for the current <see cref="HashCode"/> instance.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value that repesents hash code of the current <see cref="HashCode"/>.</returns>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override Int32 GetHashCode()
        {
            var hash = InternalCombine(FnvBasis, RuntimeHelpers.GetHashCode(typeof(HashCode)));
            return unchecked((Int32)InternalCombine(hash, this.hashvalue));
        }

        /// <summary>
        /// Returns the name of the current <see cref="HashCode"/> structure.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the name of the <see cref="HashCode"/> type.</returns>
        /// <filterpriority>2</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override String ToString() => nameof(HashCode);

        #endregion

        #region >>> Combine methods

        /// <summary>
        /// Combine the hash code from the supplied one <typeparamref name="T"/> object into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value) => typeof(T).IsValueType ? MixHash(value.GetHashCode()) : MixHash(value is null ? 0 : value.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied two <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied three <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied four <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value4">Specify the 4th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3, T value4) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode(), value4.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode(), value4 is null ? 0 : value4.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied five <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value4">Specify the 4th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value5">Specify the 5th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3, T value4, T value5) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode(), value4.GetHashCode(), value5.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode(), value4 is null ? 0 : value4.GetHashCode(),
                      value5 is null ? 0 : value5.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied six <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value4">Specify the 4th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value5">Specify the 5th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value6">Specify the 6th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3, T value4, T value5, T value6) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode(), value4.GetHashCode(),
                      value5.GetHashCode(), value6.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode(), value4 is null ? 0 : value4.GetHashCode(),
                      value5 is null ? 0 : value5.GetHashCode(), value6 is null ? 0 : value6.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied seven <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value4">Specify the 4th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value5">Specify the 5th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value6">Specify the 6th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value7">Specify the 7th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3, T value4, T value5, T value6, T value7) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode(), value4.GetHashCode(),
                      value5.GetHashCode(), value6.GetHashCode(), value7.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode(), value4 is null ? 0 : value4.GetHashCode(),
                      value5 is null ? 0 : value5.GetHashCode(), value6 is null ? 0 : value6.GetHashCode(),
                      value7 is null ? 0 : value7.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied eight <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="value1">Specify the 1st <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value2">Specify the 2nd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value3">Specify the 3rd <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value4">Specify the 4th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value5">Specify the 5th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value6">Specify the 6th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value7">Specify the 7th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="value8">Specify the 8th <typeparamref name="T"/> object to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(T value1, T value2, T value3, T value4, T value5, T value6, T value7, T value8) => typeof(T).IsValueType
            ? MixHash(value1.GetHashCode(), value2.GetHashCode(), value3.GetHashCode(), value4.GetHashCode(),
                      value5.GetHashCode(), value6.GetHashCode(), value7.GetHashCode(), value8.GetHashCode())
            : MixHash(value1 is null ? 0 : value1.GetHashCode(), value2 is null ? 0 : value2.GetHashCode(),
                      value3 is null ? 0 : value3.GetHashCode(), value4 is null ? 0 : value4.GetHashCode(),
                      value5 is null ? 0 : value5.GetHashCode(), value6 is null ? 0 : value6.GetHashCode(),
                      value7 is null ? 0 : value7.GetHashCode(), value8 is null ? 0 : value8.GetHashCode());

        /// <summary>
        /// Combine the hash code from the supplied arbirarily <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="values">Specify the one or more <typeparamref name="T"/> objects to calculate each of hash codes and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        public static Int32 Combine<T>(params T[] values)
        {
            unchecked
            {
                var hv = FnvBasis;
                if (values is not null)
                {
                    Int32 i, j;
                    Byte* k;
                    if (typeof(T).IsValueType)
                    {
                        for (i = 0; i < values.Length; i++)
                        {
                            j = values[i].GetHashCode();
                            k = (Byte*)&j;
                            hv = (hv ^ k[0]) * FnvPrime;
                            hv = (hv ^ k[1]) * FnvPrime;
                            hv = (hv ^ k[2]) * FnvPrime;
                            hv = (hv ^ k[3]) * FnvPrime;
                        }
                    }
                    else
                    {
                        T v;
                        for (i = 0; i < values.Length; i++)
                        {
                            v = values[i];
                            j = v is null ? 0 : v.GetHashCode();
                            k = (Byte*)&j;
                            hv = (hv ^ k[0]) * FnvPrime;
                            hv = (hv ^ k[1]) * FnvPrime;
                            hv = (hv ^ k[2]) * FnvPrime;
                            hv = (hv ^ k[3]) * FnvPrime;
                        }
                    }
                }
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine the hash code from the supplied arbirarily <typeparamref name="T"/> objects into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T">Specify the <see cref="Type"/> of the objects to compute each of hash codes and combine into the single hash code value.</typeparam>
        /// <param name="sequences">Specify the sequences of <typeparamref name="T"/> objects to calculate the hash codes and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine<T>(IEnumerable<T> sequences) => MixHash(sequences is null ? null : new EnumerableHashes<T>(sequences));

        /// <summary>
        /// Combine the hash code from the given one <see cref="Object"/> instance into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj) => MixHash(obj is null ? 0 : obj.GetHashCode());

        /// <summary>
        /// Combine the hash code from the given two <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given three <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given four <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj4">Specify the 4th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3, Object obj4) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode(),
            obj4 is null ? 0 : obj4.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given five <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj4">Specify the 4th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj5">Specify the 5th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3, Object obj4, Object obj5) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode(),
            obj4 is null ? 0 : obj4.GetHashCode(),
            obj5 is null ? 0 : obj5.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given six <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj4">Specify the 4th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj5">Specify the 5th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj6">Specify the 6th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3, Object obj4, Object obj5, Object obj6) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode(),
            obj4 is null ? 0 : obj4.GetHashCode(),
            obj5 is null ? 0 : obj5.GetHashCode(),
            obj6 is null ? 0 : obj6.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given seven <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj4">Specify the 4th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj5">Specify the 5th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj6">Specify the 6th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj7">Specify the 7th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3, Object obj4, Object obj5, Object obj6, Object obj7) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode(),
            obj4 is null ? 0 : obj4.GetHashCode(),
            obj5 is null ? 0 : obj5.GetHashCode(),
            obj6 is null ? 0 : obj6.GetHashCode(),
            obj7 is null ? 0 : obj7.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given eight <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="obj1">Specify the 1st <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj2">Specify the 2nd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj3">Specify the 3rd <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj4">Specify the 4th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj5">Specify the 5th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj6">Specify the 6th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj7">Specify the 7th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <param name="obj8">Specify the 8th <see cref="Object"/> instance to get the implementation hash code and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Combine(Object obj1, Object obj2, Object obj3, Object obj4, Object obj5, Object obj6, Object obj7, Object obj8) => MixHash
        (
            obj1 is null ? 0 : obj1.GetHashCode(),
            obj2 is null ? 0 : obj2.GetHashCode(),
            obj3 is null ? 0 : obj3.GetHashCode(),
            obj4 is null ? 0 : obj4.GetHashCode(),
            obj5 is null ? 0 : obj5.GetHashCode(),
            obj6 is null ? 0 : obj6.GetHashCode(),
            obj7 is null ? 0 : obj7.GetHashCode(),
            obj8 is null ? 0 : obj8.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code from the given arbirarily <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="objects">Specify the one or more object instances to calculate each of hash codes and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        public static Int32 Combine(params Object[] objects)
        {
            unchecked
            {
                var hv = FnvBasis;
                if (objects is not null)
                {
                    Int32 i, j;
                    Byte* k;
                    Object v;
                    for (i = 0; i < objects.Length; i++)
                    {
                        v = objects[i];
                        j = v is null ? 0 : v.GetHashCode();
                        k = (Byte*)&j;
                        hv = (hv ^ k[0]) * FnvPrime;
                        hv = (hv ^ k[1]) * FnvPrime;
                        hv = (hv ^ k[2]) * FnvPrime;
                        hv = (hv ^ k[3]) * FnvPrime;
                    }
                }
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine the hash code from the given arbirarily <see cref="Object"/> instances into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <param name="sources">The sequences of the object instances to calculate each of hash codes and combine with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value as the final hash code value that generated using the FNV-1a algorithm from each of input hashes.</returns>
        public static Int32 Combine(IEnumerable sources)
        {
            unchecked
            {
                var hv = FnvBasis;
                if (sources is not null)
                {
                    var iterator = sources.GetEnumerator();
                    Int32 j;
                    Byte* k;
                    Object v;
                    while (iterator.MoveNext())
                    {
                        v = iterator.Current;
                        j = v is null ? 0 : v.GetHashCode();
                        k = (Byte*)&j;
                        hv = (hv ^ k[0]) * FnvPrime;
                        hv = (hv ^ k[1]) * FnvPrime;
                        hv = (hv ^ k[2]) * FnvPrime;
                        hv = (hv ^ k[3]) * FnvPrime;
                    }
                    (iterator as IDisposable)?.Dispose();
                }
                return (Int32)hv;
            }
        }

        /// <summary>
        /// Combine the hash code values from the given two different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2>(T1 arg1, T2 arg2) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given three different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given four different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given five different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given six different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given seven different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given eight different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => MixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given nine different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given ten different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given eleven different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given twelve different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T12">Specify the <see cref="Type"/> of the 12th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg12">The 12th argument (type <typeparamref name="T12"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode(),
            arg12 is null ? 0 : arg12.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given thirteen different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T12">Specify the <see cref="Type"/> of the 12th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T13">Specify the <see cref="Type"/> of the 13th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg12">The 12th argument (type <typeparamref name="T12"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg13">The 13th argument (type <typeparamref name="T13"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode(),
            arg12 is null ? 0 : arg12.GetHashCode(),
            arg13 is null ? 0 : arg13.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given fourteen different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T12">Specify the <see cref="Type"/> of the 12th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T13">Specify the <see cref="Type"/> of the 13th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T14">Specify the <see cref="Type"/> of the 14th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg12">The 12th argument (type <typeparamref name="T12"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg13">The 13th argument (type <typeparamref name="T13"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg14">The 14th argument (type <typeparamref name="T14"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode(),
            arg12 is null ? 0 : arg12.GetHashCode(),
            arg13 is null ? 0 : arg13.GetHashCode(),
            arg14 is null ? 0 : arg14.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given fifteen different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T12">Specify the <see cref="Type"/> of the 12th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T13">Specify the <see cref="Type"/> of the 13th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T14">Specify the <see cref="Type"/> of the 14th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T15">Specify the <see cref="Type"/> of the 15th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg12">The 12th argument (type <typeparamref name="T12"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg13">The 13th argument (type <typeparamref name="T13"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg14">The 14th argument (type <typeparamref name="T14"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg15">The 15th argument (type <typeparamref name="T15"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode(),
            arg12 is null ? 0 : arg12.GetHashCode(),
            arg13 is null ? 0 : arg13.GetHashCode(),
            arg14 is null ? 0 : arg14.GetHashCode(),
            arg15 is null ? 0 : arg15.GetHashCode()
        );

        /// <summary>
        /// Combine the hash code values from the given sixteen different object types into a single hash code value using FNV-1a algorithm.
        /// </summary>
        /// <typeparam name="T1">Specify the <see cref="Type"/> of the 1st generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T2">Specify the <see cref="Type"/> of the 2nd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T3">Specify the <see cref="Type"/> of the 3rd generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T4">Specify the <see cref="Type"/> of the 4th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T5">Specify the <see cref="Type"/> of the 5th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T6">Specify the <see cref="Type"/> of the 6th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T7">Specify the <see cref="Type"/> of the 7th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T8">Specify the <see cref="Type"/> of the 8th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T9">Specify the <see cref="Type"/> of the 9th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T10">Specify the <see cref="Type"/> of the 10th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T11">Specify the <see cref="Type"/> of the 11th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T12">Specify the <see cref="Type"/> of the 12th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T13">Specify the <see cref="Type"/> of the 13th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T14">Specify the <see cref="Type"/> of the 14th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T15">Specify the <see cref="Type"/> of the 15th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <typeparam name="T16">Specify the <see cref="Type"/> of the 16th generic object to calculate the implemented hash code value and combine with the each others.</typeparam>
        /// <param name="arg1">The 1st argument (type <typeparamref name="T1"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg2">The 2nd argument (type <typeparamref name="T2"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg3">The 3rd argument (type <typeparamref name="T3"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg4">The 4th argument (type <typeparamref name="T4"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg5">The 5th argument (type <typeparamref name="T5"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg6">The 6th argument (type <typeparamref name="T6"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg7">The 7th argument (type <typeparamref name="T7"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg8">The 8th argument (type <typeparamref name="T8"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg9">The 9th argument (type <typeparamref name="T9"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg10">The 10th argument (type <typeparamref name="T10"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg11">The 11th argument (type <typeparamref name="T11"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg12">The 12th argument (type <typeparamref name="T12"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg13">The 13th argument (type <typeparamref name="T13"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg14">The 14th argument (type <typeparamref name="T14"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg15">The 15th argument (type <typeparamref name="T15"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <param name="arg16">The 16th argument (type <typeparamref name="T16"/>) value to call the implemented hash code function and combine that hash with the other hashes.</param>
        /// <returns>The <see cref="Int32"/> value that represents the final hash code value from supplied argument hashes combined using the FNV-1a hash algorithm.</returns>
        public static Int32 Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) => FastMixHash
        (
            arg1 is null ? 0 : arg1.GetHashCode(),
            arg2 is null ? 0 : arg2.GetHashCode(),
            arg3 is null ? 0 : arg3.GetHashCode(),
            arg4 is null ? 0 : arg4.GetHashCode(),
            arg5 is null ? 0 : arg5.GetHashCode(),
            arg6 is null ? 0 : arg6.GetHashCode(),
            arg7 is null ? 0 : arg7.GetHashCode(),
            arg8 is null ? 0 : arg8.GetHashCode(),
            arg9 is null ? 0 : arg9.GetHashCode(),
            arg10 is null ? 0 : arg10.GetHashCode(),
            arg11 is null ? 0 : arg11.GetHashCode(),
            arg12 is null ? 0 : arg12.GetHashCode(),
            arg13 is null ? 0 : arg13.GetHashCode(),
            arg14 is null ? 0 : arg14.GetHashCode(),
            arg15 is null ? 0 : arg15.GetHashCode(),
            arg16 is null ? 0 : arg16.GetHashCode()
        );

        #endregion
    }
}
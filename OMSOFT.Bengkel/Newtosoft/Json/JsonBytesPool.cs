#if JSON
namespace Newtonsoft.Json
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the simple static bytes array pooling service. This class is not inheritable.
    /// </summary>
    [Serializable]
    public sealed partial class JsonBytesPool : IArrayPool<Byte>
    {
        private sealed partial class PoolEntry
        {
            private static readonly Int32 PageSize = Environment.SystemPageSize;

            public PoolEntry(Int32 length, Boolean locked)
            {
                if (length < PageSize)
                {
                    length = PageSize;
                }
                else
                {
                    --length;
                    length |= length >> 0x1;
                    length |= length >> 0x2;
                    length |= length >> 0x4;
                    length |= length >> 0x8;
                    length |= length >> 0x10;
                    length++;
                }
                this.Buffer = new Byte[length];
                this.Length = length;
                this.HashID = RuntimeHelpers.GetHashCode(this.Buffer);
                this.Locked = locked;
            }

            public PoolEntry(Byte[] buffer, Boolean locked)
            {
                this.Buffer = buffer;
                this.HashID = RuntimeHelpers.GetHashCode(buffer);
                this.Length = buffer.Length;
                this.Locked = locked;
            }

            public readonly Int32 HashID;
            public readonly Byte[] Buffer;
            public readonly Int32 Length;
            public volatile Boolean Locked;
        }

        [Serializable]
        private sealed partial class KeyComparer : IEqualityComparer<Int32>
        {
            public static readonly KeyComparer Default = new();
            private KeyComparer() { }
            public Boolean Equals(Int32 x, Int32 y) => x == y;
            public Int32 GetHashCode(Int32 obj) => obj;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly List<PoolEntry> Collection;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<Int32, Int32> Mappings;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Object SyncRoot;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly JsonBytesPool Instance_;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Predicate<PoolEntry> Releaser_;

        private static Boolean Remover(PoolEntry entry) => !entry.Locked;

        static JsonBytesPool()
        {
            Collection = new(0xff);
            Mappings = new(0xff, KeyComparer.Default);
            SyncRoot = new();
            Instance_ = new();
            Releaser_ = Remover;
            RuntimeHelpers.PrepareDelegate(Releaser_);
        }

        private JsonBytesPool() { }

        public static IArrayPool<Byte> Instance => Instance_;

        public static Byte[] Demand(Int32 length)
        {
            if (length < 1) return Array.Empty<Byte>();
            lock (SyncRoot)
            {
                PoolEntry node;
                var store = Collection;
                var count = store.Count;
                if (count != 0)
                {
                    while (--count > -1)
                    {
                        if (!(node = store[count]).Locked && node.Length >= length)
                        {
                            node.Locked = true;
                            return node.Buffer;
                        }
                    }
                }
                node = new PoolEntry(length, true);
                Mappings[node.HashID] = store.Count;
                store.Add(node);
                return node.Buffer;
            }
        }

        public static void Returns(Byte[] buffer)
        {
            if (buffer is not null)
            {
                var hash = RuntimeHelpers.GetHashCode(buffer);
                lock (SyncRoot)
                {
                    if (Mappings.TryGetValue(hash, out var index))
                    {
                        Collection[index].Locked = false;
                    }
                }
            }
        }

        public static Int32 Release()
        {
            lock (SyncRoot)
            {
                var count = Collection.RemoveAll(Releaser_);
                if (count != 0) Collection.TrimExcess();
                return count;
            }
        }

        public static Int32 PoolSize
        {
            get
            {
                lock (SyncRoot)
                {
                    return Collection.Count;
                }
            }
        }

        public static Boolean IsPooled(Byte[] buffer)
        {
            lock (SyncRoot)
            {
                return buffer is not null && Mappings.ContainsKey(RuntimeHelpers.GetHashCode(buffer));
            }
        }

        public static Boolean Register(Byte[] buffer)
        {
            lock (SyncRoot)
            {
                return Register(buffer, false);
            }
        }

        public static Boolean Register(Byte[] buffer, Boolean locked)
        {
            if (buffer is null || buffer.Length == 0)
            {
                return false;
            }
            lock (SyncRoot)
            {
                var hash = RuntimeHelpers.GetHashCode(buffer);
                if (!Mappings.ContainsKey(hash))
                {
                    var node = new PoolEntry(buffer, locked);
                    Mappings[node.HashID] = Collection.Count;
                    Collection.Add(node);
                    return true;
                }
                return false;
            }
        }

        public static Boolean Remove(Byte[] buffer)
        {
            lock (SyncRoot)
            {
                if (buffer is not null)
                {
                    var hash = RuntimeHelpers.GetHashCode(buffer);
                    var storage = Collection;
                    var mapping = Mappings;
                    if (mapping.TryGetValue(hash, out var index))
                    {
                        var node = storage[index];
                        node.Locked = false;
                        storage.RemoveAt(index);
                        mapping.Clear();
                        if (storage.Count != 0)
                        {
                            var offset = 0;
                            var iterator = storage.GetEnumerator();
                            while (iterator.MoveNext()) mapping[iterator.Current.HashID] = offset++;
                        }
                        return true;
                    }
                }
                return false;
            }
        }

        Byte[] IArrayPool<Byte>.Rent(Int32 minimumLength) => Demand(minimumLength);

        void IArrayPool<Byte>.Return(Byte[] array) => Returns(array);
    }
}
#endif
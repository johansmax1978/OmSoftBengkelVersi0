namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static partial class ArrayStore<T>
    {
        private static partial class StoreTools
        {
            public static readonly Type ElementType;
            public static readonly String ElementName;
            public static readonly Int32 ElementWidth;
            public static readonly Int32 DefaultSpaces;
            public static readonly Boolean BlittableType;

            static StoreTools()
            {
                const Int32 paging = 8192;
                var type = typeof(T);
                Int32 width;
                try { width = Marshal.SizeOf(type); BlittableType = true; }
                catch { width = IntPtr.Size; BlittableType = false; }
                var spaces = paging / width;
                DefaultSpaces = spaces;
                ElementWidth = width;
                ElementName = type.Name;
                ElementType = type;
                if (spaces % 2 != 0)
                {
                    spaces--;
                    spaces |= spaces >> 0x1;
                    spaces |= spaces >> 0x2;
                    spaces |= spaces >> 0x4;
                    spaces |= spaces >> 0x8;
                    spaces |= spaces >> 0x10;
                    DefaultSpaces = ++spaces;
                }
            }
        }

        private sealed partial class StoreNode
        {
            public readonly Int32 Identity;
            public readonly Int32 Capacity;
            public readonly T[] Instance;
            public Boolean IsLocked;
            public Int32 NRented;
            public Int32 NReturn;

            public StoreNode(Int32 length, Boolean locked)
            {
                Int32 capacity, defspace = StoreTools.DefaultSpaces;
                if (length != defspace)
                {
                    if (length < defspace)
                    {
                        capacity = defspace;
                    }
                    else
                    {
                        capacity = length - 1;
                        capacity |= capacity >> 0x1;
                        capacity |= capacity >> 0x2;
                        capacity |= capacity >> 0x4;
                        capacity |= capacity >> 0x8;
                        capacity |= capacity >> 0x10;
                        capacity++;
                    }
                }
                else
                {
                    capacity = defspace;
                }
                T[] array;
                try { array = new T[capacity]; }
                catch { array = new T[capacity = length]; }
                this.Identity = RuntimeHelpers.GetHashCode(array);
                this.Capacity = capacity;
                this.Instance = array;
                this.IsLocked = locked;
            }

            public StoreNode(Int32 identity, T[] instance, Boolean isLocked)
            {
                this.Identity = identity;
                this.Capacity = instance.Length;
                this.Instance = instance;
                this.IsLocked = isLocked;
            }

            public Boolean Demand(Int32 length, Boolean clears)
            {
                if (!this.IsLocked && length <= this.Capacity)
                {
                    if (clears)
                    {
                        Array.Clear(this.Instance, 0, length);
                    }
                    this.IsLocked = true;
                    this.NRented++;
                    return true;
                }
                return false;
            }

            public void Returns()
            {
                if (this.IsLocked)
                {
                    this.IsLocked = false;
                    this.NReturn++;
                }
            }
        }

        private sealed partial class KeyComparer : IEqualityComparer<Int32>
        {
            private KeyComparer() { }
            public Boolean Equals(Int32 x, Int32 y) => x == y;
            public Int32 GetHashCode(Int32 obj) => obj;

            public static readonly KeyComparer Default;
            public static readonly Comparison<StoreNode> CompareNode;

            static KeyComparer()
            {
                Default = new();
                CompareNode = CompareNodeImpl;
                RuntimeHelpers.PrepareDelegate(CompareNode);
            }

            private static Int32 CompareNodeImpl(StoreNode x, StoreNode y) => x is null ? 1 : y is null ? -1 : 0;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<Int32, Int32> m_Mapping;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static StoreNode[] m_Storage;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Int32 m_Register;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Object m_LockRoot;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly T[] m_NullSet;

        static ArrayStore()
        {
            const Int32 initialSpace = 64, initialCount = 8;
            m_LockRoot = new();
            var capacity = StoreTools.DefaultSpaces * 8;
            var mapping = new Dictionary<Int32, Int32>(initialSpace, KeyComparer.Default);
            var storage = new StoreNode[initialSpace];
            for (var i = 0; i < initialCount; i++)
            {
                var node = new StoreNode(capacity, false);
                storage[i] = node;
                mapping[node.Identity] = i;
            }
            m_Mapping = mapping;
            m_Storage = storage;
            m_Register = initialCount;
            m_NullSet = Array.Empty<T>();
        }

        public static T[] Demand(Int32 length) => Demand(length, false);

        public static T[] Demand(Int32 length, Boolean clears)
        {
            if (length < 1)
            {
                return m_NullSet;
            }
            lock (m_LockRoot)
            {
                StoreNode node;
                var count = m_Register;
                var array = m_Storage;
                switch (count)
                {
                    case 0:
                        var spaces = array is null ? 0 : array.Length;
                        if (count + 1 > spaces)
                        {
                            var expand = new StoreNode[spaces == 0 ? 64 : spaces * 2];
                            if (count != 0)
                            {
                                Array.Copy(array, 0, expand, 0, count);
                            }
                            array = expand;
                            m_Storage = array;
                        }
                        node = new StoreNode(length, true);
                        array[count] = node;
                        m_Mapping[node.Identity] = count;
                        m_Register = count + 1;
                        node.NRented = 1;
                        return node.Instance;
                    case 1:
                        if ((node = array[0]).Demand(length, clears)) return node.Instance;
                        goto case 0;
                    case 2:
                        if ((node = array[1]).Demand(length, clears)) return node.Instance;
                        goto case 1;
                    case 3:
                        if ((node = array[2]).Demand(length, clears)) return node.Instance;
                        goto case 2;
                    case 4:
                        if ((node = array[3]).Demand(length, clears)) return node.Instance;
                        goto case 3;
                    case 5:
                        if ((node = array[4]).Demand(length, clears)) return node.Instance;
                        goto case 4;
                    case 6:
                        if ((node = array[5]).Demand(length, clears)) return node.Instance;
                        goto case 5;
                    case 7:
                        if ((node = array[6]).Demand(length, clears)) return node.Instance;
                        goto case 6;
                    case 8:
                        if ((node = array[7]).Demand(length, clears)) return node.Instance;
                        goto case 7;
                    case 9:
                        if ((node = array[8]).Demand(length, clears)) return node.Instance;
                        goto case 8;
                    case 10:
                        if ((node = array[9]).Demand(length, clears)) return node.Instance;
                        goto case 9;
                    case 11:
                        if ((node = array[10]).Demand(length, clears)) return node.Instance;
                        goto case 10;
                    case 12:
                        if ((node = array[11]).Demand(length, clears)) return node.Instance;
                        goto case 11;
                    case 13:
                        if ((node = array[12]).Demand(length, clears)) return node.Instance;
                        goto case 12;
                    case 14:
                        if ((node = array[13]).Demand(length, clears)) return node.Instance;
                        goto case 13;
                    case 15:
                        if ((node = array[14]).Demand(length, clears)) return node.Instance;
                        goto case 14;
                    case 16:
                        if ((node = array[15]).Demand(length, clears)) return node.Instance;
                        goto case 15;
                    default:
                        while (--count > -1)
                        {
                            if ((node = array[count]).Demand(length, clears))
                            {
                                return node.Instance;
                            }
                        }
                        count = m_Register;
                        goto case 0;
                }
            }
        }

        public static void Returns(T[] array)
        {
            if (array is null || array.Length == 0)
            {
                return;
            }
            lock (m_LockRoot)
            {
                var hash = RuntimeHelpers.GetHashCode(array);
                if (m_Mapping.TryGetValue(hash, out var index))
                {
                    m_Storage[index].Returns();
                }
            }
        }

        public static Int32 Release()
        {
            lock (m_LockRoot)
            {
                var count = m_Register;
                if (count != 0)
                {
                    StoreNode node;
                    var array = m_Storage;
                    var total = 0;
                    while (--count > -1)
                    {
                        if (!(node = array[count]).IsLocked)
                        {
                            Array.Clear(node.Instance, 0, node.Capacity);
                            total++;
                            array[count] = null;
                        }
                    }
                    var mapping = m_Mapping;
                    mapping.Clear();
                    count = m_Register - total;
                    m_Register = count;
                    if (count > 0)
                    {
                        Array.Sort(array, KeyComparer.CompareNode);
                        while (--count > -1)
                        {
                            node = array[count];
                            mapping[node.Identity] = count;
                        }
                    }
                    return total;
                }
                return 0;
            }
        }

        public static Boolean Register(T[] array) => Register(array, false);

        public static Boolean Register(T[] array, Boolean locked)
        {
            if (array is not null && array.Length != 0)
            {
                lock (m_LockRoot)
                {
                    var identity = RuntimeHelpers.GetHashCode(array);
                    var mapping = m_Mapping;
                    if (!mapping.TryGetValue(identity, out var index))
                    {
                        var count = m_Register;
                        var store = m_Storage;
                        var space = store is null ? 0 : store.Length;
                        if (count + 1 > space)
                        {
                            var expand = new StoreNode[space == 0 ? 64 : space * 2];
                            if (count != 0)
                            {
                                Array.Copy(array, 0, expand, 0, count);
                            }
                            store = expand;
                            m_Storage = store;
                        }
                        var node = new StoreNode(identity, array, locked);
                        store[count] = node;
                        mapping[identity] = count;
                        m_Register = count + 1;
                    }
                    else
                    {
                        m_Storage[index].IsLocked = locked;
                    }
                    return true;
                }
            }
            return false;
        }

        public static Boolean Remove(T[] array) => Remove(array, false);

        public static Boolean Remove(T[] array, Boolean force)
        {
            if (array is not null && array.Length != 0)
            {
                lock (m_LockRoot)
                {
                    var count = m_Register;
                    if (count != 0)
                    {
                        var identity = RuntimeHelpers.GetHashCode(array);
                        var mapping = m_Mapping;
                        if (mapping.TryGetValue(identity, out var index))
                        {
                            var storage = m_Storage;
                            var node = storage[index];
                            if (force || !node.IsLocked)
                            {
                                node.IsLocked = false;
                                storage[index] = null;
                                m_Register = --count;
                                mapping.Clear();
                                if (count != 0)
                                {
                                    Array.Sort(storage, KeyComparer.CompareNode);
                                    while (--count > -1)
                                    {
                                        node = storage[count];
                                        mapping[node.Identity] = count;
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static Int64 AllocSize
        {
            get
            {
                lock (m_LockRoot)
                {
                    var count = m_Register;
                    if (count != 0)
                    {
                        Int64 total = 0;
                        var array = m_Storage;
                        while (--count > 0)
                        {
                            total += array[count].Capacity;
                        }
                        return total;
                    }
                    return count;
                }
            }
        }

        public static Int32 PoolSize
        {
            get
            {
                lock (m_LockRoot)
                {
                    return m_Register;
                }
            }
        }

        public static Int32 Rented
        {
            get
            {
                lock (m_LockRoot)
                {
                    var count = m_Register;
                    if (count != 0)
                    {
                        var array = m_Storage;
                        var total = 0;
                        while (--count > -1)
                        {
                            if (array[count].IsLocked)
                            {
                                total++;
                            }
                        }
                        return total;
                    }
                    return 0;
                }
            }
        }

        public static Viewers Iterator() => new();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed partial class Viewers : IEnumerator<T[]>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Int32 m_index;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private StoreNode m_node;

            public Viewers() => this.m_index = -1;

            public T[] Current => this.Node.Instance;

            public Boolean IsLocked => this.Node.IsLocked;

            public Int32 NRented => this.Node.NRented;

            public Int32 NReturn => this.Node.NReturn;

            public Int32 Capacity => this.Node.Capacity;

            public Int32 Identity => this.Node.Identity;

            public Int32 Position
            {
                get => this.m_index;
                set
                {
                    if (value < 0)
                    {
                        this.m_index = -1;
                    }
                    if (value != this.m_index)
                    {
                        if (value < 0)
                        {
                            this.m_index = -1;
                        }
                        else
                        {
                            lock (m_LockRoot)
                            {
                                this.m_index = Math.Min(value, m_Register);
                            }
                        }
                    }

                }
            }

            public Int32 Available
            {
                get
                {
                    lock (m_LockRoot)
                    {
                        return m_Register;
                    }
                }
            }

            public Boolean MoveNext()
            {
                lock (m_LockRoot)
                {
                    this.m_node = null;
                    if (++this.m_index >= m_Register)
                    {
                        this.m_index = m_Register;
                        return false;
                    }
                    return true;
                }
            }

            public Boolean MovePrev()
            {
                this.m_node = null;
                if (--this.m_index < 0)
                {
                    this.m_index = -1;
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                this.m_index = -1;
                this.m_node = null;
            }

            public void Dispose()
            {
                lock (m_LockRoot)
                {
                    this.m_index = m_Register;
                    this.m_node = null;
                }
            }

            private StoreNode Node
            {
                get
                {
                    var value = this.m_node;
                    if (value is null)
                    {
                        var index = this.m_index;
                        if (index < 0)
                        {
                            throw new InvalidOperationException("The enumerator is not yet started, call the \"MoveNext\" function to start the enumeration.");
                        }
                        lock (m_LockRoot)
                        {
                            var count = m_Register;
                            if (index >= count)
                            {
                                throw new InvalidOperationException("The enumerator has reaching the end of stream, no more entries can be yielded.");
                            }
                            this.m_node = value = m_Storage[index];
                        }
                    }
                    return value;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never), EditorBrowsable(EditorBrowsableState.Never)]
            Object IEnumerator.Current => this.Current;

#if FLUENT
            [EditorBrowsable(EditorBrowsableState.Never)]
#endif
            public override Boolean Equals(Object obj) => obj is Viewers other && this.m_index == other.m_index;

#if FLUENT
            [EditorBrowsable(EditorBrowsableState.Never)]
#endif
            public override Int32 GetHashCode()
            {
                var hash = RuntimeHelpers.GetHashCode(typeof(Viewers));
                hash = ((hash << 0x5) + hash) ^ this.m_index;
                return hash;
            }

#if FLUENT
            [EditorBrowsable(EditorBrowsableState.Never)]
#endif
            public override String ToString()
            {
                if (this.m_index == -1)
                {
                    return "-INITIAL-";
                }
                else
                {
                    lock (m_LockRoot)
                    {
                        if (this.m_index >= m_Register)
                        {
                            return "-COMPLETED-";
                        }
                        else
                        {
                            var node = this.Node;
                            return $"[{this.m_index}] {StoreTools.ElementName}[{node.Capacity}] {{ Identity: 0x{node.Identity:X8}, Status: {(node.IsLocked ? "Rented" : "Idle")}, Rented: {node.NRented} times, Returned: {node.NReturn} times }}";
                        }
                    }
                }
            }
        }

    }
}
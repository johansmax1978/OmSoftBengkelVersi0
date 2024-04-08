namespace System.Collections
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class ReferenceComparer : IComparer, IEqualityComparer, IComparer<Object>, IEqualityComparer<Object>
    {
        public static readonly ReferenceComparer Default = new();

        public ReferenceComparer() { }

        public new Boolean Equals(Object x, Object y) => ReferenceEquals(x, y);

        public Int32 GetHashCode(Object obj) => RuntimeHelpers.GetHashCode(obj);

        public Int32 Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : x is null ? 1 : y is null ? -1 : StringComparer.InvariantCultureIgnoreCase.Compare(x.GetType().ToString(), y.GetType().ToString());
    }
}
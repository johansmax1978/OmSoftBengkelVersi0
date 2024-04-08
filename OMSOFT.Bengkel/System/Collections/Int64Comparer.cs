namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class Int64Comparer : IComparer, IEqualityComparer, IComparer<Int64>, IEqualityComparer<Int64>
    {
        public static readonly Int64Comparer Default = new();

        public Int64Comparer() { }

        public Int32 Compare(Int64 x, Int64 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(Int64 x, Int64 y) => x == y;

        public Int32 GetHashCode(Int64 obj) => unchecked((Int32)obj ^ (Int32)(obj >> 32));

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? 0 : Convert.ToInt64(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToInt64(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? 0 : Convert.ToInt64(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToInt64(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToInt64(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is Int64Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(Int64Comparer);
    }
}
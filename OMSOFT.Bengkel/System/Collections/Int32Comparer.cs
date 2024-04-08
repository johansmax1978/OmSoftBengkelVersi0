namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class Int32Comparer : IComparer, IEqualityComparer, IComparer<Int32>, IEqualityComparer<Int32>
    {
        public static readonly Int32Comparer Default = new();

        public Int32Comparer() { }

        public Int32 Compare(Int32 x, Int32 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(Int32 x, Int32 y) => x == y;

        public Int32 GetHashCode(Int32 obj) => obj;

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? 0 : Convert.ToInt32(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToInt32(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? 0 : Convert.ToInt32(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToInt32(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToInt32(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is Int32Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(Int32Comparer);
    }
}
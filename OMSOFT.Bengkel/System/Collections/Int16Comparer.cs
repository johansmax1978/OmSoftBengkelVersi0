namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class Int16Comparer : IComparer, IEqualityComparer, IComparer<Int16>, IEqualityComparer<Int16>
    {
        public static readonly Int16Comparer Default = new();

        public Int16Comparer() { }

        public Int32 Compare(Int16 x, Int16 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(Int16 x, Int16 y) => x == y;

        public Int32 GetHashCode(Int16 obj) => obj;

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? (Int16)0 : Convert.ToInt16(x, CultureInfo.CurrentCulture), y is null ? (Int16)0 : Convert.ToInt16(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? (Int16)0 : Convert.ToInt16(x, CultureInfo.CurrentCulture), y is null ? (Int16)0 : Convert.ToInt16(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToInt16(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is Int16Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(Int16Comparer);
    }
}
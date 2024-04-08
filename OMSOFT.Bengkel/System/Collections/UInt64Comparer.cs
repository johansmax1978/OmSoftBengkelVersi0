namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
#if CLS
    [CLSCompliant(false)]
#endif
    [Serializable]
    public sealed partial class UInt64Comparer : IComparer, IEqualityComparer, IComparer<UInt64>, IEqualityComparer<UInt64>
    {
        public static readonly UInt64Comparer Default = new();

        public UInt64Comparer() { }

        public Int32 Compare(UInt64 x, UInt64 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(UInt64 x, UInt64 y) => x == y;

        public Int32 GetHashCode(UInt64 obj) => unchecked((Int32)obj ^ (Int32)(obj >> 32));

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? 0 : Convert.ToUInt64(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToUInt64(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? 0 : Convert.ToUInt64(x, CultureInfo.CurrentCulture), y is null ? 0 : Convert.ToUInt64(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToUInt64(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is UInt64Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(UInt64Comparer);
    }
}
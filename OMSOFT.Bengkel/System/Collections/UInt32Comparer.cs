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
    public sealed partial class UInt32Comparer : IComparer, IEqualityComparer, IComparer<UInt32>, IEqualityComparer<UInt32>
    {
        public static readonly UInt32Comparer Default = new();

        public UInt32Comparer() { }

        public Int32 Compare(UInt32 x, UInt32 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(UInt32 x, UInt32 y) => x == y;

        public Int32 GetHashCode(UInt32 obj) => unchecked((Int32)obj);

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? (UInt32)0 : Convert.ToUInt32(x, CultureInfo.CurrentCulture), y is null ? (UInt32)0 : Convert.ToUInt32(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? (UInt32)0 : Convert.ToUInt32(x, CultureInfo.CurrentCulture), y is null ? (UInt32)0 : Convert.ToUInt32(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToUInt32(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is UInt32Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(UInt32Comparer);
    }
}
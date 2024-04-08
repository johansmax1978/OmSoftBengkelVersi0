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
    public sealed partial class SByteComparer : IComparer, IEqualityComparer, IComparer<SByte>, IEqualityComparer<SByte>
    {
        public static readonly SByteComparer Default = new();

        public SByteComparer() { }

        public Int32 Compare(SByte x, SByte y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(SByte x, SByte y) => x == y;

        public Int32 GetHashCode(SByte obj) => obj;

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? (SByte)0 : Convert.ToSByte(x, CultureInfo.CurrentCulture), y is null ? (SByte)0 : Convert.ToSByte(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? (SByte)0 : Convert.ToSByte(x, CultureInfo.CurrentCulture), y is null ? (SByte)0 : Convert.ToSByte(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToSByte(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is SByteComparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(SByteComparer);
    }
}
namespace System.Collections
{
    using System.Collections.Generic;
#if CLS
    [CLSCompliant(false)]
#endif
    [Serializable]
    public sealed partial class UIntPtrComparer : IComparer, IEqualityComparer, IComparer<nuint>, IEqualityComparer<nuint>
    {
        private static Int32 ImplHashCode32(nuint obj) => unchecked((Int32)obj);

        private static Int32 ImplHashCode64(nuint obj) => unchecked((Int32)obj ^ (Int32)(obj >> 32));

        private static unsafe readonly delegate*<nuint, Int32> ImplHashCode;
        private static unsafe readonly delegate*<Object, nuint> ImplConvert;

        unsafe static UIntPtrComparer()
        {
            ImplHashCode = IntPtr.Size == 4 ? &ImplHashCode32 : &ImplHashCode64;
            delegate*<Object, nint> convert = &IntPtrComparer.Convert;
            ImplConvert = (delegate*<Object, nuint>)(void*)convert;
            Default = new();
        }

        public static readonly UIntPtrComparer Default;

        public Int32 Compare(nuint x, nuint y)=> x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(nuint x, nuint y) => x == y;

        public unsafe Int32 GetHashCode(nuint obj) => ImplHashCode(obj);

        unsafe Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(ImplConvert(x), ImplConvert(y));

        unsafe Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(ImplConvert(x), ImplConvert(y));

        unsafe Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(ImplConvert(obj));

    }
}
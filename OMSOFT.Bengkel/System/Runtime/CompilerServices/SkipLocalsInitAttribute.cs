#if NETFX
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Mark the decorated target to prevent the compiler emitting <see langword="init"/> in the local variable declarations (eg. <see langword="locals init"/> to <see langword="locals"/>). This class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, Inherited = false)]
    internal sealed class SkipLocalsInitAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipLocalsInitAttribute"/> class.
        /// </summary>
        public SkipLocalsInitAttribute() { }
    }
}
#endif
#if NETFX
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Used to indicate to the compiler that a method should be called in its containing module's initializer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleInitializerAttribute"/> class
        /// </summary>
        public ModuleInitializerAttribute() { }
    }
}
#endif
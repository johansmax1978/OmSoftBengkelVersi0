#if JSON
namespace Newtonsoft.Json
{
    using System;

    /// <summary>
    /// Represents the set of options that used to create a JSON serializer or serializer configurations.
    /// </summary>
    [Flags]
    public enum JsonOptions
    {
        /// <summary>
        /// The JSON indentation should not used in the current formatting. The result will be minified and wrapped in a signle line.
        /// </summary>
        Inline = 0x0,

        /// <summary>
        /// The JSON indentation is should be used in the current formatting. The result will be pretified and has multiple lines.
        /// </summary>
        Pretty = 0x1,

        /// <summary>
        /// If specified, all of element or property which have null value (for reference type) or default value (for value type) will excluded.<br/>
        /// </summary>
        Simple = 0x2,

        /// <summary>
        /// If specified, the .NET type will be serialized when serializing non primitive objects.
        /// </summary>
        Typed = 0x4,

        /// <summary>
        /// If specified, all of JSON errors that occured in serialization or deserialization will suppressed silently.
        /// </summary>
        Silent = 0x8,

        /// <summary>
        /// If specified, activate tracing for JSON serializer, the trace output will persisted in the file at folder "\{application directory}\logs\".
        /// </summary>
        Trace = 0x10
    }
}
#endif
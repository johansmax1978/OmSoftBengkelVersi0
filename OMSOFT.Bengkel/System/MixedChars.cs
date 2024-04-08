namespace System
{
    /// <summary>
    /// Represents the set of characters that used in the random password, random serial number, or random text generation.
    /// </summary>
    [Flags]
    public enum MixedChars
    {
        /// <summary>
        /// Indicating the default character parts set for generating random password, random serial number, or random text are used.
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Instruct the provider to include all of lower case characters (from 'a' to 'z') in the current random generation.
        /// </summary>
        Lowers = 0x1,

        /// <summary>
        /// Instruct the provider to include all of upper case characters (from 'A' to 'Z') in the current random generation.
        /// </summary>
        Uppers = 0x2,

        /// <summary>
        /// Instruct the provider to include all of decimal number characters (from '0' to '9') in the current random generation.
        /// </summary>
        Number = 0x4,

        /// <summary>
        /// Instruct the provider to include the common ANSI symbol characters in the current random generation, the list of symbols are:<br/>
        /// <b>~</b>, <b>@</b>, <b>#</b>, <b>$</b>, <b>%</b>, <b>^</b>, <b>&amp;</b>, <b>*</b>, <b>(</b>, <b>)</b>, <b>+</b>, <b>=</b>, <b>-</b>, <b>&lt;</b>, <b>&gt;</b>, <b>{</b>, <b>}</b>, <b>:</b>, <b>;</b>, <b>?</b>, <b>[</b>, and <b>]</b><br/>
        /// <i>----The symbols set are containing <b>22 variants</b> characters.</i>
        /// </summary>
        Symbols = 0x8,

        /// <summary>
        /// Combine all lower case ('a' to 'z') and upper case ('A' to 'Z') characters in the current random generation.
        /// </summary>
        Letters = Lowers | Uppers,

        /// <summary>
        /// Combine all of upper case characters ('A' to 'Z') and all decimal number characters ('0' to '9') in the current random generation.
        /// </summary>
        Serials = Uppers | Number,

        /// <summary>
        /// Combine all sets in the current random generation (both lower case and upper case alphabets, decimal numbers, and common symbols).
        /// </summary>
        AllParts = Letters | Number | Symbols,
    }
}
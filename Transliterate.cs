using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("TransliterateLibrary.UnitTest")]
namespace PlayfulSparkle
{
    /// <summary>
    /// Provides functionality to transliterate and normalize strings.
    /// </summary>
    public static class Transliterate
    {
        /// <summary>
        /// A dictionary mapping Unicode representations of emojis to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        internal static Dictionary<string, string> emojiUnicodeToReplacement = new Dictionary<string, string>();

        /// <summary>
        /// A dictionary mapping Unicode representations of various characters or character sequences to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        internal static Dictionary<string, string> defaultMappingsUnicodeToReplacement = new Dictionary<string, string>();

        /// <summary>
        /// Defines the different Unicode normalization forms that can be applied to a string.
        /// </summary>
        public enum Normalization
        {
            /// <summary>
            /// Represents the Unicode Normalization Form D (NFD), which decomposes composed characters into their base characters and combining characters.
            /// </summary>
            Decompose, // NFD
            /// <summary>
            /// Represents the Unicode Normalization Form C (NFC), which composes characters by combining base characters with their combining characters.
            /// </summary>
            Compose, // NFC
            /// <summary>
            /// Represents the Unicode Normalization Form KC (NFKC), which applies compatibility decomposition (mapping compatibility variants to their base forms) followed by composition.
            /// </summary>
            CompatibilityCompose, // NFKC
            /// <summary>
            /// Represents the Unicode Normalization Form KD (NFKD), which applies compatibility decomposition (mapping compatibility variants to their base forms) followed by decomposition.
            /// </summary>
            CompatibilityDecompose // NFKD
        }

        /// <summary>
        /// Initializes the <see cref="emojiUnicodeToReplacement"/> and <see cref="defaultMappingsUnicodeToReplacement"/> dictionaries
        /// by preprocessing data from the <c>Emoji.chars</c> and <c>DefaultMappings.chars</c> (not shown) respectively.
        /// This static constructor is called only once when the class is first accessed.
        /// </summary>
        static Transliterate()
        {
            emojiUnicodeToReplacement = PreprocessDictionary(Emoji.chars); // Assuming Emoji.chars is accessible

            defaultMappingsUnicodeToReplacement = PreprocessDictionary(DefaultMappings.chars); // Assuming DefaultMappings.chars is accessible
        }

        /// <summary>
        /// Transliterates and normalizes the str string based on the specified normalization form and optional custom mappings.
        /// </summary>
        /// <param name="str">The str string to be processed.</param>
        /// <param name="normalization">The desired Unicode normalization form to apply.</param>
        /// <param name="useDefaultMapping">Indicates whether to use the default character mappings.</param>
        /// <param name="customMapping">An optional dictionary containing custom character or sequence mappings to be applied before the default mappings.
        /// The keys of this dictionary should be Unicode character sequences (as strings), and the values should be their replacement strings.</param>
        /// <returns>The transliterated and normalized string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="normalization"/> value is not a valid member of the <see cref="Normalization"/> enum.</exception>
        public static string Decompose(
            string str,
            Normalization normalization,
            bool useDefaultMapping = true,
            Dictionary<string, string> customMapping = null
        )
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException(nameof(str), "Input string cannot be null or empty.");
            }

            if (!IsValidUnicodeString(str))
            {
                throw new ArgumentOutOfRangeException(nameof(str), "Input string contains invalid Unicode characters.");
            }

            NormalizationForm normalizationForm = NormalizationForm.FormD;

            switch (normalization)
            {
                case Normalization.Decompose:
                    normalizationForm = NormalizationForm.FormD;
                    break;
                case Normalization.Compose:
                    normalizationForm = NormalizationForm.FormC;
                    break;
                case Normalization.CompatibilityCompose:
                    normalizationForm = NormalizationForm.FormKC;
                    break;
                case Normalization.CompatibilityDecompose:
                    normalizationForm = NormalizationForm.FormKD;
                    break;
            }


            if (!useDefaultMapping && customMapping == null)
            {
                return Normalize(str, normalizationForm);
            }

            // Validate user-provided custom mapping before using it.
            if (!ValidateCustomMapping(customMapping))
            {
                throw new ArgumentOutOfRangeException(nameof(customMapping), "Custom mapping contains invalid entries.");
            }

            // First pass - handle both emoji sequences and complex character mappings
            StringBuilder firstPassResult = new StringBuilder();

            int idx = 0;

            while (idx < str.Length)
            {
                bool found = false;

                // Try to match the longest sequence first (up to 8 characters)
                for (int len = Math.Min(8, str.Length - idx); len > 0 && !found; len--)
                {
                    if (idx + len <= str.Length)
                    {
                        string candidateSequence = str.Substring(idx, len);

                        if (customMapping != null && customMapping.TryGetValue(candidateSequence, out string customMappingReplacement))
                        {
                            firstPassResult.Append(customMappingReplacement);

                            idx += len;

                            found = true;
                        }
                        else if (useDefaultMapping && defaultMappingsUnicodeToReplacement.TryGetValue(candidateSequence, out string mappingReplacement))
                        {
                            firstPassResult.Append(mappingReplacement);

                            idx += len;

                            found = true;
                        }
                        else if (useDefaultMapping && emojiUnicodeToReplacement.TryGetValue(candidateSequence, out string emojiReplacement))
                        {
                            firstPassResult.Append(emojiReplacement);

                            idx += len;

                            found = true;
                        }
                    }
                }

                // If no match was found, process as a single character
                if (!found)
                {
                    char chr = str[idx];

                    string charStr = chr.ToString();

                    string unicodeKey = $"U+{(int)chr:X4}";

                    // Try to find in DefaultMappings dictionary by Unicode notation
                    if (useDefaultMapping && DefaultMappings.chars.TryGetValue(unicodeKey, out string mappingReplacement)) // Assuming DefaultMappings.chars is accessible
                    {
                        firstPassResult.Append(mappingReplacement);
                    }
                    // Try to find in Emoji dictionary by Unicode notation
                    else if (useDefaultMapping && Emoji.chars.TryGetValue(unicodeKey, out string emojiReplacement)) // Assuming Emoji.chars is accessible
                    {
                        firstPassResult.Append(emojiReplacement);
                    }
                    else
                    {
                        // If no match, add the character as is
                        firstPassResult.Append(chr);
                    }

                    idx++;
                }
            }

            return Normalize(firstPassResult.ToString(), normalizationForm);
        }

        /// <summary>
        /// Asynchronously transliterates and normalizes the str string based on the specified normalization form and optional custom mappings.
        /// This method runs the decomposition process in a separate task to avoid blocking the calling thread.
        /// </summary>
        /// <param name="str">The str string to be processed.</param>
        /// <param name="normalization">The desired Unicode normalization form to apply.</param>
        /// <param name="useDefaultMapping">Indicates whether to use the default character mappings.</param>
        /// <param name="customMapping">An optional dictionary containing custom character or sequence mappings to be applied before the default mappings.
        /// The keys of this dictionary should be Unicode character sequences (as strings), and the values should be their replacement strings.</param>
        /// <returns>A <see cref="Task{String}"/> that represents the asynchronous operation, containing the transliterated and normalized string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="normalization"/> value is not a valid member of the <see cref="Normalization"/> enum.</exception>
        public static async Task<string> DecomposeAsync(
            string str,
            Normalization normalization,
            bool useDefaultMapping = true,
            Dictionary<string, string> customMapping = null
        )
        {
            return await Task.Run(() => Decompose(str, normalization, useDefaultMapping, customMapping));
        }

        /// <summary>
        /// Validates a custom mapping dictionary to ensure that keys and values adhere to specific length constraints.
        /// </summary>
        /// <param name="customMapping">The dictionary representing the custom mapping, where keys and values are strings.</param>
        /// <returns>
        /// <c>true</c> if the custom mapping is valid (or null); otherwise, <c>false</c>.
        /// A mapping is considered valid if all keys have a length of at most 6 graphemes and all values have a length of at most 40 graphemes.
        /// If the <paramref name="customMapping"/> is null, this method returns <c>true</c>.
        /// </returns>
        internal static bool ValidateCustomMapping(Dictionary<string, string> customMapping)
        {
            if (customMapping == null)
            {
                return true;
            }

            foreach (KeyValuePair<string, string> pair in customMapping)
            {
                if (!StrLength(pair.Key, 6) || !StrLength(pair.Value, 40))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the length of a string, measured in graphemes, is less than or equal to a specified maximum.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="maxGraphemes">The maximum allowed number of graphemes in the string.</param>
        /// <returns>
        /// <c>true</c> if the string is not null or whitespace and its length in graphemes is less than or equal to <paramref name="maxGraphemes"/>; otherwise, <c>false</c>.
        /// </returns>
        internal static bool StrLength(string str, int maxGraphemes)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            var enumerator = StringInfo.GetTextElementEnumerator(str);

            int count = 0;

            while (enumerator.MoveNext())
            {
                count++;

                if (count > maxGraphemes)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Normalizes the str string according to the specified Unicode normalization form and then removes any non-spacing combining marks.
        /// This process is useful for tasks like text comparison or preparing text for indexing where diacritics or other combining marks should be ignored.
        /// </summary>
        /// <param name="str">The str string to be normalized and processed.</param>
        /// <param name="normalizationForm">The Unicode normalization form to apply. Common forms include NFC, NFD, NFKC, and NFKD.</param>
        /// <returns>A new string that is normalized according to the specified form and has all non-spacing combining marks removed.</returns>
        internal static string Normalize(string str, NormalizationForm normalizationForm)
        {
            // Now apply normalization to the result
            string normalizedResult = str.Normalize(normalizationForm);

            // Remove combining marks after normalization
            StringBuilder finalResult = new StringBuilder();

            foreach (char chr in normalizedResult)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(chr) != UnicodeCategory.NonSpacingMark)
                {
                    finalResult.Append(chr);
                }
            }

            return finalResult.ToString();
        }

        /// <summary>
        /// Preprocesses a dictionary by converting Unicode notation (e.g., "U+XXXX" or "U+XXXX U+YYYY") in the keys
        /// to their corresponding actual Unicode characters.
        /// </summary>
        /// <param name="source">The str dictionary where keys are in Unicode notation.</param>
        /// <returns>A new dictionary where the keys are the actual Unicode characters represented by the notation in the str dictionary.</returns>
        internal static Dictionary<string, string> PreprocessDictionary(Dictionary<string, string> source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in source)
            {
                // Convert from "U+XXXX U+YYYY" format to actual Unicode characters
                string unicodeSequence = ConvertUnicodeNotationToChars(entry.Key);

                result[unicodeSequence] = entry.Value;
            }
            return result;
        }

        /// <summary>
        /// Converts a string containing Unicode notations (e.g., "U+1F642 U+200D U+2194 U+FE0F")
        /// to a string of the corresponding actual Unicode characters.
        /// </summary>
        /// <param name="unicodeNotation">The string containing Unicode notations, where each notation starts with "U+" followed by the hexadecimal Unicode code point,
        /// and multiple notations can be separated by spaces.</param>
        /// <returns>A string containing the Unicode characters represented by the str notation.</returns>
        internal static string ConvertUnicodeNotationToChars(string unicodeNotation)
        {
            if (string.IsNullOrEmpty(unicodeNotation))
                return string.Empty;

            var result = new StringBuilder(unicodeNotation.Length / 3); // Estimate capacity

            int startPos = 0;

            int length = unicodeNotation.Length;

            while (startPos < length)
            {
                // Find next "U+" marker
                int markerPos = unicodeNotation.IndexOf("U+", startPos, StringComparison.Ordinal);

                if (markerPos == -1)
                    break;

                // Move past "U+"
                int hexStart = markerPos + 2;

                if (hexStart >= length)
                    break;

                // Find end of hex value (space or end of string)
                int hexEnd = unicodeNotation.IndexOf(' ', hexStart);

                if (hexEnd == -1)
                    hexEnd = length;

                // Extract and parse hex value
                if (hexEnd > hexStart && hexEnd - hexStart <= 8) // Max valid Unicode is U+10FFFF (6 chars + "U+")
                {
                    string hexValue = unicodeNotation.Substring(hexStart, hexEnd - hexStart);

                    // Try to parse the hex value safely
                    if (int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int codePoint))
                    {
                        // Validate Unicode range (U+0000 to U+10FFFF)
                        if (codePoint >= 0 && codePoint <= 0x10FFFF)
                        {
                            // Convert to UTF-16 character(s)
                            result.Append(char.ConvertFromUtf32(codePoint));
                        }
                    }
                }

                // Move to next position after current code point
                startPos = hexEnd + 1;
            }

            return result.ToString();
        }

        /// <summary>
        /// Validates whether a given string contains only valid Unicode characters. 
        /// A valid Unicode character is defined as any character that is not a control character.
        /// </summary>
        /// <param name="str">The string to be validated.</param>
        /// <returns>
        ///   <c>true</c> if the string is not null or empty, and does not contain any control characters; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsValidUnicodeString(string str)
        {
            bool expectingLowSurrogate = false;

            for (int i = 0; i < str.Length; i++)
            {
                char current = str[i];

                if (expectingLowSurrogate)
                {
                    if (!char.IsLowSurrogate(current))
                    {
                        return false;
                    }

                    expectingLowSurrogate = false;
                }
                else
                {
                    if (char.IsHighSurrogate(current))
                    {
                        if (i == str.Length - 1)
                        {
                            return false; // High surrogate at end of string
                        }

                        expectingLowSurrogate = true;
                    }
                    else if (char.IsLowSurrogate(current))
                    {
                        return false; // Unexpected low surrogate
                    }
                }
            }

            return !expectingLowSurrogate; // Check for missing low surrogate at end
        }
    }
}
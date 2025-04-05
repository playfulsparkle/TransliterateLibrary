using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PlayfulSparkle
{
    /// <summary>
    /// Provides functionality to transliterate and normalize strings.
    /// </summary>
    public class Transliterate
    {
        /// <summary>
        /// A dictionary mapping Unicode representations of emojis to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        private static Dictionary<string, string> emojiUnicodeToReplacement = new Dictionary<string, string>();

        /// <summary>
        /// A dictionary mapping Unicode representations of various characters or character sequences to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        private static Dictionary<string, string> defaultMappingsUnicodeToReplacement = new Dictionary<string, string>();

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
        /// Transliterates and normalizes the input string based on the specified normalization form and optional custom mappings.
        /// </summary>
        /// <param name="str">The input string to be processed.</param>
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
        /// Asynchronously transliterates and normalizes the input string based on the specified normalization form and optional custom mappings.
        /// This method runs the decomposition process in a separate task to avoid blocking the calling thread.
        /// </summary>
        /// <param name="str">The input string to be processed.</param>
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
        /// Normalizes the input string according to the specified Unicode normalization form and then removes any non-spacing combining marks.
        /// This process is useful for tasks like text comparison or preparing text for indexing where diacritics or other combining marks should be ignored.
        /// </summary>
        /// <param name="str">The input string to be normalized and processed.</param>
        /// <param name="normalizationForm">The Unicode normalization form to apply. Common forms include NFC, NFD, NFKC, and NFKD.</param>
        /// <returns>A new string that is normalized according to the specified form and has all non-spacing combining marks removed.</returns>
        private static string Normalize(string str, NormalizationForm normalizationForm)
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
        /// <param name="source">The input dictionary where keys are in Unicode notation.</param>
        /// <returns>A new dictionary where the keys are the actual Unicode characters represented by the notation in the input dictionary.</returns>
        private static Dictionary<string, string> PreprocessDictionary(Dictionary<string, string> source)
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
        /// <returns>A string containing the Unicode characters represented by the input notation.</returns>
        private static string ConvertUnicodeNotationToChars(string unicodeNotation)
        {
            StringBuilder result = new StringBuilder();

            string[] parts = unicodeNotation.Split(' ');

            foreach (string part in parts)
            {
                if (part.StartsWith("U+"))
                {
                    string hexValue = part.Substring(2);

                    int intValue = int.Parse(hexValue, NumberStyles.HexNumber);

                    if (intValue <= 0xFFFF) // Handle values within the Basic Multilingual Plane (BMP)
                    {
                        result.Append((char)intValue);
                    }
                    else // Convert to surrogate pairs for values outside the BMP
                    {
                        result.Append(char.ConvertFromUtf32(intValue));
                    }
                }
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
        private static bool IsValidUnicodeString(string str)
        {
            // First check for isolated surrogates
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                // Check for high surrogates
                if (char.IsHighSurrogate(c))
                {
                    // High surrogate must be followed by a low surrogate
                    if (i + 1 >= str.Length || !char.IsLowSurrogate(str[i + 1]))
                        return false;

                    // Skip the low surrogate we just checked
                    i++;
                }
                // Check for isolated low surrogates
                else if (char.IsLowSurrogate(c))
                {
                    // Low surrogate without a preceding high surrogate
                    return false;
                }
            }

            // Check for other invalid Unicode code points
            // A clever approach: use UTF8Encoding with error detection
            try
            {
                byte[] encoded = Encoding.UTF8.GetBytes(str);
                string decoded = Encoding.UTF8.GetString(encoded);

                // If the round-trip changes the string, there were invalid sequences
                if (decoded.Length != str.Length)
                {
                    return false;
                }

                for (int i = 0; i < str.Length; i++)
                {
                    if (decoded[i] != str[i])
                        return false;
                }

                // Check for specific invalid code points
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];

                    // Skip surrogate pairs (already validated above)
                    if (char.IsHighSurrogate(c) && i + 1 < str.Length && char.IsLowSurrogate(str[i + 1]))
                    {
                        // Get the Unicode code point
                        int codePoint = char.ConvertToUtf32(str, i);

                        // Check for noncharacters
                        if ((codePoint >= 0xFDD0 && codePoint <= 0xFDEF) ||
                            (codePoint & 0xFFFE) == 0xFFFE ||
                            (codePoint & 0xFFFF) == 0xFFFF)
                            return false;

                        i++; // Skip the low surrogate
                    }
                    // For BMP characters, check directly
                    else if (!char.IsSurrogate(c))
                    {
                        // Check for noncharacters
                        if ((c >= 0xFDD0 && c <= 0xFDEF) ||
                            (c & 0xFFFE) == 0xFFFE)
                            return false;
                    }
                }

                return true;
            }
            catch (EncoderFallbackException)
            {
                // If encoding throws an exception, the string contains invalid Unicode
                return false;
            }
        }
    }
}
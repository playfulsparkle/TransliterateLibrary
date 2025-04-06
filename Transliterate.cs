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
        internal static Dictionary<string, string> emojiUnicodeMappings = new Dictionary<string, string>();

        /// <summary>
        /// A dictionary mapping Unicode representations of various characters or character sequences to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        internal static Dictionary<string, string> defaultUnicodeMappings = new Dictionary<string, string>();

        /// <summary>
        /// Stores the maximum length of keys in the <see cref="emojiUnicodeMappings"/> dictionary.
        /// This value is calculated during the class's static initialization to optimize lookup operations.
        /// </summary>
        internal static int emojiMappingsMaxKeyLength;

        /// <summary>
        /// Stores the maximum length of keys in the <see cref="defaultUnicodeMappings"/> dictionary.
        /// This value is calculated during the class's static initialization to optimize lookup operations.
        /// </summary>
        internal static int defaultMappingsMaxKeyLength;

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
        /// Initializes the <see cref="emojiUnicodeMappings"/> and <see cref="defaultUnicodeMappings"/> dictionaries
        /// by preprocessing data from the <c>Emoji.chars</c> and <c>DefaultMappings.chars</c> (not shown) respectively.
        /// This static constructor is called only once when the class is first accessed.
        /// </summary>
        static Transliterate()
        {
            emojiUnicodeMappings = PrepareDictionary(Emoji.chars);

            defaultUnicodeMappings = PrepareDictionary(DefaultMappings.chars);

            emojiMappingsMaxKeyLength = GetMaxKeyLength(emojiUnicodeMappings);

            defaultMappingsMaxKeyLength = GetMaxKeyLength(defaultUnicodeMappings);
        }

        /// <summary>
        /// Transliterates and normalizes the text string based on the specified normalization form and optional custom mappings.
        /// </summary>
        /// <param name="text">The text string to be processed.</param>
        /// <param name="normalization">The desired Unicode normalization form to apply.</param>
        /// <param name="useDefaultMapping">Indicates whether to use the default character mappings.</param>
        /// <param name="customMapping">An optional dictionary containing custom character or sequence mappings to be applied before the default mappings.
        /// The keys of this dictionary should be Unicode character sequences (as strings), and the values should be their replacement strings.</param>
        /// <returns>The transliterated and normalized string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="normalization"/> value is not a valid member of the <see cref="Normalization"/> enum.</exception>
        public static string Decompose(
            string text,
            Normalization normalization,
            bool useDefaultMapping = true,
            Dictionary<string, string> customMapping = null
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(nameof(text), "Input string cannot be null or empty.");
            }

            if (!ValidateUnicodeString(text))
            {
                throw new ArgumentOutOfRangeException(nameof(text), "Input string contains invalid Unicode characters.");
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

            int maxKeyLength = 0;

            if (useDefaultMapping)
            {
                maxKeyLength = Math.Max(emojiMappingsMaxKeyLength, defaultMappingsMaxKeyLength);
            }

            if (customMapping != null)
            {
                int customMax = 0;
                foreach (string key in customMapping.Keys)
                {
                    if (key.Length > customMax)
                    {
                        customMax = key.Length;
                    }
                }
                maxKeyLength = Math.Max(maxKeyLength, customMax);
            }

            if (!useDefaultMapping && customMapping == null)
            {
                return NormalizeText(text, normalizationForm);
            }

            // Validate user-provided custom mapping before using it.
            if (!ValidateMappingEntries(customMapping))
            {
                throw new ArgumentOutOfRangeException(nameof(customMapping), "Custom mapping contains invalid entries.");
            }

            // First pass - handle both emoji sequences and complex character mappings
            StringBuilder firstPassResult = new StringBuilder();

            int idx = 0;

            while (idx < text.Length)
            {
                bool found = false;

                int currentMaxPossible = Math.Min(maxKeyLength, text.Length - idx);

                for (int len = currentMaxPossible; len > 0 && !found; len--)
                {
                    if (idx + len <= text.Length)
                    {
                        string candidateSequence = text.Substring(idx, len);

                        // Check custom mappings first
                        if (customMapping != null && customMapping.TryGetValue(candidateSequence, out string customMappingReplacement))
                        {
                            firstPassResult.Append(customMappingReplacement);
                            idx += len;
                            found = true;
                        }
                        else if (useDefaultMapping && emojiUnicodeMappings.TryGetValue(candidateSequence, out string emojiReplacement))
                        {
                            firstPassResult.Append(emojiReplacement);
                            idx += len;
                            found = true;
                        }
                        else if (useDefaultMapping && defaultUnicodeMappings.TryGetValue(candidateSequence, out string mappingReplacement))
                        {
                            firstPassResult.Append(mappingReplacement);
                            idx += len;
                            found = true;
                        }
                    }
                }

                // If no match was found, process as a single character
                if (!found)
                {
                    char chr = text[idx];

                    string charStr = chr.ToString();

                    string unicodeKey = $"U+{(int)chr:X4}";

                    // Try to find in Emoji dictionary by Unicode notation
                    if (useDefaultMapping && Emoji.chars.TryGetValue(unicodeKey, out string emojiReplacement)) // Assuming Emoji.chars is accessible
                    {
                        firstPassResult.Append(emojiReplacement);
                    }
                    // Try to find in DefaultMappings dictionary by Unicode notation
                    else if (useDefaultMapping && DefaultMappings.chars.TryGetValue(unicodeKey, out string mappingReplacement)) // Assuming DefaultMappings.chars is accessible
                    {
                        firstPassResult.Append(mappingReplacement);
                    }
                    else
                    {
                        // If no match, add the character as is
                        firstPassResult.Append(chr);
                    }

                    idx++;
                }
            }

            return NormalizeText(firstPassResult.ToString(), normalizationForm);
        }

        /// <summary>
        /// Asynchronously transliterates and normalizes the text string based on the specified normalization form and optional custom mappings.
        /// This method runs the decomposition process in a separate task to avoid blocking the calling thread.
        /// </summary>
        /// <param name="text">The text string to be processed.</param>
        /// <param name="normalization">The desired Unicode normalization form to apply.</param>
        /// <param name="useDefaultMapping">Indicates whether to use the default character mappings.</param>
        /// <param name="customMapping">An optional dictionary containing custom character or sequence mappings to be applied before the default mappings.
        /// The keys of this dictionary should be Unicode character sequences (as strings), and the values should be their replacement strings.</param>
        /// <returns>A <see cref="Task{String}"/> that represents the asynchronous operation, containing the transliterated and normalized string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="normalization"/> value is not a valid member of the <see cref="Normalization"/> enum.</exception>
        public static async Task<string> DecomposeAsync(
            string text,
            Normalization normalization,
            bool useDefaultMapping = true,
            Dictionary<string, string> customMapping = null
        )
        {
            return await Task.Run(() => Decompose(text, normalization, useDefaultMapping, customMapping));
        }

        /// <summary>
        /// Gets the maximum unicodeNotationLength of the keys in a given dictionary.
        /// </summary>
        /// <param name="dict">The dictionary to examine.</param>
        /// <returns>The maximum unicodeNotationLength of the keys in the dictionary.</returns>
        internal static int GetMaxKeyLength(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return 0;
            }

            int maxLength = 0;

            foreach (string key in dict.Keys)
            {
                if (key.Length > maxLength)
                {
                    maxLength = key.Length;
                }
            }

            return maxLength;
        }

        /// <summary>
        /// Validates a custom mapping dictionary to ensure that keys and values adhere to specific unicodeNotationLength constraints.
        /// </summary>
        /// <param name="dict">The dictionary representing the custom mapping, where keys and values are strings.</param>
        /// <returns>
        /// <c>true</c> if the custom mapping is valid (or null); otherwise, <c>false</c>.
        /// A mapping is considered valid if all keys have a unicodeNotationLength of at most 6 graphemes and all values have a unicodeNotationLength of at most 40 graphemes.
        /// If the <paramref name="dict"/> is null, this method returns <c>true</c>.
        /// </returns>
        internal static bool ValidateMappingEntries(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return true;
            }

            foreach (KeyValuePair<string, string> mappingEntry in dict)
            {
                if (!IsValidGraphemeLength(mappingEntry.Key, 6) || !IsValidGraphemeLength(mappingEntry.Value, 40))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Preprocesses a dictionary by converting Unicode notation (e.g., "U+XXXX" or "U+XXXX U+YYYY") in the keys
        /// to their corresponding actual Unicode characters.
        /// </summary>
        /// <param name="dict">The text dictionary where keys are in Unicode notation.</param>
        /// <returns>A new dictionary where the keys are the actual Unicode characters represented by the notation in the text dictionary.</returns>
        internal static Dictionary<string, string> PrepareDictionary(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> processedDictionary = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> unicodeMappingEntry in dict)
            {
                // Convert from "U+XXXX U+YYYY" format to actual Unicode characters
                string unicodeCharSequence = UnicodeNotationToCharacters(unicodeMappingEntry.Key);

                processedDictionary[unicodeCharSequence] = unicodeMappingEntry.Value;
            }

            return processedDictionary;
        }

        /// <summary>
        /// Checks if the unicodeNotationLength of a string, measured in graphemes, is less than or equal to a specified maximum.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <param name="maxGraphemes">The maximum allowed number of graphemes in the string.</param>
        /// <returns>
        /// <c>true</c> if the string is not null or whitespace and its unicodeNotationLength in graphemes is less than or equal to <paramref name="maxGraphemes"/>; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsValidGraphemeLength(string text, int maxGraphemes)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(text);

            int graphemeCount = 0;

            while (textElementEnumerator.MoveNext())
            {
                graphemeCount++;

                if (graphemeCount > maxGraphemes)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Normalizes the text string according to the specified Unicode normalization form and then removes any non-spacing combining marks.
        /// This process is useful for tasks like text comparison or preparing text for indexing where diacritics or other combining marks should be ignored.
        /// </summary>
        /// <param name="text">The text string to be normalized and processed.</param>
        /// <param name="normalizationForm">The Unicode normalization form to apply. Common forms include NFC, NFD, NFKC, and NFKD.</param>
        /// <returns>A new string that is normalized according to the specified form and has all non-spacing combining marks removed.</returns>
        internal static string NormalizeText(string text, NormalizationForm normalizationForm)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // Now apply normalization to the processedDictionary
            string normalizedText = text.Normalize(normalizationForm);

            // Remove combining marks after normalization
            StringBuilder normalizedResult = new StringBuilder();

            foreach (char currentCharacter in normalizedText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(currentCharacter) != UnicodeCategory.NonSpacingMark)
                {
                    normalizedResult.Append(currentCharacter);
                }
            }

            return normalizedResult.ToString();
        }

        /// <summary>
        /// Converts a string containing Unicode notations (e.g., "U+1F642 U+200D U+2194 U+FE0F")
        /// to a string of the corresponding actual Unicode characters.
        /// </summary>
        /// <param name="text">The string containing Unicode notations, where each notation starts with "U+" followed by the hexadecimal Unicode code point,
        /// and multiple notations can be separated by spaces.</param>
        /// <returns>A string containing the Unicode characters represented by the text notation.</returns>
        internal static string UnicodeNotationToCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder unicodeStringBuilder = new StringBuilder(text.Length / 3); // Estimate capacity

            int currentPosition = 0;

            int unicodeNotationLength = text.Length;

            while (currentPosition < unicodeNotationLength)
            {
                // Find next "U+" marker
                int unicodeMarkerPosition = text.IndexOf("U+", currentPosition, StringComparison.Ordinal);

                if (unicodeMarkerPosition == -1)
                {
                    break;
                }

                // Move past "U+"
                int unicodeHexStartPosition = unicodeMarkerPosition + 2;

                if (unicodeHexStartPosition >= unicodeNotationLength)
                {
                    break;
                }

                // Find end of hex value (space or end of string)
                int unicodeHexEndPosition = text.IndexOf(' ', unicodeHexStartPosition);

                if (unicodeHexEndPosition == -1)
                {
                    unicodeHexEndPosition = unicodeNotationLength;
                }

                // Extract and parse hex value
                if (unicodeHexEndPosition > unicodeHexStartPosition && unicodeHexEndPosition - unicodeHexStartPosition <= 8) // Max valid Unicode is U+10FFFF (6 chars + "U+")
                {
                    string unicodeHexValue = text.Substring(unicodeHexStartPosition, unicodeHexEndPosition - unicodeHexStartPosition);

                    // Try to parse the hex value safely
                    if (int.TryParse(unicodeHexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int unicodeCodePoint))
                    {
                        // Validate Unicode range (U+0000 to U+10FFFF)
                        if (unicodeCodePoint >= 0 && unicodeCodePoint <= 0x10FFFF)
                        {
                            // Convert to UTF-16 character(s)
                            unicodeStringBuilder.Append(char.ConvertFromUtf32(unicodeCodePoint));
                        }
                    }
                }

                // Move to next position after currentCharacter code point
                currentPosition = unicodeHexEndPosition + 1;
            }

            return unicodeStringBuilder.ToString();
        }

        /// <summary>
        /// Validates whether a given string contains only valid Unicode characters. 
        /// A valid Unicode character is defined as any character that is not a control character.
        /// </summary>
        /// <param name="text">The string to be validated.</param>
        /// <returns>
        ///   <c>true</c> if the string is not null or empty, and does not contain any control characters; otherwise, <c>false</c>.
        /// </returns>
        internal static bool ValidateUnicodeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            bool awaitingLowSurrogate = false;

            for (int index = 0; index < text.Length; index++)
            {
                char currentCharacter = text[index];

                if (awaitingLowSurrogate)
                {
                    if (!char.IsLowSurrogate(currentCharacter))
                    {
                        return false;
                    }

                    awaitingLowSurrogate = false;
                }
                else
                {
                    if (char.IsHighSurrogate(currentCharacter))
                    {
                        if (index == text.Length - 1)
                        {
                            return false; // High surrogate at end of string
                        }

                        awaitingLowSurrogate = true;
                    }
                    else if (char.IsLowSurrogate(currentCharacter))
                    {
                        return false; // Unexpected low surrogate
                    }
                }
            }

            return !awaitingLowSurrogate; // Check for missing low surrogate at end
        }
    }
}
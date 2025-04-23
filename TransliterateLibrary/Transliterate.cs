using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        internal static readonly Dictionary<string, string> emojiUnicodeMappings = new Dictionary<string, string>();

        /// <summary>
        /// A dictionary mapping Unicode representations of various characters or character sequences to their replacement strings.
        /// This dictionary is initialized during the class's static initialization.
        /// </summary>
        internal static readonly Dictionary<string, string> defaultUnicodeMappings = new Dictionary<string, string>();

        /// <summary>
        /// Stores the maximum length of keys in the <see cref="emojiUnicodeMappings"/> dictionary.
        /// This value is calculated during the class's static initialization to optimize lookup operations.
        /// </summary>
        internal static readonly int emojiMappingsMaxKeyLength;

        /// <summary>
        /// Stores the maximum length of keys in the <see cref="defaultUnicodeMappings"/> dictionary.
        /// This value is calculated during the class's static initialization to optimize lookup operations.
        /// </summary>
        internal static readonly int defaultMappingsMaxKeyLength;

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

            // If no mappings are used, just normalize and return
            if (!useDefaultMapping && customMapping == null)
            {
                return NormalizeText(text, normalizationForm);
            }

            // Validate user-provided custom mapping
            if (!ValidateMappingEntries(customMapping))
            {
                throw new ArgumentOutOfRangeException(nameof(customMapping), "Custom mapping contains invalid entries.");
            }

            // Calculate the maximum key length for sequence matching
            int maxKeyLength = GetCustomMappingMaxKegLength(useDefaultMapping, customMapping);


            StringBuilder result = new StringBuilder(text.Length);

            int idx = 0;

            while (idx < text.Length)
            {
                // Try to match multi-character sequences first, from longest to shortest
                if (TryMatchSequence(text, idx, maxKeyLength, useDefaultMapping, customMapping, out string replacement, out int matchLength))
                {
                    result.Append(replacement);

                    idx += matchLength;

                    continue;
                }

                // No multi-character sequence matched, process as single character
                string charReplacement = GetCharacterReplacement(text, idx, useDefaultMapping);

                result.Append(charReplacement);

                // Move past this character (handle surrogate pairs)
                idx += char.IsHighSurrogate(text[idx]) &&
                    idx + 1 < text.Length &&
                    char.IsLowSurrogate(text[idx + 1]) ? 2 : 1;
            }

            return NormalizeText(result.ToString(), normalizationForm);
        }

        /// <summary>
        /// Attempts to find the longest possible replacement match for a sequence of characters
        /// starting at the given index in the input string, considering custom and default mappings.
        /// </summary>
        /// <param name="text">The input string to search within.</param>
        /// <param name="index">The zero-based index in the <paramref name="text"/> where the search for a match begins.</param>
        /// <param name="maxKeyLength">The maximum possible length of a key in any of the mappings being considered. This helps optimize the search.</param>
        /// <param name="useDefaultMapping">A boolean value indicating whether to include the default emoji and standard mappings in the search.</param>
        /// <param name="customMapping">An optional dictionary containing custom string mappings. These mappings are checked first and take priority over default mappings.</param>
        /// <param name="replacement">When this method returns <c>true</c>, contains the replacement string found for the matched sequence; otherwise, the default value for the type (<c>null</c>).</param>
        /// <param name="matchLength">When this method returns <c>true</c>, contains the length of the matched sequence in the input string; otherwise, <c>0</c>.</param>
        /// <returns>
        /// <c>true</c> if a matching sequence is found in either the custom or default mappings;
        /// otherwise, <c>false</c>.
        /// </returns>
        private static bool TryMatchSequence(
            string text,
            int index,
            int maxKeyLength,
            bool useDefaultMapping,
            Dictionary<string, string> customMapping,
            out string replacement,
            out int matchLength)
        {
            // Determine the maximum possible length for a candidate sequence starting at the current index.
            int maxPossibleLength = Math.Min(maxKeyLength, text.Length - index);

            // Ensure that the longest possible match is found first.
            for (int len = maxPossibleLength; len > 0; len--)
            {
                // Extract the candidate sequence from the input string at the current index with the current length.
                string candidateSequence = text.Substring(index, len);

                // Check custom mappings first. Custom mappings have higher priority.
                if (customMapping != null && customMapping.TryGetValue(candidateSequence, out replacement))
                {
                    // If a match is found in custom mappings, set the match length and return true.
                    matchLength = len;

                    return true;
                }

                // If no match was found in custom mappings, and default mappings are enabled, check them.
                if (useDefaultMapping)
                {
                    // Check in the emoji unicode mappings dictionary (assuming emojiUnicodeMappings is a Dictionary<string, string>)
                    if (emojiUnicodeMappings.TryGetValue(candidateSequence, out replacement))
                    {
                        // If a match is found in emoji mappings, set the match length and return true.
                        matchLength = len;

                        return true;
                    }

                    // Check in the default standard unicode mappings dictionary (assuming defaultUnicodeMappings is a Dictionary<string, string>)
                    if (defaultUnicodeMappings.TryGetValue(candidateSequence, out replacement))
                    {
                        // If a match is found in default mappings, set the match length and return true.
                        matchLength = len;

                        return true;
                    }
                }
            }

            // If the loop completes without finding any match, set output parameters to default values and return false.
            replacement = null;

            matchLength = 0;

            return false;
        }

        /// <summary>
        /// Gets the replacement string for a single character or a surrogate pair at the specified index
        /// within the input string, based on default emoji and standard mappings.
        /// </summary>
        /// <param name="text">The input string containing the character to replace.</param>
        /// <param name="index">The zero-based index of the character (or the first character of a surrogate pair) within the string.</param>
        /// <param name="useDefaultMapping">A boolean value indicating whether to check the default emoji and standard mappings for a replacement.</param>
        /// <returns>
        /// The replacement string found in the default mappings if <paramref name="useDefaultMapping"/> is true
        /// and a match is found for the character/code point at the given index.
        /// Returns the original character(s) at the index as a string if no replacement is found or if
        /// <paramref name="useDefaultMapping"/> is false.
        /// </returns>
        private static string GetCharacterReplacement(string text, int index, bool useDefaultMapping)
        {
            char currentChar = text[index];

            // Handle both BMP and surrogate pair characters
            int codePoint;

            bool isSurrogatePair = char.IsHighSurrogate(currentChar) &&
                                  index + 1 < text.Length &&
                                  char.IsLowSurrogate(text[index + 1]);

            if (isSurrogatePair)
            {
                codePoint = char.ConvertToUtf32(currentChar, text[index + 1]);
            }
            else
            {
                codePoint = currentChar;
            }

            // If default mappings are enabled, check them
            if (useDefaultMapping)
            {
                // Check in emoji dictionary
                foreach (KeyValuePair<int[], string> entry in Emoji.chars)
                {
                    if (entry.Key.Length == 1 && entry.Key[0] == codePoint)
                    {
                        return entry.Value;
                    }
                }

                // Check in default mappings dictionary
                foreach (KeyValuePair<int[], string> entry in DefaultMappings.chars)
                {
                    if (entry.Key.Length == 1 && entry.Key[0] == codePoint)
                    {
                        return entry.Value;
                    }
                }
            }

            // If no replacement found, return the original character
            return isSurrogatePair
                ? char.ConvertFromUtf32(codePoint)
                : currentChar.ToString();
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
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> that represents the asynchronous operation, containing the transliterated and normalized string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="normalization"/> value is not a valid member of the <see cref="Normalization"/> enum.</exception>
        /// <exception cref="ArgumentException">Thrown if the input text is null or empty, or if it contains invalid Unicode characters.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
        public static async Task<string> DecomposeAsync(
            string text,
            Normalization normalization,
            bool useDefaultMapping = true,
            Dictionary<string, string> customMapping = null,
            CancellationToken cancellationToken = default
        )
        {
            return await Task.Run(() =>
                Decompose(text, normalization, useDefaultMapping, customMapping),
                cancellationToken
            );
        }

        /// <summary>
        /// Calculates the maximum length among the string keys in a given dictionary.
        /// </summary>
        /// <param name="dict">The dictionary whose string keys will be examined to find the maximum length.</param>
        /// <returns>
        /// The maximum length of any key in the dictionary.
        /// Returns 0 if the dictionary is null or empty.
        /// </returns>
        internal static int GetMaxKeyLength(Dictionary<string, string> dict = null)
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
        /// Calculates the maximum key length considering default mappings and an optional custom mapping.
        /// </summary>
        /// <param name="useDefaultMapping">A boolean value indicating whether to include the maximum key lengths from default emoji and standard mappings in the calculation.</param>
        /// <param name="customMapping">An optional dictionary containing custom string mappings. The maximum length of keys in this dictionary will be considered if the dictionary is not null or empty.</param>
        /// <returns>The maximum key length found among the considered mappings (default and/or custom).</returns>
        private static int GetCustomMappingMaxKegLength(bool useDefaultMapping, Dictionary<string, string> customMapping = null)
        {
            int maxKeyLength = useDefaultMapping
                ? Math.Max(emojiMappingsMaxKeyLength, defaultMappingsMaxKeyLength)
                : 0;

            if (customMapping != null && customMapping.Count > 0)
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

            return maxKeyLength;
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

            string normalizedText = string.Empty;

            try
            {
                // Now apply normalization to the processedDictionary
                normalizedText = text.Normalize(normalizationForm);
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }

            // Remove combining marks after normalization
            int estimatedCapacity = Math.Max(1, (int)(normalizedText.Length * 0.8));

            StringBuilder normalizedResult = new StringBuilder(estimatedCapacity);

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
        /// Converts a dictionary where keys are arrays of integer Unicode code points
        /// to a new dictionary where keys are the corresponding actual Unicode characters.
        /// This method handles single and multi-code point characters and validates code points.
        /// </summary>
        /// <param name="dict">The input dictionary where keys are arrays of integer Unicode code points and values are strings.</param>
        /// <returns>A new dictionary where the keys are the actual Unicode characters represented by the code points. Returns an empty dictionary if the input is null or empty, or if no valid keys can be generated.</returns>
        internal static Dictionary<string, string> PrepareDictionary(Dictionary<int[], string> dict)
        {
            // Return an empty dictionary immediately if the input is null or empty
            if (dict == null || dict.Count == 0)
            {
                return new Dictionary<string, string>(0);
            }

            // Initialize the result dictionary with a capacity based on the input dictionary count
            Dictionary<string, string> result = new Dictionary<string, string>(dict.Count);

            // Iterate through each key-value pair in the input dictionary
            foreach (var entry in dict)
            {
                int[] codePoints = entry.Key;

                // Skip the entry if the code point array key is null or empty
                if (codePoints == null || codePoints.Length == 0)
                {
                    continue;
                }

                StringBuilder sb = new StringBuilder(codePoints.Length);

                // Convert each integer code point to its corresponding Unicode character(s)
                foreach (int codePoint in codePoints)
                {
                    // Validate if the code point is within the valid Unicode range (U+0000 to U+10FFFF)
                    if (codePoint >= 0 && codePoint <= 0x10FFFF)
                    {
                        try
                        {
                            // Convert the integer code point to a string representation of the character(s)
                            // char.ConvertFromUtf32 handles surrogate pairs for characters outside the Basic Multilingual Plane (BMP)
                            sb.Append(char.ConvertFromUtf32(codePoint));
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Catch potential exceptions if ConvertFromUtf32 encounters an invalid sequence or value
                            // Although the range check is done, this adds robustness.
                            // If an invalid code point is encountered, skip it and continue with the next.
                            // This might happen for values within the range but not valid UTF-32 code points (e.g., surrogates outside valid pairs).
                        }
                    }
                    // Code points outside the valid Unicode range are ignored
                }

                string unicodeCharSequence = sb.ToString();

                if (!string.IsNullOrEmpty(unicodeCharSequence))
                {
                    if (!result.ContainsKey(unicodeCharSequence))
                    {
                        result.Add(unicodeCharSequence, entry.Value);
                    }
                }
                // Entries with null, empty, or only invalid code points will not be added to the result dictionary
            }

            return result;
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
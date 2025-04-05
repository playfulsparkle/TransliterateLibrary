using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PlayfulSparkle
{
    public class Transliterate
    {
        private static Dictionary<string, string> smileyUnicodeToReplacement = new Dictionary<string, string>();

        private static Dictionary<string, string> mappingsUnicodeToReplacement = new Dictionary<string, string>();

        public enum Normalization
        {
            Decompose, // NFD
            Compose, // NFC
            CompatibilityCompose, // NFKC
            CompatibilityDecompose // NFKD
        }

        static Transliterate()
        {
            smileyUnicodeToReplacement = PreprocessDictionary(Smiley.chars);

            mappingsUnicodeToReplacement = PreprocessDictionary(Mappings.chars);
        }

        public static string Decompose(string str, Normalization normalization, Dictionary<string, string> customMapping = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            NormalizationForm normalizationForm;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(normalization), normalization, null);
            }
            
            if (customMapping != null)
            {
                customMapping = PreprocessDictionary(customMapping);
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
                        else if (mappingsUnicodeToReplacement.TryGetValue(candidateSequence, out string mappingReplacement))
                        {
                            firstPassResult.Append(mappingReplacement);

                            idx += len;

                            found = true;
                        }
                        else if (smileyUnicodeToReplacement.TryGetValue(candidateSequence, out string smileyReplacement))
                        {
                            firstPassResult.Append(smileyReplacement);

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

                    // Try to find in Mappings dictionary by Unicode notation
                    if (Mappings.chars.TryGetValue(unicodeKey, out string mappingReplacement))
                    {
                        firstPassResult.Append(mappingReplacement);
                    }
                    // Try to find in Smiley dictionary by Unicode notation
                    else if (Smiley.chars.TryGetValue(unicodeKey, out string smileyReplacement))
                    {
                        firstPassResult.Append(smileyReplacement);
                    }
                    else
                    {
                        // If no match, add the character as is
                        firstPassResult.Append(chr);
                    }

                    idx++;
                }
            }

            // Now apply normalization to the result
            string normalizedResult = firstPassResult.ToString().Normalize(normalizationForm);

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

        // Preprocess a dictionary to convert Unicode notation to actual characters
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

        // Convert Unicode notation like "U+1F642 U+200D U+2194 U+FE0F" to actual characters
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
                    
                    if (intValue <= 0xFFFF) // Handle values beyond the BMP (Basic Multilingual Plane)
                    {
                        result.Append((char)intValue);
                    }
                    else // Convert to surrogate pairs for values outside BMP
                    {
                        result.Append(char.ConvertFromUtf32(intValue));
                    }
                }
            }

            return result.ToString();
        }
    }
}

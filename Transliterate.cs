using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PlayfulSparkle
{
    public class Transliterate
    {
        public enum Normalization
        {
            Decompose, // NFD
            Compose, // NFC
            CompatibilityCompose, // NFKC
            CompatibilityDecompose // NFKD
        }

        public static string Decompose(string str, Normalization normalization)
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

            string normalized = str.Normalize(normalizationForm);

            // Remove combining marks
            StringBuilder result = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    // Apply character mapping if available
                    string charStr = c.ToString();

                    if (Mappings.chars.TryGetValue($"U+{(int)c:X4}", out string unicodeReplacement))
                    {
                        result.Append(unicodeReplacement);
                    }
                    if (Smiley.chars.TryGetValue($"U+{(int)c:X4}", out string smileyReplacement))
                    {
                        result.Append(smileyReplacement);
                    }
                    else
                    {
                        result.Append(charStr);
                    }
                }
            }

            return result.ToString();
        }
    }
}

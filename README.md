# Playful Sparkle: Transliterate Library

**Playful Sparkle: Transliterate Library** is a high-precision, extensible C# library targeting .NET Standard 2.0, purpose-built for deterministic Unicode transliteration and normalization. It enables structured transformation of input strings by decomposing composite characters, replacing multi-character graphemes and emoji sequences with pre-defined or user-defined ASCII-compatible mappings, and re-normalizing the result using standardized Unicode normalization forms (NFD, NFC, NFKD, NFKC).

The library is designed to process complex Unicode input in a consistent and idempotent manner. It supports:
- Full control over normalization granularity (canonical vs compatibility, composition vs decomposition),
- Multi-pass mapping resolution for both standard character sets and emoji/smiley code points,
- On-the-fly integration of user-defined mapping dictionaries,
- Surrogate pair decoding for code points beyond the Basic Multilingual Plane,
- Post-normalization filtering to remove diacritics and combining marks for ASCII-safe output.

Ideal for applications requiring language-agnostic preprocessing such as SEO sanitization, search indexing, canonical form comparison, filename/path generation, and legacy system compatibility.

---

## Features

- - Full Unicode normalization support, including NFD (Decompose), NFC (Compose), NFKC (Compatibility Compose), and NFKD (Compatibility Decompose) normalization forms
- Transliteration of accented characters, diacritics, and complex Unicode sequences (e.g., ligatures, non-ASCII characters) to simplified ASCII representations
- Smiley and emoji sequence replacement based on pre-defined Unicode mappings for consistent transformation
- Efficient handling of multi-codepoint sequences, such as emojis and other combined graphemes, ensuring correct encoding and substitution
- Removal of non-spacing marks (diacritics) after normalization for producing clean, ASCII-compatible strings
- Support for customizable character-to-character mappings via user-defined dictionaries for project-specific transliteration needs
- Preprocessing of Unicode mapping dictionaries from `U+XXXX` notation, converting them into valid character sequences for processing
- Cross-platform compatibility with all environments supporting .NET Standard 2.0, enabling integration into various .NET applications

---

## Known Issues

- **Custom mappings reprocessing**: Custom mappings are reprocessed on each method call, which may impact performance when dealing with large input strings or multiple consecutive transliterations.

---

## Release Notes

### 0.0.1

* Initial public release of the **Playful Sparkle: Transliterate Library** extension for Visual Studio.

---

## Support

For any inquiries, bug reports, or feature requests related to the **Playful Sparkle: Transliterate Library** extension, please feel free to utilize the following channels:

* **GitHub Issues**: For bug reports, feature suggestions, or technical discussions, please open a new issue on the [GitHub repository](https://github.com/playfulsparkle/vs_ps_replace_accents/issues). This allows for community visibility and tracking of reported issues.
* **Email Support**: For general questions or private inquiries, you can contact the developer directly via email at `support@playfulsparkle.com`. Please allow a reasonable timeframe for a response.

We encourage users to use the GitHub Issues page for bug reports and feature requests as it helps in better organization and tracking of the extension's development.

---

## License

This extension is licensed under the [BSD-3-Clause License](https://github.com/playfulsparkle/vs_ps_replace_accents/blob/main/LICENSE). See the `LICENSE` file for complete details.

---
## Author

Hi! We're the team behind Playful Sparkle, a creative agency from Slovakia. We got started way back in 2004 and have been having fun building digital solutions ever since. Whether it's crafting a brand, designing a website, developing an app, or anything in between, we're all about delivering great results with a smile. We hope you enjoy using our Visual Studio extension!
---

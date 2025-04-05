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

## API

### Decompose

The `Decompose` method is used to transliterate and normalize an input string based on a specified Unicode normalization form. It processes the string by applying custom character mappings (if provided) and default mappings for complex characters (e.g., emoji and Unicode sequences). The method can be used to decompose composed characters into their base forms or apply other normalization forms like composition or compatibility normalization.

```csharp
public static string Decompose(string str, Normalization? normalization = null, Dictionary<string, string> customMapping = null)
```

**Parameters:**

* **str:** The input string to be processed.
* **normalization:** The desired Unicode normalization form to apply. This can be one of the values from the Normalization enum:
	- **Normalization.Decompose:** Decomposes composed characters into base characters and combining characters (NFD).
	- **Normalization.Compose:** Combines characters into composed characters (NFC).
	- **Normalization.CompatibilityCompose:** Applies compatibility decomposition followed by composition (NFKC).
	- **Normalization.CompatibilityDecompose:** Applies compatibility decomposition followed by decomposition (NFKD).
* **customMapping (optional):** A dictionary of custom character mappings to apply before the default mappings. Keys should be Unicode character sequences as strings, and values should be their replacement strings.

**Returns:**

The transliterated and normalized string.

**Example:**

```csharp
string input = "Some text with 😊 and complex characters!";

var result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

Console.WriteLine(result);
```

### DecomposeAsync

The `DecomposeAsync` method is an asynchronous version of the `Decompose` method. It runs the transliteration and normalization process in a separate task to avoid blocking the calling thread, which is useful in scenarios where you need to process large strings or perform the operation without affecting the responsiveness of your application.

```csharp
public static async Task<string> DecomposeAsync(string str, Normalization? normalization = null, Dictionary<string, string> customMapping = null)
```

**Parameters:**

* **str:** The input string to be processed.
* **normalization:** The desired Unicode normalization form to apply. This can be one of the values from the Normalization enum:
	- **Normalization.Decompose:** Decomposes composed characters into base characters and combining characters (NFD).
	- **Normalization.Compose:** Combines characters into composed characters (NFC).
	- **Normalization.CompatibilityCompose:** Applies compatibility decomposition followed by composition (NFKC).
	- **Normalization.CompatibilityDecompose:** Applies compatibility decomposition followed by decomposition (NFKD).
* **customMapping (optional):** A dictionary of custom character mappings to apply before the default mappings. Keys should be Unicode character sequences as strings, and values should be their replacement strings.

**Returns:**

A `Task<string>` representing the asynchronous operation, containing the transliterated and normalized string.

**Example:**

```csharp
string input = "Another text with 😊 and more complex characters!";

var result = await Transliterate.DecomposeAsync(input, Transliterate.Normalization.Compose);

Console.WriteLine(result);
```

### PreprocessDictionary

The `PreprocessDictionary` method is used to process a dictionary where keys are Unicode notations (e.g., `"U+XXXX"` or `"U+XXXX U+YYYY"`) and convert them into actual Unicode characters.

```csharp
public static Dictionary<string, string> PreprocessDictionary(Dictionary<string, string> source)
```

**Parameters:**

* **source:** The input dictionary where keys are in Unicode notation (e.g., `"U+1F642"`).

**Returns:**

A new dictionary where the keys are actual Unicode characters represented by the notation in the input dictionary.

**Example:**

```csharp
Dictionary<string, string> unicodeMapping = new Dictionary<string, string>
{
    { "U+1F642", "😊" },
    { "U+1F60D", "😍" }
};

var processedDictionary = Transliterate.PreprocessDictionary(unicodeMapping);
foreach (var entry in processedDictionary)
{
    Console.WriteLine($"Unicode: {entry.Key}, Replacement: {entry.Value}");
}
```

---

## Known Issues

- **Custom mappings reprocessing**: Custom mappings are reprocessed on each method call, which may impact performance when dealing with large input strings or multiple consecutive transliterations.

---

## Release Notes

### 0.0.12

* Fixed: `IsValidUnicodeString` method has been replaced with a more comprehensive implementation that:
	- Properly validates surrogate pairs in Unicode strings
	- Detects isolated high and low surrogates (e.g., character 0xD800)
	- Identifies Unicode noncharacters (ranges 0xFDD0-0xFDEF and code points ending with FFFE/FFFF)
	- Uses UTF-8 encoding validation as an additional verification layer
	- Maintains performance while providing more accurate Unicode validation

### 0.0.11

* Fixed: The `IsValidUnicodeString` method now accurately validates user input for valid Unicode sequences by correctly identifying and rejecting strings containing lone high or low surrogate characters.

### 0.0.10

* Updated API documentation.

### 0.0.9

* Removed `PreprocessDictionary` for the user-defined character mapping. The user can now pass directly to the `Decompose` method.

### 0.0.8

* Added async version of `Decompose` method for non-blocking operations.
* Added check for user input to ensure valid input.

### 0.0.7

* Added custom mappings support for user-defined character mappings.

### 0.0.6

* Updated `Decompose` method to handle surrogate pairs and multi-codepoint sequences.

### 0.0.5

* Testing default mapping unicode notation to latin characters.

### 0.0.4

* Testing unicode notation to latin characters.

### 0.0.3

* Implemented core functionality for the **Playful Sparkle: Transliterate Library** extension for Visual Studio.
* Added Emoji and smiley sequence replacement based on pre-defined Unicode mappings.
* Added default mappings for complex characters (e.g., ligatures, non-ASCII characters) to simplified ASCII representations.

### 0.0.2

* Added core files and project documentation for the **Playful Sparkle: Transliterate Library** extension for Visual Studio.

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

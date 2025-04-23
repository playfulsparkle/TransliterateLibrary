using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlayfulSparkle;

namespace TransliterateLibrary.UnitTest
{
    [TestClass]
    public class TransliterateTest
    {
        [TestMethod]
        public void Test_Emoji_Dataset()
        {
           Dictionary<string, string> emojiUnicodeMappings = Transliterate.PrepareDictionary(Emoji.chars);

            foreach (KeyValuePair<string, string> entry in emojiUnicodeMappings)
            {
                // Arrange
                string input = entry.Key;
                string expected = entry.Value;

                // Act
                string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void Test_DefaultMappings_Dataset()
        {
            Dictionary<string, string> defaultUnicodeMappings = Transliterate.PrepareDictionary(DefaultMappings.chars);

            foreach (KeyValuePair<string, string> entry in defaultUnicodeMappings)
            {
                // Arrange
                string input = entry.Key;
                string expected = entry.Value;

                // Act
                string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

                // Assert
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void Decompose_ShouldReturn_ComplexEol()
        {
            // Arrange
            string input = "To insert this: \tPress these keys:\r\nà, è, ì, ò, ù, À, È, Ì, Ò, Ù \tCtrl+` (accent grave), the letter\r\ná, é, í, ó, ú, ý, Á, É, Í, Ó, Ú, Ý\tCtrl+' (apostrophe), the letter\r\nâ, ê, î, ô, û, Â, Ê, Î, Ô, Û\tCtrl+Shift+^ (caret), the letter\r\nã, ñ, õ, Ã, Ñ, Õ\tCtrl+Shift+~ (tilde), the letter\r\nä, ë, ï, ö, ü, ÿ, Ä, Ë, Ï, Ö, Ü, Ÿ\tCtrl+Shift+: (colon), the letter\r\nå, Å\tCtrl+Shift+@ (At), a or A\r\næ, Æ\tCtrl+Shift+& (ampersand), a or A\r\nœ, Œ\tCtrl+Shift+& (ampersand), o or O\r\nç, Ç\tCtrl+, (comma), c or C\r\nð, Ð\tCtrl+' (apostrophe), d or D\r\nø, Ø\tCtrl+/, o or O\r\n¿\tAlt+Ctrl+Shift+?\r\n¡\tAlt+Ctrl+Shift+!\r\nß\tCtrl+Shift+&, s";
            string expected = "To insert this: \tPress these keys:\r\na, e, i, o, u, A, E, I, O, U \tCtrl+` (accent grave), the letter\r\na, e, i, o, u, y, A, E, I, O, U, Y\tCtrl+' (apostrophe), the letter\r\na, e, i, o, u, A, E, I, O, U\tCtrl+Shift+^ (caret), the letter\r\na, n, o, A, N, O\tCtrl+Shift+~ (tilde), the letter\r\nae, e, i, oe, ue, y, Ae, E, I, Oe, Ue, Y\tCtrl+Shift+: (colon), the letter\r\na, A\tCtrl+Shift+@ (At), a or A\r\nae, AE\tCtrl+Shift+& (ampersand), a or A\r\nœ, Œ\tCtrl+Shift+& (ampersand), o or O\r\nc, C\tCtrl+, (comma), c or C\r\nd, D\tCtrl+' (apostrophe), d or D\r\no, O\tCtrl+/, o or O\r\n¿\tAlt+Ctrl+Shift+?\r\n¡\tAlt+Ctrl+Shift+!\r\nss\tCtrl+Shift+&, s";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Decompose_ShouldReturn_HeadShakingHorizontally()
        {
            // Arrange
            string input = "🙂‍↔️";
            string expected = "head shaking horizontally";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Decompose_ShouldReturn_NerdFace()
        {
            // Arrange
            string input = "🤓";
            string expected = "nerd face";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Decompose_ShouldReturn_DecomposedBasicUnicodeText()
        {
            // Arrange
            string input = "éáűőúóüöíÉÁŰÚŐÓÜÖÍôňúäéáýžťčšľÔŇÚÄÉÁÝŽŤČŠĽ";
            string expected = "eauououeoeiEAUUOOUeOeIonuaeeayztcslONUAeEAYZTCSL";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Decompose_ShouldReturn_ComplexText()
        {
            // Arrange
            string input = "你好, 世界! This is a test with ümlauts(üöä) and emojis 😊👍.";
            string expected = "你好, 世界! This is a test with uemlauts(ueoeae) and emojis smiling face with smiling eyesthumbs up.";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_EnglishWithSymbolsAndAccents()
        {
            // Arrange
            string input = "I ❤ cofée";
            string expected = "I red heart cofee";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_GermanWithUmlauts()
        {
            // Arrange
            string input = "Fußgängerübergänge";
            string expected = "Fussgaengeruebergaenge";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_Russian()
        {
            // Arrange
            string input = "Я люблю единорогов";
            string expected = "Ya lyublyu edinorogov";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_Arabic()
        {
            // Arrange
            string input = "أنا أحب حيدات";
            string expected = "ana ahb hydat";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_Vietnamese()
        {
            // Arrange
            string input = "tôi yêu những chú kỳ lân";
            string expected = "toi yeu nhung chu ky lan";

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_NullInput()
        {
            // Arrange
            string input = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Transliterate.Decompose(input, Transliterate.Normalization.Decompose));
        }

        [TestMethod]
        public void TestDecompose_EmptyInput()
        {
            // Arrange
            string input = "";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Transliterate.Decompose(input, Transliterate.Normalization.Decompose));
        }

        [TestMethod]
        public void TestDecompose_InvalidUnicodeString()
        {
            // Arrange
            char invalidChar = (char)0xD800;

            string input = "ValidPart" + invalidChar + "AnotherValidPart";

            // Act & Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Transliterate.Decompose(input, Transliterate.Normalization.Decompose));
        }

        [TestMethod]
        public void TestDecompose_CustomMapping()
        {
            // Arrange
            string input = "test_custom";
            string expected = "test-custom";

            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "_", "-" }
            };

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose, true, customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestDecompose_CustomMappingOverridesDefault()
        {
            // Arrange
            string input = "ee";
            string expected = "xx";

            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "e", "x" }
            };

            // Act
            string result = Transliterate.Decompose(input, Transliterate.Normalization.Decompose, true, customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestStrLength_EmptyString()
        {
            // Arrange
            string input = "";
            int maxGraphemes = 5;
            bool expected = false;

            // Act
            bool result = Transliterate.IsValidGraphemeLength(input, maxGraphemes);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestStrLength_WithinLimit()
        {
            // Arrange
            string input = "hello";
            int maxGraphemes = 5;
            bool expected = true;

            // Act
            bool result = Transliterate.IsValidGraphemeLength(input, maxGraphemes);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestStrLength_ExceedsLimit()
        {
            // Arrange
            string input = "hello";
            int maxGraphemes = 4;
            bool expected = false;

            // Act
            bool result = Transliterate.IsValidGraphemeLength(input, maxGraphemes);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestStrLength_ComplexCharacters()
        {
            // Arrange
            string input = "äb"; // 'a' + combining diaeresis + 'b' - should be 2 graphemes
            int maxGraphemes = 2;
            bool expected = true;

            // Act
            bool result = Transliterate.IsValidGraphemeLength(input, maxGraphemes);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_NullMapping()
        {
            // Arrange
            Dictionary<string, string> customMapping = null;
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_EmptyMapping()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>();
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ValidMapping()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "a", "value" },
                { "bb", "another value" }
            };
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_InvalidKeyLength()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "toolongkey", "value" }
            };
            bool expected = false;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_InvalidValueLength()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "a", new string('x', 41) }
            };
            bool expected = false;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_BothInvalidLength()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "toolongkey", new string('x', 41) }
            };
            bool expected = false;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInKey()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "äbcde", "value" } // 5 graphemes
            };
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInKeyExceedsLimit()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "äbcdef", "value" } // 6 graphemes
            };
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInKeyJustExceedsLimit()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "äbcdefg", "value" } // 7 graphemes
            };
            bool expected = false;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInValue()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "abc", "value with äb" } // value length is 12, which is less than 40
            };
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInValueExceedsLimit()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "abc", new string('a', 38) + "äb" } // value length is 40
            };
            bool expected = true;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestValidateCustomMapping_ComplexCharactersInValueJustExceedsLimit()
        {
            // Arrange
            Dictionary<string, string> customMapping = new Dictionary<string, string>()
            {
                { "abc", new string('a', 39) + "äb" } // value length is 41
            };
            bool expected = false;

            // Act
            bool result = Transliterate.ValidateMappingEntries(customMapping);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}

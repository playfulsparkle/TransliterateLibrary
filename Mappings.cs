using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace PlayfulSparkle
{
    internal class Mappings
    {
        public static Dictionary<string, string> chars = new Dictionary<string, string>()
        {
{"U+0435", "e"},
 {"U+0401", "Yo"},
 {"U+0451", "yo"},
 {"U+0416", "Zh"},
 {"U+0436", "zh"},
 {"U+0417", "Z"},
 {"U+0437", "z"},
 {"U+0418", "I"},
 {"U+0438", "i"},
 {"U+044B U+0439", "iy"},
 {"U+042B U+0439", "Iy"},
 {"U+042B U+0419", "IY"},
 {"U+044B U+0419", "iY"},
        };
    }
}

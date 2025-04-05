using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PlayfulSparkle
{
    internal class Smiley
    {
        public static Dictionary<string, string> chars = new Dictionary<string, string>()
        {
            // Face smiling
            {"U+1F600", "grinning face"},
            {"U+1F603", "grinning face with big eyes"},
            {"U+1F604", "grinning face with smiling eyes"},
            {"U+1F601", "beaming face with smiling eyes"},
            {"U+1F606", "grinning squinting face"},
            {"U+1F605", "grinning face with sweat"},
            {"U+1F923", "rolling on the floor laughing"},
            {"U+1F602", "face with tears of joy"},
            {"U+1F642", "slightly smiling face"},
            {"U+1F643", "upside-down face"},
            {"U+1FAE0", "melting face"},
            {"U+1F609", "winking face"},
            {"U+1F60A", "smiling face with smiling eyes"},
            {"U+1F607", "smiling face with halo"},

            // Face affection
            {"U+1F970", "smiling face with hearts"},
            {"U+1F60D", "smiling face with heart-eyes"},
            {"U+1F929", "star-struck"},
            {"U+1F618", "face blowing a kiss"},
            {"U+1F617", "kissing face"},
            {"U+263A", "smiling face"},
            {"U+1F61A", "kissing face with closed eyes"},
            {"U+1F619", "kissing face with smiling eyes"},
            {"U+1F972", "smiling face with tear"},

            // Face tongue
            {"U+1F60B", "face savoring food"},
            {"U+1F61B", "face with tongue"},
            {"U+1F61C", "winking face with tongue"},
            {"U+1F92A", "zany face"},
            {"U+1F61D", "squinting face with tongue"},
            {"U+1F911", "money-mouth face"},

            // Face hand
            {"U+1F917", "smiling face with open hands"},
            {"U+1F92D", "face with hand over mouth"},
            {"U+1FAE2", "face with open eyes and hand over mouth"},
            {"U+1FAE3", "face with peeking eye"},
            {"U+1F92B", "shushing face"},
            {"U+1F914", "thinking face"},
            {"U+1FAE1", "saluting face"},

            // Face neutral skeptical
            {"U+1F910", "zipper-mouth face"},
            {"U+1F928", "face with raised eyebrow"},
            {"U+1F610", "neutral face"},
            {"U+1F611", "expressionless face"},
            {"U+1F636", "face without mouth"},
            {"U+1FAE5", "dotted line face"},
            {"U+1F636 U+200D U+1F32B U+FE0F", "face in clouds"},
            {"U+1F60F", "smirking face"},
            {"U+1F612", "unamused face"},
            {"U+1F644", "face with rolling eyes"},
            {"U+1F62C", "grimacing face"},
            {"U+1F62E U+200D U+1F4A8", "face exhaling"},
            {"U+1F925", "lying face"},
            {"U+1FAE8", "shaking face"},
            {"U+1F642 U+200D U+2194 U+FE0F", "head shaking horizontally"},
            {"U+1F642 U+200D U+2195 U+FE0F", "head shaking vertically"},
        };
    }
}

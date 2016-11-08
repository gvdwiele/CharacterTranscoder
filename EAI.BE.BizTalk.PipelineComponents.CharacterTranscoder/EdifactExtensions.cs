using EAI.BE.BizTalk.PipelineComponents;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EAI.BE.BizTalk.Extensions
{

    public static class EdifactExtensions
    {
        private static readonly Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");

        private static readonly Encoding Utf8 = new UTF8Encoding();

        public static bool IsUnoa(this char c)
        {

            //UNOA allows:

            //A to Z
            //0 to 9
            //. , – ( ) / = (space)


            return (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '.' || c == ',' || c == '-' || c == '(' || c == ')' || c == '/' || c == '=' || c == ' ';
        }

        public static bool IsUnob(this char c)
        {

            //UNOB allows:

            //All of UNOA
            //a to z
            //‘ + : ? ! ” % & * ; < >

            return IsUnoa(c) || (c >= 'a' && c <= 'z') || c == '\'' || c == '+' || c == ':' || c == '?' || c == '!' || c == '"' || c == '%' || c == '&' || c == '*' || c == ';' || c == '>' || c == '<';
        }

        public static bool IsUnoc(this char c)
        {

            //UNOC allows:

            //ISO 8859 character set

            var bytes = Iso88591.GetBytes(new char[] { c }, 0, 1);
            var result = Iso88591.GetChars(bytes);
            return result.GetLength(0) != 0 && Equals(c, result[0]);
        }

        public static bool IsUnod_Unok(this char c)
        {
            //UNOD-UNOK allows:

            //All of UNOA-UNOC
            //All of UTF8 characters

            return IsUnoc(c) || IsUtf8(c);
        }

        public static bool IsUtf8(this char c)
        {
            var bytes = Utf8.GetBytes(new char[] { c }, 0, 1);
            var result = Utf8.GetChars(bytes);
            return result.GetLength(0) != 0 && Equals(c, result[0]);
        }


        public static char ToUnoa(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;

            if (c.IsUnoa())
                return c;

            var normalized = c.Normalize();

            if (normalized.IsUnoa())
                return normalized;

            var upper = char.ToUpperInvariant(normalized);

            return upper.IsUnoa() ? upper : fallback;
        }

        public static char ToUnob(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;

            if (c.IsUnob())
                return c;

            var normalized = c.Normalize();

            return normalized.IsUnob() ? normalized : fallback;
        }

        public static char ToUnoc(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;


            if (c.IsUnoc())
                return c;

            var normalized = c.Normalize();

            return normalized.IsUnoc() ? normalized : fallback;
        }


        public static char ToUnod(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnoe(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnof(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnog(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnoh(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnoi(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnoj(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        public static char ToUnok(this char c, char fallback = ' ')
        {
            return c.ToUnod_Unok(fallback);
        }

        private static char ToUnod_Unok(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;


            if (c.IsUnod_Unok())
                return c;

            var normalized = c.Normalize();

            return normalized.IsUnod_Unok() ? normalized : fallback;
        }

        public static char Normalize(this char ch)
        {
            var s = ch.ToString();
            foreach (var c in s.Normalize(NormalizationForm.FormKD))
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        break;

                    default:
                        return c;
                }

            return ch;
        }


        public static char Translate(this char ch, EdifactCharacterSet charSet, char fallbackChar)
        {
            switch (charSet)
            {
                case EdifactCharacterSet.UNOA:
                    return ch.ToUnoa(fallbackChar);
                case EdifactCharacterSet.UNOB:
                    return ch.ToUnob(fallbackChar);
                case EdifactCharacterSet.UNOC:
                    return ch.ToUnoc(fallbackChar);
                case EdifactCharacterSet.UNOD:
                    return ch.ToUnoc(fallbackChar);
                case EdifactCharacterSet.UNOE:
                    return ch.ToUnoe(fallbackChar);
                case EdifactCharacterSet.UNOF:
                    return ch.ToUnof(fallbackChar);
                case EdifactCharacterSet.UNOG:
                    return ch.ToUnog(fallbackChar);
                case EdifactCharacterSet.UNOH:
                    return ch.ToUnoh(fallbackChar);
                case EdifactCharacterSet.UNOI:
                    return ch.ToUnoi(fallbackChar);
                case EdifactCharacterSet.UNOJ:
                    return ch.ToUnoj(fallbackChar);
                case EdifactCharacterSet.UNOK:
                    return ch.ToUnok(fallbackChar);
                case EdifactCharacterSet.UNOX:
                case EdifactCharacterSet.UNOY:
                case EdifactCharacterSet.KECA:
                    return ch;
                default:
                    return ch;
            }
        }


    }

}

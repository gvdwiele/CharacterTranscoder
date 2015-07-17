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
        private static Encoding ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");

        private static Encoding UTF8 = new UTF8Encoding();

        public static bool IsUNOA(this char c)
        {

            //UNOA allows:

            //A to Z
            //0 to 9
            //. , – ( ) / = (space)


            return (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '.' || c == ',' || c == '-' || c == '(' || c == ')' || c == '/' || c == '=' || c == ' ';
        }

        public static bool IsUNOB(this char c)
        {

            //UNOB allows:

            //All of UNOA
            //a to z
            //‘ + : ? ! ” % & * ; < >

            return IsUNOA(c) || (c >= 'a' && c <= 'z') || c == '\'' || c == '+' || c == ':' || c == '?' || c == '!' || c == '"' || c == '%' || c == '&' || c == '*' || c == ';' || c == '>' || c == '<';
        }

        public static bool IsUNOC(this char c)
        {

            //UNOC allows:

            //ISO 8859 character set

            byte[] bytes = ISO_8859_1.GetBytes(new char[] { c }, 0, 1);
            char[] result = ISO_8859_1.GetChars(bytes);
            if (result.GetLength(0) == 0)
                return false;
            return Char.Equals(c, result[0]);
        }

        public static bool IsUNOD_UNOK(this char c)
        {
            //UNOD-UNOK allows:

            //All of UNOA-UNOC
            //All of UTF8 characters

            return IsUNOC(c) || IsUTF8(c);
        }

        public static bool IsUTF8(this char c)
        {
            byte[] bytes = UTF8.GetBytes(new char[] { c }, 0, 1);
            char[] result = UTF8.GetChars(bytes);
            if (result.GetLength(0) == 0)
                return false;
            return Char.Equals(c, result[0]);
        }


        public static char ToUNOA(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;

            if (c.IsUNOA())
                return c;

            char normalized = c.Normalize();

            if (normalized.IsUNOA())
                return normalized;

            char upper = char.ToUpperInvariant(normalized);

            if (upper.IsUNOA())
            {
                return upper;
            }

            return fallback;

        }

        public static char ToUNOB(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;

            if (c.IsUNOB())
                return c;

            char normalized = c.Normalize();

            if (normalized.IsUNOB())
            {

                return normalized;
            }

            return fallback;

        }

        public static char ToUNOC(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;


            if (c.IsUNOC())
                return c;

            char normalized = c.Normalize();

            if (normalized.IsUNOC())
            {

                return normalized;
            }

            return fallback;

        }


        public static char ToUNOD(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOE(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOF(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOG(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOH(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOI(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOJ(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        public static char ToUNOK(this char c, char fallback = ' ')
        {
            return c.ToUNOD_UNOK(fallback);
        }

        private static char ToUNOD_UNOK(this char c, char fallback = ' ')
        {
            if (char.IsControl(c))
                return c;


            if (c.IsUNOD_UNOK())
                return c;

            char normalized = c.Normalize();

            if (normalized.IsUNOD_UNOK())
            {
                return normalized;
            }

            return fallback;

        }

        public static char Normalize(this char ch)
        {
            string s = ch.ToString();
            foreach (char c in s.Normalize(NormalizationForm.FormKD))
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
                    return ch.ToUNOA(fallbackChar);
                case EdifactCharacterSet.UNOB:
                    return ch.ToUNOB(fallbackChar);
                case EdifactCharacterSet.UNOC:
                    return ch.ToUNOC(fallbackChar);
                case EdifactCharacterSet.UNOD:
                    return ch.ToUNOC(fallbackChar);
                case EdifactCharacterSet.UNOE:
                    return ch.ToUNOE(fallbackChar);
                case EdifactCharacterSet.UNOF:
                    return ch.ToUNOF(fallbackChar);
                case EdifactCharacterSet.UNOG:
                    return ch.ToUNOG(fallbackChar);
                case EdifactCharacterSet.UNOH:
                    return ch.ToUNOH(fallbackChar);
                case EdifactCharacterSet.UNOI:
                    return ch.ToUNOI(fallbackChar);
                case EdifactCharacterSet.UNOJ:
                    return ch.ToUNOJ(fallbackChar);
                case EdifactCharacterSet.UNOK:
                    return ch.ToUNOK(fallbackChar);
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

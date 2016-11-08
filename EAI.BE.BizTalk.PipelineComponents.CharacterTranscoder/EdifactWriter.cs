using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAI.BE.BizTalk.Extensions;

namespace EAI.BE.BizTalk.PipelineComponents
{
   
    public class EdifactWriter : StreamReader
    {
        private readonly char _fallbackChar;

        bool _normalize;
        readonly EdifactCharacterSet _edifactCharacterSet;

        public EdifactWriter(Stream stream, Encoding encoding, EdifactCharacterSet edifactCharacterSet, char replacementCharacter, bool normalize = true)
            : base(stream, encoding)
        {
            this._fallbackChar = replacementCharacter;
            this._edifactCharacterSet = edifactCharacterSet;
            this._normalize = normalize;
        }

        public override int Peek()
        {
            int value = base.Peek();
            if (value == -1)
                return value;

            return Clean((char)value);
            
        }

        private char Clean(char ch)
        {
            switch (_edifactCharacterSet)
            {
                case EdifactCharacterSet.UNOA:
                    return ch.ToUnoa(_fallbackChar);
                case EdifactCharacterSet.UNOB:
                    return ch.ToUnob(_fallbackChar);
                case EdifactCharacterSet.UNOC:
                    return ch.ToUnoc(_fallbackChar);
                case EdifactCharacterSet.UNOD:
                    return ch.ToUnoc(_fallbackChar);
                case EdifactCharacterSet.UNOE:
                    return ch.ToUnoe(_fallbackChar);
                case EdifactCharacterSet.UNOF:
                    return ch.ToUnof(_fallbackChar);
                case EdifactCharacterSet.UNOG:
                    return ch.ToUnog(_fallbackChar);
                case EdifactCharacterSet.UNOH:
                    return ch.ToUnoh(_fallbackChar);
                case EdifactCharacterSet.UNOI:
                    return ch.ToUnoi(_fallbackChar);
                case EdifactCharacterSet.UNOJ:
                    return ch.ToUnoj(_fallbackChar);
                case EdifactCharacterSet.UNOK:
                    return ch.ToUnok(_fallbackChar);
                case EdifactCharacterSet.UNOX:
                case EdifactCharacterSet.UNOY:
                case EdifactCharacterSet.KECA:
                    return ch;
                default:
                    return ch;
            }
        }

        public override int Read()
        {
            int value = base.Read();
            if (value == -1)
                return value;

            return Clean((char)value);
        }

        public override string ReadToEnd()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var ch = this.Read();
                if (ch == -1)
                    break;
                sb.Append((char)ch);
            }
            return sb.ToString();
        }

        public override string ReadLine()
        {

            string input = base.ReadLine();

            if (input == null)
                return null;

            var sb = new StringBuilder();

            foreach (char c in input)
            {
                sb.Append(Clean(c));
            }

            return sb.ToString();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int readCount = base.Read(buffer, index, count);
            for (int i = index; i < readCount + index; i++)
            {

                buffer[i] = Clean(buffer[i]);

            }
            return readCount;
        }



    }
}

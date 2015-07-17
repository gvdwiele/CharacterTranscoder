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
    /// <summary>
    /// Reads EDIFACT seekeable streams and replaces unsupported characters for the given characterset.
    /// If the EDIFACT stream contains a UNO syntax definition (UNOA,UNOB,...) this characterset will be used.
    /// If no UNO syntax definition is present in the stream the charSet from the constructor will be used. 
    /// </summary>
    public class EdifactReader : StreamReader
    {
        private readonly char _fallbackChar;

        bool _normalize;
        EdifactCharacterSet _charSet;
        bool _removeControlChars;

        public EdifactCharacterSet CharSet
        {
            get { return _charSet; }
        }

        public EdifactReader(Stream stream, Encoding encoding, EdifactCharacterSet charSet, char fallbackChar, bool normalize = true, bool removeControlChars = false)
            : base(stream, encoding)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanSeek)
                throw new ArgumentNullException("stream must be seekeable");

            this._removeControlChars = removeControlChars;
            this._fallbackChar = fallbackChar;
            this._charSet = charSet;
            this._normalize = normalize;

            EdifactCharacterSet messageCharSet;
            if (TryReadEdiCharacterSet(stream, out messageCharSet, encoding))
            {
                this._charSet = messageCharSet;
            }

        }

        public override int Peek()
        {
            int value = base.Peek();
            if (value == -1)
                return value;

            char c = (char)value;

            if (_removeControlChars && char.IsControl(c))
                return Peek(); // recurse

            return c.Translate(_charSet, _fallbackChar);

        }

        

        public override int Read()
        {
            int value = base.Read();
            if (value == -1)
                return value;

            char c = (char)value;

            if (_removeControlChars && char.IsControl(c))
                return Read(); // recurse

            return c.Translate(_charSet, _fallbackChar);
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

            if (input == string.Empty)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (char c in input)
            {
                if (!(_removeControlChars && char.IsControl(c)))
                {
                    sb.Append(c.Translate(_charSet, _fallbackChar));
                }
            }

            var line = sb.ToString();

            if (line == string.Empty)
                return ReadLine(); //recurse

            return line;
        }

        //public override int Read(char[] buffer, int index, int count)
        //{

        //    int readCount = base.Read(buffer, index, count);
        //    for (int i = index; i < readCount + index; i++)
        //    {
        //        buffer[i] = Clean(buffer[i]);

        //    }
        //    return readCount;
        //}

        private static bool TryReadEdiCharacterSet(Stream stream, out EdifactCharacterSet edifactCharacterSet, Encoding encoding)
        {
            edifactCharacterSet = EdifactCharacterSet.UNOA;

            if (stream == null)
                throw new ArgumentNullException("stream");

            var position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var sr = new StreamReader(stream, encoding);
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    var unoIndex = line.IndexOf("UNO");

                    if (unoIndex == -1 || line.Length < unoIndex + 4)
                    {
                        return false;
                    }
                    var uno = line.Substring(unoIndex, 4);

                    if (Enum.TryParse<EdifactCharacterSet>(uno, out edifactCharacterSet))
                    {
                        return true;
                        //var regex = new Regex(Regex.Escape(charsetIn));
                        //line = regex.Replace(line, TargetCharacterSet.ToString(), 1);
                    }

                }
            }
            catch
            {
            }
            finally
            {
                stream.Seek(position, SeekOrigin.Begin);

            }

            return false;
        }

    }
}

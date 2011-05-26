using System.IO;
using System;
using System.Collections.Generic;

namespace EAI.BE.BizTalk.PipelineComponents
{
    /// <summary>
    /// WindowsNewLineStream is a stream decorator that converts Unix style newlines to Windows style.
    /// Compatible with UTF-8 or ANSI encoded streams (use TranscodingStream if required) 
    /// </summary>
    public class WindowsNewLineStream : Stream
    {
        private Stream _stream;
        private long _position = 0;

        private Queue<int> _buffer = new Queue<int>(1);
        private bool _bufferFilled = false;

        //CR = 0x0d = 13
        //LF = 0x0a = 10
        public const int CR = 13;
        public const int LF = 10;

        public WindowsNewLineStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this._stream = stream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Length
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }


        /// <summary>
        /// An implementation is free to return fewer bytes than requested even if the end of the stream has not been reached.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            int next, nextnext;

            do
            {
                if (_bufferFilled)
                {
                    next = _buffer.Dequeue();
                    if (next == -1)
                    {
                        return read;
                    }
                    _bufferFilled = _buffer.Count > 0;
                }
                else
                {
                    //effective read;
                    next = _stream.ReadByte();
                    if (next == -1)
                    {
                        return read;
                    }
                    if (next == CR)
                    {
                        //check if followed by LF
                        nextnext = _stream.ReadByte();
                        if (nextnext != LF)
                        {
                            _buffer.Enqueue(LF);
                        }
                        _buffer.Enqueue(nextnext);
                        _bufferFilled = true;
                    }
                    if (next == LF)
                    {
                        //LF with missing CR
                        next = CR;
                        _buffer.Enqueue(LF);
                        _bufferFilled = true;
                    }
                }

                buffer[offset + read] = (byte)next;
                read++;
                if (read == count)
                {
                    return read;
                }
            }
            while (true);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }


    }
}

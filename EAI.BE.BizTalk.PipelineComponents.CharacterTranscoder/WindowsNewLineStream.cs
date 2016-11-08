using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EAI.BE.BizTalk.PipelineComponents
{
    /// <summary>
    /// WindowsNewLineStream is a stream decorator that converts Unix style newlines to Windows style.
    /// Compatible with UTF-8 or ANSI encoded streams (use TranscodingStream if required) 
    /// </summary>
    public class WindowsNewLineStream : Stream
    {
        private readonly Stream _stream;
        private long _position = 0;

        private readonly Queue<int> _buffer = new Queue<int>(1);
        private bool _bufferFilled = false;
        private bool _isOpen = false;

        //CR = 0x0d = 13
        //LF = 0x0a = 10
        public const int CR = 13;
        public const int LF = 10;

        private static readonly TraceSwitch TraceSwitch = new TraceSwitch("WindowsNewLineStream tracing", "", TraceLevel.Warning.ToString());

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

        public override int ReadByte()
        {
            var buffer = new byte[]{0};
            var read = Read(buffer, 0, 1);
            return (read == 0 ? -1 : buffer[0]);
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
            var read = 0;

            Trace.WriteLineIf(TraceSwitch.TraceVerbose, "\nRead start.");
            Trace.WriteLineIf(TraceSwitch.TraceVerbose, string.Format("_bufferfilled={0} buffer={1} offset={2} count={3}",_bufferFilled.ToString(),buffer.Length,offset.ToString(),count.ToString()));

            do
            {
                int next;
                if (_bufferFilled)
                {
                    next = _buffer.Dequeue();
                    if (next == -1)
                    {
                        Trace.WriteLineIf(TraceSwitch.TraceVerbose, "Read exit 1: " + read);
                        _position += read;
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
                        Trace.WriteLineIf(TraceSwitch.TraceVerbose, "Read exit 2: " + read);
                        _position += read;
                        return read;
                    }
                    if (next == CR)
                    {
                        //check if followed by LF
                        var nextnext = _stream.ReadByte();
                        
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
                if (read != count) continue;
                Trace.WriteLineIf(TraceSwitch.TraceVerbose, "Read exit 3: " + read);
                _position += read;
                return read;
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

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.MemoryStream" /> class and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._isOpen = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}

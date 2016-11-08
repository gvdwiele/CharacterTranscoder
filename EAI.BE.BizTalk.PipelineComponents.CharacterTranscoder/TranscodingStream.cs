using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EAI.BE.BizTalk.PipelineComponents
{
    /// <summary>
    /// TranscodingStream class is a binary tranformation stream decorator. While reading the underlying bytestream is transcoded from a source to a target encoding, on the fly. There is no support for seeking nor writing.
    /// </summary>
    public class TranscodingStream : Stream
    {
        private readonly Stream _stream;
        private readonly StreamReader _reader;
        private readonly Encoder _encoder;
        private long _position = 0;

        private readonly Queue<byte> _buffer = new Queue<byte>(3);
        private bool _bufferFilled = false;

        private TranscodingStream(Stream stream, Encoding outEncoding) 
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (outEncoding == null)
                throw new ArgumentNullException("outEncoding");

            this._stream = stream;
            this._encoder = outEncoding.GetEncoder();
        }

        public TranscodingStream(Stream stream, Encoding outEncoding, Encoding inEncoding) : this(stream, outEncoding)
        {
            if (inEncoding == null)
                throw new ArgumentNullException("inEncoding");
         
            this._reader = new StreamReader(_stream, inEncoding);
        }

        public TranscodingStream(Stream stream, Encoding outEncoding, bool detectEncodingFromByteOrderMarks) : this(stream,outEncoding)
        {
            this._reader = detectEncodingFromByteOrderMarks == false ? new StreamReader(_stream, Encoding.UTF8) : new StreamReader(_stream, true);
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            //if (_position < 0)
            //    throw new InvalidOperationException();

            var written = 0;
            var open = offset + count > buffer.Length ? buffer.Length - offset : count;
            var position = 0;
            var write = 0;

            while (open > 0)
            {
                var maxSafeWrite = open / 4;
                var read = 0;
                char[] readBuffer;

                if(maxSafeWrite>0)
                {
                    readBuffer = new char[maxSafeWrite];
                    
                    read = _reader.Read(readBuffer, 0, maxSafeWrite);
                    
                    if (read == 0)
                        return written;

                    write = _encoder.GetBytes(readBuffer, 0, read, buffer, offset + position, true);
                    position += write;
                    _position += write;
                    written += write;
                    
                    if (read < maxSafeWrite)
                        return written;
                }
                else
                {
                    readBuffer = new char[1];

                    if (_bufferFilled == false)
                    {
                        read = _reader.Read(readBuffer, 0, 1);

                        if (read == 0)
                            return written;

                        byte[] writeBuffer = new byte[_encoder.GetByteCount(readBuffer, 0, 1, false)];

                        _encoder.GetBytes(readBuffer, 0, 1, writeBuffer, 0, true);

                        if (writeBuffer.Length > open)
                        {
                            //more bytes are read from source than available in the output buffer 
                            write = open;
                            for (var i = 0; i < writeBuffer.Length - open; i++)
                            {
                                _buffer.Enqueue(writeBuffer[i + open]);
                            }
                            _bufferFilled = true;
                        }
                        else
                        {
                            write = writeBuffer.Length;
                        }

                        for (var i = 0; i < write; i++)
                        {
                            if (offset + position > buffer.GetUpperBound(0))
                                return written;

                            buffer[offset + position] = writeBuffer[i];
                            position += 1;
                            _position += 1;
                            written += 1;

                        }

                    }
                    else
                    {
                        
                        if (_bufferFilled)
                        {
                            buffer[offset + position] = _buffer.Dequeue();
                            _bufferFilled = _buffer.Count > 0;
                            
                            position += 1;
                            _position += 1;
                            written += 1;
                            write = 1;
                        }
                        
                    }
                    
                }

                open = open - write;
            }

            return written;
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

        // base close will call dispose below, so inner stream will effectively be close (streamreader closes stream inside dispose)
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _reader.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
} 

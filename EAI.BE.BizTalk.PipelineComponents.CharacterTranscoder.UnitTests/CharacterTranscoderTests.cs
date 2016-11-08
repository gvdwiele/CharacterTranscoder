using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using EAI.BE.BizTalk.PipelineComponents;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder.UnitTests
{
    [TestClass]
    public class CharacterTranscoderTests
    {

        [TestMethod]
        public void MultiByteCharactersInSourceStreamDoNotGetLostStreaming()
        {
            const string value = "Øþ";
            var bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var expected = new byte[] { 0xD8, 0xFE };

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.GetEncoding(1252), Encoding.UTF8);


            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), expected));
        }

        [TestMethod]
        public void EmptyWrite()
        {
            const string value = "";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  expected = new byte[] { 0x41};
            var  output = new byte[9];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.GetEncoding(1252), Encoding.UTF8);

            var count = after.Read(output, 0, 1);

            Assert.AreEqual(0, after.Read(output, 0, 1));
        }

        [TestMethod]
        public void SourceStreamIsShorterThanDestinationStream()
        {
            const string value = "A";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  expected = new byte[100]; 
            expected[0] = 0x41;
            var  output = new byte[100];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.GetEncoding(1252), Encoding.UTF8);

            var count = after.Read(output, 0, 100);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod]
        public void NoFullRead()
        {
            const string value = "AAA";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  expected = new byte[] { 0x41, 0x41};
            var  output = new byte[2];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.UTF8, Encoding.UTF8);

            var count = after.Read(output, 0, 2);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod]
        public void NoFullRead2()
        {
            const string value = "BAA";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  expected = new byte[] { 0x00, 0x42 };
            var  output = new byte[2];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.UTF8, Encoding.UTF8);

            var count = after.Read(output, 1, 1);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void IdisposableTest()
        {
            const string value = "BAA";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);

            Stream inStream = new MemoryStream(bytesUtf8);


            using (Stream after = new TranscodingStream(inStream, Encoding.UTF8, Encoding.UTF8))
            {
                while (after.ReadByte() != -1)
                { }
            }

            var length = inStream.Length;
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void IdisposableTest2()
        {
            const string value = "BAA";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);

            Stream inStream = new MemoryStream(bytesUtf8);

            Stream after = new TranscodingStream(inStream, Encoding.UTF8, Encoding.UTF8);
            after.Close();

            var length = inStream.Length;
        }

        [TestMethod]
        public void Decode_UTF32_And_Encode_1252()
        {
            const string value = "AØþ";
            var  bytesUtf32 = Encoding.UTF32.GetBytes(value);
            var  bytes1252 = Encoding.GetEncoding(1252).GetBytes(value);
            var  output = new byte[bytes1252.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf32), Encoding.GetEncoding(1252), Encoding.UTF32);

            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), bytes1252));
        }

        [TestMethod]
        public void Decode_1252_And_Encode_UTF32()
        {
            const string value = "AØþ";
            var  bytesUtf32 = Encoding.UTF32.GetBytes(value);
            var  bytes1252 = Encoding.GetEncoding(1252).GetBytes(value);
            var  output = new byte[bytesUtf32.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytes1252), Encoding.UTF32, Encoding.GetEncoding(1252));

            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), bytesUtf32));
        }


        [TestMethod]
        public void Decode_UTF8_And_Encode_UTF16()
        {
            const string value = "ABCDEFGHØþ";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  bytesUtf16 = Encoding.Unicode.GetBytes(value);
            var  output = new byte[bytesUtf16.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.Unicode, Encoding.UTF8);
            var count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUtf16));
        }

        [TestMethod]
        public void Decode_And_Encode_Same_Encoding_UTF16()
        {
            const string value = "ABCDEFGHØþ";
            var  bytesUtf16 = Encoding.Unicode.GetBytes(value);
            var  output = new byte[bytesUtf16.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf16), Encoding.Unicode, Encoding.Unicode);
            var count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUtf16));
        }

        [TestMethod]
        public void Decode_And_Encode_Same_Encoding_UTF8()
        {
            const string value = "ABCDEFGHØþ";
            var  bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var  output = new byte[bytesUtf8.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUtf8), Encoding.UTF8, Encoding.UTF8);
            var count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUtf8));
        }

        private static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            return !a1.Where((t, i) => t != a2[i]).Any();
        }

        public static MemoryStream Fix3F(Stream source)
        {
            var target = new MemoryStream();
            var targetEncoding = Encoding.GetEncoding(1252);

            var sourceReader = new StreamReader(source, Encoding.UTF8, false);
            var targetWriter = new StreamWriter(target, targetEncoding);

            var charRead = sourceReader.Read();
            while (charRead != -1)
            {
                targetWriter.Write((char)charRead);
                charRead = sourceReader.Read();
            }
            targetWriter.Flush();
            target.Seek(0, SeekOrigin.Begin);
            return target;
        }

        private static byte[] StreamToArray(Stream bytes)
        {
            var read = bytes.ReadByte();
            var listOfBytes = new List<byte>();
            while (read != -1)
            {
                listOfBytes.Add((byte)read);
                read = bytes.ReadByte();
            }
            var  output = listOfBytes.ToArray();
            
            return output;
        }
    }
}

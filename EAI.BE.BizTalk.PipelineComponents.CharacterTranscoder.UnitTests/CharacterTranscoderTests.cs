using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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
            string value = "Øþ";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] expected = new byte[] { 0xD8, 0xFE };

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.GetEncoding(1252), Encoding.UTF8);


            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), expected));
        }

        [TestMethod]
        public void EmptyWrite()
        {
            string value = "";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] expected = new byte[] { 0x41};
            byte[] output = new byte[9];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.GetEncoding(1252), Encoding.UTF8);

            int count = after.Read(output, 0, 1);

            Assert.AreEqual(0, after.Read(output, 0, 1));
        }

        [TestMethod]
        public void SourceStreamIsShorterThanDestinationStream()
        {
            string value = "A";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] expected = new byte[100]; 
            expected[0] = 0x41;
            byte[] output = new byte[100];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.GetEncoding(1252), Encoding.UTF8);

            int count = after.Read(output, 0, 100);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod]
        public void NoFullRead()
        {
            string value = "AAA";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] expected = new byte[] { 0x41, 0x41};
            byte[] output = new byte[2];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.UTF8, Encoding.UTF8);

            int count = after.Read(output, 0, 2);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod]
        public void NoFullRead2()
        {
            string value = "BAA";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] expected = new byte[] { 0x00, 0x42 };
            byte[] output = new byte[2];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.UTF8, Encoding.UTF8);

            int count = after.Read(output, 1, 1);

            Assert.IsTrue(ByteArrayCompare(output, expected));
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void IdisposableTest()
        {
            string value = "BAA";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);

            Stream inStream = new MemoryStream(bytesUTF8);


            using (Stream after = new TranscodingStream(inStream, Encoding.UTF8, Encoding.UTF8))
            {
                while (after.ReadByte() != -1)
                { }
            }

            long length = inStream.Length;
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void IdisposableTest2()
        {
            string value = "BAA";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);

            Stream inStream = new MemoryStream(bytesUTF8);

            Stream after = new TranscodingStream(inStream, Encoding.UTF8, Encoding.UTF8);
            after.Close();

            long length = inStream.Length;
        }

        [TestMethod]
        public void Decode_UTF32_And_Encode_1252()
        {
            string value = "AØþ";
            byte[] bytesUTF32 = Encoding.UTF32.GetBytes(value);
            byte[] bytes1252 = Encoding.GetEncoding(1252).GetBytes(value);
            byte[] output = new byte[bytes1252.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF32), Encoding.GetEncoding(1252), Encoding.UTF32);

            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), bytes1252));
        }

        [TestMethod]
        public void Decode_1252_And_Encode_UTF32()
        {
            string value = "AØþ";
            byte[] bytesUTF32 = Encoding.UTF32.GetBytes(value);
            byte[] bytes1252 = Encoding.GetEncoding(1252).GetBytes(value);
            byte[] output = new byte[bytesUTF32.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytes1252), Encoding.UTF32, Encoding.GetEncoding(1252));

            Assert.IsTrue(ByteArrayCompare(StreamToArray(after), bytesUTF32));
        }


        [TestMethod]
        public void Decode_UTF8_And_Encode_UTF16()
        {
            string value = "ABCDEFGHØþ";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] bytesUTF16 = Encoding.Unicode.GetBytes(value);
            byte[] output = new byte[bytesUTF16.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.Unicode, Encoding.UTF8);
            int count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUTF16));
        }

        [TestMethod]
        public void Decode_And_Encode_Same_Encoding_UTF16()
        {
            string value = "ABCDEFGHØþ";
            byte[] bytesUTF16 = Encoding.Unicode.GetBytes(value);
            byte[] output = new byte[bytesUTF16.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF16), Encoding.Unicode, Encoding.Unicode);
            int count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUTF16));
        }

        [TestMethod]
        public void Decode_And_Encode_Same_Encoding_UTF8()
        {
            string value = "ABCDEFGHØþ";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] output = new byte[bytesUTF8.Length];

            Stream after = new TranscodingStream(new MemoryStream(bytesUTF8), Encoding.UTF8, Encoding.UTF8);
            int count = after.Read(output, 0, 40);

            Assert.IsTrue(ByteArrayCompare(output, bytesUTF8));
        }

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        public static MemoryStream Fix3F(Stream source)
        {
            MemoryStream target = new MemoryStream();
            Encoding targetEncoding = Encoding.GetEncoding(1252);

            StreamReader sourceReader = new StreamReader(source, Encoding.UTF8, false);
            StreamWriter targetWriter = new StreamWriter(target, targetEncoding);

            int charRead = sourceReader.Read();
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
            int read = bytes.ReadByte();
            List<byte> listOfBytes = new List<byte>();
            while (read != -1)
            {
                listOfBytes.Add((byte)read);
                read = bytes.ReadByte();
            }
            byte[] output = listOfBytes.ToArray();
            
            return output;
        }
    }
}

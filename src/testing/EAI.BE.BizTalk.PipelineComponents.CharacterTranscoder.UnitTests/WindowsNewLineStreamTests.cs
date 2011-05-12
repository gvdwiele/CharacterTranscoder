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
    public class WindowsNewLineStreamTests
    {
        public const int A = 65;
        public const int B = 66;
        public const int C = 67;

        [TestMethod]
        public void GradualRead()
        {
            string value = "ABC";
            byte[] bytesUTF8 = Encoding.UTF8.GetBytes(value);
            byte[] output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(bytesUTF8));

            int count;
            
            count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1,count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, 0, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, 0 }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, C }));

        }

        [TestMethod]
        public void WhenEOSorEMptyStreamReturnMinusOne()
        {
            byte[] output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[]{}));

            int count;

            count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(-1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { 0, 0, 0 }));
        }

        [TestMethod]
        public void InsertLFWhenMissing()
        {
            byte[] output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { WindowsNewLineStream.CR, WindowsNewLineStreamTests.A }));

            int count;

            count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, 0, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, 0 }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, WindowsNewLineStreamTests.A }));

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

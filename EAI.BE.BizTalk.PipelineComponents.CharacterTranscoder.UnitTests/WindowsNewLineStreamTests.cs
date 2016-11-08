using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EAI.BE.BizTalk.PipelineComponents;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder.TestArtifacts;
using WinterdomTesting = Winterdom.BizTalk.PipelineTesting;
using Microsoft.BizTalk.Streaming;

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
            const string value = "ABC";
            var bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(bytesUtf8));

            var count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, 0, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, 0 }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, C }));

        }


        [TestMethod]
        public void ReadMultipleBytes()
        {
            const string value = "ABC";
            var bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(bytesUtf8));

            var count = after.Read(output, 0, 3);
            Assert.AreEqual<int>(3, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, C }));

        }


        [TestMethod]
        public void TryReadBeyondEos()
        {
            const string value = "ABC";
            var bytesUtf8 = Encoding.UTF8.GetBytes(value);
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(bytesUtf8));

            var count = after.Read(output, 0, 5);
            Assert.AreEqual<int>(3, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, C }));
            count = after.Read(output, 0, 5);
            Assert.AreEqual<int>(0, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { A, B, C }));
        }

        [TestMethod]
        public void WhenEosOrEmptyStreamReturnMinusOne()
        {
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { }));

            var count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(0, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { 0, 0, 0 }));
        }


        [TestMethod]
        public void InsertLfWhenMissing()
        {
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { WindowsNewLineStream.CR, WindowsNewLineStreamTests.A }));

            var count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, 0, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, 0 }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, WindowsNewLineStreamTests.A }));

        }

        [TestMethod]
        public void PassthruWindowsNewLine()
        {
            var output = new byte[3];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, WindowsNewLineStreamTests.A }));

            var count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, 0, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, 0 }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, WindowsNewLineStreamTests.A }));

        }

        [TestMethod]
        public void CarriageReturnAtEos()
        {
            var output = new byte[2];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { WindowsNewLineStream.CR }));

            var count = after.Read(output, 0, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, 0 }));

            count = after.Read(output, 1, 1);
            Assert.AreEqual<int>(1, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF }));

            count = after.Read(output, 2, 1);
            Assert.AreEqual<int>(0, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF }));

        }

        [TestMethod]
        public void InsertCrWhenMissing()
        {
            var output = new byte[4];

            Stream after = new WindowsNewLineStream(new MemoryStream(new byte[] { WindowsNewLineStream.LF, WindowsNewLineStream.LF }));

            var count = after.Read(output, 0, 4);
            Assert.AreEqual<int>(4, count);
            Assert.IsTrue(ByteArrayCompare(output, new byte[] { WindowsNewLineStream.CR, WindowsNewLineStream.LF, WindowsNewLineStream.CR, WindowsNewLineStream.LF }));
        }

        [TestMethod]
        public void IntegrationTestExecutingAndParsingFromPipeline()
        {
            //Microsoft.BizTalk.ParsingEngine.AbortException
            var input = DocLoader.LoadStream("samples.ok.txt");
            var msg = MessageHelper.CreateFromStream(input);
            var pipeline = WinterdomTesting.PipelineFactory.CreateReceivePipeline(typeof(ReceivePipelineToWindowsNewLineFF));
            pipeline.AddDocSpec(typeof(FlatFileManifest));
            var col = pipeline.Execute(msg);
            Assert.AreEqual(1, col.Count);
        }

        [TestMethod, ExpectedException(typeof(System.Xml.XmlException))]
        public void IntegrationTestExecutingAndParsingFromPipelineFailure()
        {
            //Microsoft.BizTalk.ParsingEngine.AbortException
            var input = DocLoader.LoadStream("samples.nok.txt");
            var msg = MessageHelper.CreateFromStream(input);
            var pipeline = WinterdomTesting.PipelineFactory.CreateReceivePipeline(typeof(ReceivePipelineToWindowsNewLineFF));
            pipeline.AddDocSpec(typeof(FlatFileManifest));
            var col = pipeline.Execute(msg);
            Assert.AreEqual(1, col.Count);
        }

        [TestMethod]
        public void IntegrationTestExecutingFromPipeline()
        {
            var input = DocLoader.LoadStream("samples.ok.txt");
            var msg = MessageHelper.CreateFromStream(input);
            var pipeline = WinterdomTesting.PipelineFactory.CreateReceivePipeline(typeof(ReceivePipelineToWindowsNewLine));
            pipeline.AddDocSpec(typeof(FlatFileManifest));
            var col = pipeline.Execute(msg);
            Assert.AreEqual(1, col.Count);
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
            var output = listOfBytes.ToArray();

            return output;
        }
    }
}

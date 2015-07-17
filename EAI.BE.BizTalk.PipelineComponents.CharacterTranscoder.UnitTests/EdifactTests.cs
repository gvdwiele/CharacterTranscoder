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
using EAI.BE.BizTalk.Extensions;

namespace EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder.UnitTests
{
    [TestClass]
    public class EdifactTests
    {
        private static Encoding ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");

        private static Encoding UTF8 = new UTF8Encoding();

        private static Encoding ASCII = new ASCIIEncoding();


        private static Encoding Windows1252 = Encoding.GetEncoding(1252);


        [TestMethod]
        public void EdifactReader_UNOA_Default()
        {
            string value = "ABC@@@";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.'))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();

            Assert.AreEqual<string>("ABC...", result);

        }

        [TestMethod]
        public void EdifactReader_UNOA_ToUpper()
        {
            string value = "ABCabc";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.'))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }
            
            }

            var result = sb.ToString();

            Assert.AreEqual<string>("ABCABC", result);

        }

        [TestMethod]
        public void EdifactReader_UNOA_ToUpperSpecial()
        {
            string value = "ABCábc";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.'))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();

            Assert.AreEqual<string>("ABCABC", result);

        }

        [TestMethod]
        public void EdifactReader_UNOB_Default()
        {
            string value = "ABCabc";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();


            Assert.AreEqual<string>("ABCabc", result);

        }

        [TestMethod]
        public void EdifactReader_UNOB_Replacement()
        {
            string value = "ABC{}";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();


            Assert.AreEqual<string>("ABC..", result);

        }


        [TestMethod]
        public void EdifactReader_UNOB_Normalize()
        {
            string value = "ABCúíó";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();


            Assert.AreEqual<string>("ABCuio", result);

        }



        [TestMethod]
        public void EdifactReader_UNOC_Default()
        {
            string value = "ABCúíó";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOC, '.', true))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();


            Assert.AreEqual<string>("ABCúíó", result);

        }

         [TestMethod]
        public void EdifactReader_UNOC_Replacement()
        {

             //only in UTF8 so not in UNOC
            //Ђ 	CYRILLIC CAPITAL LETTER DJE (U+0402) 	d082

            string value = "ABCЂ";
            byte[] bytes = UTF8.GetBytes(value);
            byte[] output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true))
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sb.Append((char)c);
                }

            }

            var result = sb.ToString();


            Assert.AreEqual<string>("ABC.", result);

        }


         [TestMethod]
         public void EdifactReader_RemoveControlChars()
         {
             string value = "A\aB\bCDEF\fGHIJKLMON\nPQR\rST\tUV\vWXYZ";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true, true))
             {
                 int c;
                 while ((c = sr.Read()) != -1)
                 {
                     sb.Append((char)c);
                 }

             }

             var result = sb.ToString();

             Assert.AreEqual<string>("ABCDEFGHIJKLMONPQRSTUVWXYZ", result);

         }


         [TestMethod]
         public void EdifactReader_DontRemoveControlChars()
         {
             string value = "A\aB\bCDEF\fGHIJKLMON\nPQR\rST\tUV\vWXYZ";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true, false))
             {
                 int c;
                 while ((c = sr.Read()) != -1)
                 {
                     sb.Append((char)c);
                 }

             }

             var result = sb.ToString();

             Assert.AreEqual<string>("A\aB\bCDEF\fGHIJKLMON\nPQR\rST\tUV\vWXYZ", result);

         }

         [TestMethod]
         public void EdifactReader_UNOA_ReadLine()
         {
             string value = "ABC@@@";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];


             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.'))
             {
                 int c;
                 while ((c = sr.Read()) != -1)
                 {
                     sb.Append((char)c);
                 }

             }

             var resultRead = sb.ToString();

             var sr2 = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.');
             var resultReadLine = sr2.ReadLine();

             Assert.AreEqual<string>("ABC...", resultRead);
             Assert.AreEqual<string>("ABC...", resultReadLine);

         }


         [TestMethod]
         public void EdifactReader_UNOA_ReadLine_WithNewLine()
         {
             string value = @"ABC@@@
XYZ@@@";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.');
             var result = sr.ReadLine();

             Assert.AreEqual<string>("ABC...", result);

             result = sr.ReadLine();

             Assert.AreEqual<string>("XYZ...", result);

         }


         [TestMethod]
         public void EdifactReader_UNOA_ReadLine_WithEmptyNewLine()
         {
             string value = @"ABC@@@

XYZ@@@";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.');
             var result = sr.ReadLine();

             Assert.AreEqual<string>("ABC...", result);

             result = sr.ReadLine();

             Assert.AreEqual<string>("", result);

             result = sr.ReadLine();

             Assert.AreEqual<string>("XYZ...", result);

         }

         [TestMethod]
         public void EdifactReader_UNOA_ReadToEnd_WithEmptyNewLine()
         {
             string value = @"ABC@@@

XYZ@@@";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOA, '.');
             var result = sr.ReadToEnd();

             Assert.AreEqual<string>(@"ABC...

XYZ...", result);

      
         }


         [TestMethod]
         public void EdifactReader_Replacement()
         {
             string value = "ABC{}";
             byte[] bytes = UTF8.GetBytes(value);
             byte[] output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOB, '.', true))
             {
                 int c;
                 while ((c = sr.Read()) != -1)
                 {
                     sb.Append((char)c);
                 }

             }

             var result = sb.ToString();


             Assert.AreEqual<string>("ABC..", result);

         }


         [TestMethod]
         public void EdifactReader_ReadCharacterSetFromStream()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1.txt");
             var sr = new EdifactReader(input, UTF8, EdifactCharacterSet.UNOA, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOB);
         }

         [TestMethod]
         public void EdifactReader_CharacterSetMissingFromStream_Fallback()
         {
             string value = @"dummy";
             byte[] bytes = UTF8.GetBytes(value);
             var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOX, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOX);
         }


         [TestMethod]
         public void EdifactReader_ReadCharacterFromShortStream()
         {
             string value = @"UNOD";
             byte[] bytes = UTF8.GetBytes(value);
             var sr = new EdifactReader(new MemoryStream(bytes), UTF8, EdifactCharacterSet.UNOX, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOD);
         }

         [TestMethod]
         public void EdifactCleaner_DefaultRun()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1.txt");
             IBaseMessage msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg); 
             Assert.AreEqual(1, result.Count);

             var expected = new String(UTF8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1.txt"))));
             var after = new String(UTF8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

         }


         [TestMethod]
         public void EdifactXmlCleaner_UNOBRun()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1.xml");
             IBaseMessage msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactXmlCleaner();
             cleaner.TargetCharSet = EdifactCharacterSet.UNOB;
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Send().WithPreAssembler(cleaner).End().Execute(msg);
             
             var expected = new String(UTF8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1_cleaned.xml"))));
             var after = new String(UTF8.GetChars(StreamToArray(result.BodyPart.GetOriginalDataStream())));

             string diff;
             bool equals = CompareStrings(expected, after, out diff);

             Assert.AreEqual(string.Empty, diff);
         }

         private static bool CompareStrings(string expected, string after, out string diff)
         {
             var expectedChars = expected.ToCharArray();
             var afterChars = after.ToCharArray();

             var sb = new StringBuilder();
             for (int i = 0; i < expectedChars.Length; i++)
             {
                 if (i > afterChars.GetUpperBound(0))
                 {
                     sb.AppendLine("Position " + i + ": expected " + expectedChars[i] + ", is missing");
                     continue;
                 }
                 if (after[i] != expectedChars[i])
                     sb.AppendLine("Position " + i + ": expected " + expectedChars[i] + ", is " + after[i]);
             }

             diff = sb.ToString();

             bool equals = (string.Empty == sb.ToString());
             return equals;
         }


         [TestMethod]
         public void EdifactCleaner_OverrideCharSet()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1.txt");
             IBaseMessage msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             cleaner.OverrideCharSet = true;
             cleaner.TargetCharSet = EdifactCharacterSet.UNOC;
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             var expected = "UNOC";
             var after = new String(UTF8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after.Substring(5,4));

         }

         [TestMethod]
         public void EdifactCleaner_RemoveControlChars()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1_with_control_chars.txt");
             IBaseMessage msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             cleaner.RemoveControlChars = true;
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             var expected = new String(UTF8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1.txt"))));
             var after = new String(UTF8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

         }

         [TestMethod]
         public void EdifactCleaner_DoesntRemoveControlChars()
         {
             Stream input = DocLoader.LoadStream("samples.edifact1_with_control_chars.txt");
             IBaseMessage msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             var expected = new String(UTF8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1_with_control_chars.txt"))));
             var after = new String(UTF8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

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

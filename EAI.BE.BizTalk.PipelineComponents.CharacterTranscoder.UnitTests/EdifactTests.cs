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
        private static Encoding _iso88591 = Encoding.GetEncoding("ISO-8859-1");

        private static readonly Encoding Utf8 = new UTF8Encoding();

        private static Encoding _windows1252 = Encoding.GetEncoding(1252);


        [TestMethod]
        public void EdifactReader_UNOA_Default()
        {
            const string value = "ABC@@@";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.'))
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
            const string value = "ABCabc";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.'))
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
            const string value = "ABCábc";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];


            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.'))
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
            const string value = "ABCabc";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true))
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
            const string value = "ABC{}";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true))
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
            const string value = "ABCúíó";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true))
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
            const string value = "ABCúíó";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOC, '.', true))
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

            const string value = "ABCЂ";
            var bytes = Utf8.GetBytes(value);
            var output = new byte[3];

            var sb = new StringBuilder();

            using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true))
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
             const string value = "A\aB\bCDEF\fGHIJKLMON\nPQR\rST\tUV\vWXYZ";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true, true))
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
             const string value = "A\aB\bCDEF\fGHIJKLMON\nPQR\rST\tUV\vWXYZ";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true, false))
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
             const string value = "ABC@@@";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];


             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.'))
             {
                 int c;
                 while ((c = sr.Read()) != -1)
                 {
                     sb.Append((char)c);
                 }

             }

             var resultRead = sb.ToString();

             var sr2 = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.');
             var resultReadLine = sr2.ReadLine();

             Assert.AreEqual<string>("ABC...", resultRead);
             Assert.AreEqual<string>("ABC...", resultReadLine);

         }


         [TestMethod]
         public void EdifactReader_UNOA_ReadLine_WithNewLine()
         {
             const string value = @"ABC@@@
XYZ@@@";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.');
             var result = sr.ReadLine();

             Assert.AreEqual<string>("ABC...", result);

             result = sr.ReadLine();

             Assert.AreEqual<string>("XYZ...", result);

         }


         [TestMethod]
         public void EdifactReader_UNOA_ReadLine_WithEmptyNewLine()
         {
             const string value = @"ABC@@@

XYZ@@@";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.');
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
             const string value = @"ABC@@@

XYZ@@@";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];


             var sb = new StringBuilder();

             var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOA, '.');
             var result = sr.ReadToEnd();

             Assert.AreEqual<string>(@"ABC...

XYZ...", result);

      
         }


         [TestMethod]
         public void EdifactReader_Replacement()
         {
             const string value = "ABC{}";
             var bytes = Utf8.GetBytes(value);
             var output = new byte[3];

             var sb = new StringBuilder();

             using (var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOB, '.', true))
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
             var input = DocLoader.LoadStream("samples.edifact1.txt");
             var sr = new EdifactReader(input, Utf8, EdifactCharacterSet.UNOA, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOB);
         }

         [TestMethod]
         public void EdifactReader_CharacterSetMissingFromStream_Fallback()
         {
             const string value = @"dummy";
             var bytes = Utf8.GetBytes(value);
             var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOX, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOX);
         }


         [TestMethod]
         public void EdifactReader_ReadCharacterFromShortStream()
         {
             const string value = @"UNOD";
             var bytes = Utf8.GetBytes(value);
             var sr = new EdifactReader(new MemoryStream(bytes), Utf8, EdifactCharacterSet.UNOX, '.');
             Assert.AreEqual(sr.CharSet, EdifactCharacterSet.UNOD);
         }

         [TestMethod]
         public void EdifactCleaner_DefaultRun()
         {
             var input = DocLoader.LoadStream("samples.edifact1.txt");
             var msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg); 
             Assert.AreEqual(1, result.Count);

             var expected = new string(Utf8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1.txt"))));
             var after = new string(Utf8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

         }


         [TestMethod]
         public void EdifactXmlCleaner_UNOBRun()
         {
             var input = DocLoader.LoadStream("samples.edifact1.xml");
             var msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactXmlCleaner {TargetCharSet = EdifactCharacterSet.UNOB};
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Send().WithPreAssembler(cleaner).End().Execute(msg);
             
             var expected = new string(Utf8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1_cleaned.xml"))));
             var after = new string(Utf8.GetChars(StreamToArray(result.BodyPart.GetOriginalDataStream())));

             string diff;
             var equals = CompareStrings(expected, after, out diff);

             Assert.AreEqual(string.Empty, diff);
         }

         [TestMethod]
         public void EdifactXmlCleaner_Separators()
         {
             var msg = MessageHelper.CreateFromString("﻿<ROOT>Returns everything except the ! and the ?</ROOT>");
             var cleaner = new EdifactXmlCleaner { TargetCharSet = EdifactCharacterSet.UNOC , Separators = "?!", FallbackChar = '*'};
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Send().WithPreAssembler(cleaner).End().Execute(msg);

             const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ROOT>Returns everything except the * and the *</ROOT>";
             string after;

             using (var reader = new StreamReader(result.BodyPart.GetOriginalDataStream(), Encoding.UTF8))
             {
                 after = reader.ReadToEnd();
             }


             string diff;
             var equals = CompareStrings(expected, after, out diff);

             Assert.AreEqual(string.Empty, diff);
         }

         private static bool CompareStrings(string expected, string after, out string diff)
         {
             var expectedChars = expected.ToCharArray();
             var afterChars = after.ToCharArray();

             var sb = new StringBuilder();
             for (var i = 0; i < expectedChars.Length; i++)
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

             var equals = (string.Empty == sb.ToString());
             return equals;
         }


         [TestMethod]
         public void EdifactCleaner_OverrideCharSet()
         {
             var input = DocLoader.LoadStream("samples.edifact1.txt");
             var msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner
             {
                 OverrideCharSet = true,
                 TargetCharSet = EdifactCharacterSet.UNOC
             };
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             const string expected = "UNOC";
             var after = new string(Utf8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after.Substring(5,4));

         }

         [TestMethod]
         public void EdifactCleaner_RemoveControlChars()
         {
             var input = DocLoader.LoadStream("samples.edifact1_with_control_chars.txt");
             var msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner {RemoveControlChars = true};
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             var expected = new string(Utf8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1.txt"))));
             var after = new string(Utf8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

         }

         [TestMethod]
         public void EdifactCleaner_DoesntRemoveControlChars()
         {
             var input = DocLoader.LoadStream("samples.edifact1_with_control_chars.txt");
             var msg = MessageHelper.CreateFromStream(input);
             var cleaner = new EdifactCleaner();
             var result = Winterdom.BizTalk.PipelineTesting.Simple.Pipelines.Receive().WithDecoder(cleaner).End().Execute(msg);
             Assert.AreEqual(1, result.Count);

             var expected = new string(Utf8.GetChars(StreamToArray(DocLoader.LoadStream("samples.edifact1_with_control_chars.txt"))));
             var after = new string(Utf8.GetChars(StreamToArray(result[0].BodyPart.GetOriginalDataStream())));

             Assert.AreEqual(expected, after);

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

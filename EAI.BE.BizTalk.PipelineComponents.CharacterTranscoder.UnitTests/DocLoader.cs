using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder.UnitTests
{
    internal static class DocLoader
    {
        /// <summary>
        /// Loads a document instance from a resource
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <returns></returns>
        public static Stream LoadStream(string name)
        {
            string resName = typeof(DocLoader).Namespace + "." + name;
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resName);
        }

        public static void ExtractToDir(string name, string dir)
        {
            string fullname = Path.Combine(dir, name);
            using (Stream source = LoadStream(name))
            using (Stream target = File.Create(fullname))
                CopyStream(source, target);
        }

        private static void CopyStream(Stream source, Stream target)
        {
            byte[] buffer = new byte[4096];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                target.Write(buffer, 0, read);
        }

    }
}

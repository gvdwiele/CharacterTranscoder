namespace EAI.BE.BizTalk.PipelineComponents
{
    using System;
    using System.IO;
    using System.Text;
    using System.Drawing;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using Microsoft.BizTalk.Message.Interop;
    using Microsoft.BizTalk.Component.Interop;
    using Microsoft.BizTalk.Component;
    using Microsoft.BizTalk.Messaging;
    using System.Collections.Generic;
    using Winterdom.BizTalk.Samples.FixEncoding.Design;

    using Microsoft.XLANGs.BaseTypes;
    using System.Text.RegularExpressions;
    using Microsoft.BizTalk.Streaming;




    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [System.Runtime.InteropServices.Guid("56FCE4CB-75A3-4877-8200-7BB1636A129F")]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    public class EdifactCleaner : Microsoft.BizTalk.Component.Interop.IComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
    {
        private readonly System.Resources.ResourceManager _resourceManager = new System.Resources.ResourceManager("EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder", Assembly.GetExecutingAssembly());

        private Encoding _encoding = Encoding.UTF8;

        [Editor(typeof(EncodingTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(EncodingTypeConverter))]
        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        private EdifactCharacterSet _targetCharSet = EdifactCharacterSet.UNOC;

        [Browsable(true)]
        [DisplayName("Character Set")]
        public EdifactCharacterSet TargetCharSet
        {
            get
            {
                return _targetCharSet;
            }
            set
            {
                _targetCharSet = value;
            }
        }


        char _fallbackChar = ' ';

        [Browsable(true)]
        [DisplayName("Fallback character")]
        public char FallbackChar
        {
            get { return _fallbackChar; }
            set { _fallbackChar = value; }
        }

        [Browsable(true)]
        [DisplayName("Normalize invalid characters")]
        public bool Normalize { get; set; }

        [Browsable(true)]
        [DisplayName("Remove Control Characters")]
        public bool RemoveControlChars { get; set; }


        [Browsable(true)]
        [DisplayName("Override Character Set")]
        public bool OverrideCharSet { get; set; }

        #region IBaseComponent members
        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return _resourceManager.GetString("COMPONENTNAME.EdifactCleaner", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Version of the component
        /// </summary>
        [Browsable(false)]
        public string Version
        {
            get
            {
                return _resourceManager.GetString("COMPONENTVERSION.EdifactCleaner", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Description of the component
        /// </summary>
        [Browsable(false)]
        public string Description
        {
            get
            {
                return _resourceManager.GetString("COMPONENTDESCRIPTION.EdifactCleaner", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion

        #region IPersistPropertyBag members
        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classid">
        /// Class ID of the component
        /// </param>
        public void GetClassID(out System.Guid classid)
        {
            classid = new System.Guid("56FCE4CB-75A3-4877-8200-7BB1636A129F");
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public void InitNew()
        {
        }

        /// <summary>
        /// Loads configuration properties for the component
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="errlog">Error status</param>
        public virtual void Load(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, int errlog)
        {
            object val = null;
            val = ReadPropertyBag(pb, "Encoding");
            if ((val != null))
            {
                this._encoding = Encoding.GetEncoding((int)(val));
            }

            val = ReadPropertyBag(pb, "TargetCharSet");
            if (val != null)
            {
                this.TargetCharSet = (EdifactCharacterSet)Enum.Parse(typeof(EdifactCharacterSet), ReadPropertyBag(pb, "TargetCharSet") as string, true);
            }
            val = ReadPropertyBag(pb, "OverrideCharSet");
            if ((val != null))
            {
                this.OverrideCharSet = ((bool)(val));
            }
            val = ReadPropertyBag(pb, "Normalize");
            if ((val != null))
            {
                this.Normalize = ((bool)(val));
            }
            val = ReadPropertyBag(pb, "RemoveControlChars");
            if ((val != null))
            {
                this.RemoveControlChars = ((bool)(val));
            }
            val = ReadPropertyBag(pb, "FallbackChar");
            if ((val != null))
            {
                this.FallbackChar = ((char)(val));
            }

        }


        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="fClearDirty">not used</param>
        /// <param name="fSaveAllProperties">not used</param>
        public virtual void Save(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
        {
            WritePropertyBag(pb, "Encoding", this.Encoding.CodePage);
            WritePropertyBag(pb, "TargetCharSet", this.TargetCharSet.ToString());
            WritePropertyBag(pb, "OverrideCharSet", this.OverrideCharSet);
            WritePropertyBag(pb, "Normalize", this.Normalize);
            WritePropertyBag(pb, "RemoveControlChars", this.RemoveControlChars);
            WritePropertyBag(pb, "FallbackChar", this.FallbackChar);
        }

        #region utility functionality
        /// <summary>
        /// Reads property value from property bag
        /// </summary>
        /// <param name="pb">Property bag</param>
        /// <param name="propName">Name of property</param>
        /// <returns>Value of the property</returns>
        private static object ReadPropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName)
        {
            object val = null;
            try
            {
                pb.Read(propName, out val, 0);
            }
            catch (System.ArgumentException)
            {
                return val;
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
            return val;
        }

        /// <summary>
        /// Writes property values into a property bag.
        /// </summary>
        /// <param name="pb">Property bag.</param>
        /// <param name="propName">Name of property.</param>
        /// <param name="val">Value of property.</param>
        private static void WritePropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName, object val)
        {
            try
            {
                pb.Write(propName, ref val);
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
        }
        #endregion
        #endregion

        #region IComponentUI members
        /// <summary>
        /// Component icon to use in BizTalk Editor
        /// </summary>
        [Browsable(false)]
        public IntPtr Icon
        {
            get
            {
                var bitmap = (System.Drawing.Bitmap)(this._resourceManager.GetObject("COMPONENTICON", System.Globalization.CultureInfo.InvariantCulture));
                return bitmap !=
                       null ? bitmap.GetHicon() : IntPtr.Zero;

                ;

            }
        }

        /// <summary>
        /// The Validate method is called by the BizTalk Editor during the build 
        /// of a BizTalk project.
        /// </summary>
        /// <param name="obj">An Object containing the configuration properties.</param>
        /// <returns>The IEnumerator enables the caller to enumerate through a collection of strings containing error messages. These error messages appear as compiler error messages. To report successful property validation, the method should return an empty enumerator.</returns>
        public System.Collections.IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();
            return null;
        }
        #endregion

        #region IComponent members
        /// <summary>
        /// Implements IComponent.Execute method.
        /// </summary>
        /// <param name="pc">Pipeline context</param>
        /// <param name="inmsg">Input message</param>
        /// <returns>Original input message</returns>
        /// <remarks>
        /// IComponent.Execute method is used to initiate
        /// the processing of the message in this pipeline component.
        /// </remarks>
        public Microsoft.BizTalk.Message.Interop.IBaseMessage Execute(Microsoft.BizTalk.Component.Interop.IPipelineContext pc, Microsoft.BizTalk.Message.Interop.IBaseMessage inmsg)
        {

            var encoding = this.Encoding;
            if (!string.IsNullOrEmpty(inmsg.BodyPart.Charset))
            {
                encoding = Encoding.GetEncoding(inmsg.BodyPart.Charset);
            }

            var stream = GetSeekeableMessageStream(inmsg);

            var sr = new EdifactReader(stream, encoding, TargetCharSet, FallbackChar, Normalize, RemoveControlChars);

            if (OverrideCharSet && sr.CharSet != TargetCharSet)
            {
                stream = ReplaceCharSet(stream, encoding, TargetCharSet);
                sr = new EdifactReader(stream, encoding, TargetCharSet, FallbackChar, Normalize, RemoveControlChars);
            }

            var result = new MemoryStream();

            var sw = new StreamWriter(result, encoding);

            using (sr)
            {
                int c;
                while ((c = sr.Read()) != -1)
                {
                    sw.Write((char)c);
                }

            }
            sw.Flush();
            result.Seek(0, SeekOrigin.Begin);
            
            inmsg.BodyPart.Data = result;

            pc.ResourceTracker.AddResource(result);

            return inmsg;
        }

        private static Stream GetSeekeableMessageStream(IBaseMessage message)
        {
            var messageStream = message.BodyPart.GetOriginalDataStream();
            if (messageStream.CanSeek) return messageStream;
            // Create a virtual and seekable stream
            const int bufferSize = 0x280;
            const int thresholdSize = 0x100000;
            Stream virtualReadStream = new VirtualStream(bufferSize, thresholdSize);
            Stream seekableReadStream = new ReadOnlySeekableStream(messageStream, virtualReadStream, bufferSize);
            messageStream = seekableReadStream;
            message.BodyPart.Data = messageStream;
            return messageStream;
        }

        /// <summary>
        /// Non-streaming replace :)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <param name="charSet"></param>
        /// <returns></returns>
        private static Stream ReplaceCharSet(Stream stream, Encoding encoding, EdifactCharacterSet charSet)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            using (var sr = new StreamReader(stream, encoding))
            {
                var edifact = sr.ReadToEnd();

                var unoIndex = edifact.IndexOf("UNO", StringComparison.Ordinal);

                if (unoIndex >= 0 && edifact.Length > 3)
                {
                    var charsetInString = edifact.Substring(unoIndex, 4);
                    EdifactCharacterSet charSetIn;

                    if (Enum.TryParse<EdifactCharacterSet>(charsetInString, out charSetIn))
                    {
                        var regex = new Regex(Regex.Escape(charsetInString));
                        edifact = regex.Replace(edifact, charSet.ToString(), 1);
                    }
                }

                var result = new MemoryStream();
                var writer = new StreamWriter(result);
                writer.Write(edifact);
                writer.Flush();
                result.Position = 0;
                return result;
            }

        }

        #endregion
    }
}

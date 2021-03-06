using System.Linq;

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
    using System.Xml;

    using EAI.BE.BizTalk.Extensions;


    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [System.Runtime.InteropServices.Guid("A7B125C4-C929-4602-BA07-1385468D744B")]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    public class EdifactXmlCleaner : Microsoft.BizTalk.Component.Interop.IComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
    {
        private readonly System.Resources.ResourceManager _resourceManager = new System.Resources.ResourceManager("EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder", Assembly.GetExecutingAssembly());

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


        private char _fallbackChar = ' ';

        [Browsable(true)]
        [DisplayName("Fallback character")]
        public char FallbackChar
        {
            get { return _fallbackChar; }
            set { _fallbackChar = value; }
        }

        private string _extraCharsToReplace = "";

        [Browsable(true)]
        [DisplayName("Extra Replacement Characters")]
        [Description("Extra characters you want to replace (such as separators).")]
        public string ExtraCharsToReplace
        {
            get { return _extraCharsToReplace; }
            set { _extraCharsToReplace = value; }
        }

        [Browsable(true)]
        [DisplayName("Normalize")]
        [Description("Replaces control characters by spaces, deletes leading and trailing spaces.")]
        public bool Normalize { get; set; }


        #region IBaseComponent members
        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return _resourceManager.GetString("COMPONENTNAME.EdifactXmlCleaner", System.Globalization.CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("COMPONENTVERSION.EdifactXmlCleaner", System.Globalization.CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("COMPONENTDESCRIPTION.EdifactXmlCleaner", System.Globalization.CultureInfo.InvariantCulture);
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
            classid = new System.Guid("A7B125C4-C929-4602-BA07-1385468D744B");
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
            
            val = ReadPropertyBag(pb, "TargetCharSet");
            if (val != null)
            {
                this.TargetCharSet = (EdifactCharacterSet)Enum.Parse(typeof(EdifactCharacterSet), ReadPropertyBag(pb, "TargetCharSet") as string, true);
            }
            val = ReadPropertyBag(pb, "FallbackChar");
            if ((val != null))
            {
                if(val is char)
                { 
                    this.FallbackChar = ((char)(val));
                }
                if (val is ushort)
                {
                    this.FallbackChar = Convert.ToChar(val);
                }

            }
            val = ReadPropertyBag(pb, "ExtraCharsToReplace");
            if ((val != null))
            {
                this.ExtraCharsToReplace = ((string)(val));
            }
            val = ReadPropertyBag(pb, "Normalize");
            if ((val != null))
            {
                this.Normalize = ((bool)(val));
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
            WritePropertyBag(pb, "TargetCharSet", this.TargetCharSet.ToString());
            WritePropertyBag(pb, "FallbackChar", this.FallbackChar);
            WritePropertyBag(pb, "ExtraCharsToReplace", this.ExtraCharsToReplace);
            WritePropertyBag(pb, "Normalize", this.Normalize);
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
            var stream = GetSeekeableMessageStream(inmsg);

            EdifactCharacterSet targetCharSet;

            var syntax = inmsg.Context.Read("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties") as string;
            
            if (syntax == null || Enum.TryParse<EdifactCharacterSet>(syntax, out targetCharSet)==false)
            {
                targetCharSet = _targetCharSet;
            }

            var result = new MemoryStream();

            var settings = new XmlWriterSettings {Encoding = Encoding.UTF8, OmitXmlDeclaration = true};

            var separators = _extraCharsToReplace.ToCharArray();
            
            using (var reader = XmlReader.Create(stream))
            using (var writer = XmlWriter.Create(result, settings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            writer.WriteAttributes(reader, true);

                            if (reader.IsEmptyElement)
                            {
                                writer.WriteEndElement();
                            }
                            break;

                        case XmlNodeType.Text:
                            var value = reader.Value;

                            var sb = new StringBuilder();
                            foreach(var c in value)
                            { 
                                var step1 = c.Translate(targetCharSet,_fallbackChar);
                                var step2 = separators.Contains(step1) ? _fallbackChar : step1;
                                var step3 = this.Normalize && char.IsControl(step2) ? ' ' : step2;
                                sb.Append(step3);
                            }
                            writer.WriteString(this.Normalize ? sb.ToString().Trim() : sb.ToString());
                            break;

                        case XmlNodeType.EndElement:
                            writer.WriteFullEndElement();
                            break;

                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            //writer.WriteProcessingInstruction(reader.Name, reader.Value);
                            break;

                        case XmlNodeType.SignificantWhitespace:
                            writer.WriteWhitespace(reader.Value);
                            break;
                    }
                }
                writer.Flush();
            }
            
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

       
        #endregion
    }
}

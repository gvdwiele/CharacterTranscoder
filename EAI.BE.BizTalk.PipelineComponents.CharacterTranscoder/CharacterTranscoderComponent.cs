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
    
    
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [System.Runtime.InteropServices.Guid("0add01b8-e8de-417a-99f7-73796b91cab4")]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    public class CharacterTranscoderComponent : Microsoft.BizTalk.Component.Interop.IComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
    {
        private readonly System.Resources.ResourceManager _resourceManager = new System.Resources.ResourceManager("EAI.BE.BizTalk.PipelineComponents.CharacterTranscoder", Assembly.GetExecutingAssembly());

        private static readonly PropertyBase EncodingInProperty = new TRANSCODER.EncodingIn();
        private static readonly PropertyBase EncodingOutProperty = new TRANSCODER.EncodingOut();

        private Encoding _encodingOut = Encoding.UTF8;
        private Encoding _encodingIn = Encoding.UTF8;
        
        [Editor(typeof(EncodingTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(EncodingTypeConverter))]
        public Encoding EncodingOut
        {
            get
            {
                return _encodingOut;
            }
            set
            {
                _encodingOut = value;
            }
        }

        [Editor(typeof(EncodingTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(EncodingTypeConverter))]
        public Encoding EncodingIn
        {
            get
            {
                return _encodingIn;
            }
            set
            {
                _encodingIn = value;
            }
        }
        
        #region IBaseComponent members
        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return _resourceManager.GetString("COMPONENTNAME.CharacterTranscoder", System.Globalization.CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("COMPONENTVERSION.CharacterTranscoder", System.Globalization.CultureInfo.InvariantCulture);
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
                return _resourceManager.GetString("COMPONENTDESCRIPTION.CharacterTranscoder", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion


        static CharacterTranscoderComponent()
        {
            //var encodingList = Encoding.GetEncodings().ToDictionary(ei => ei.CodePage, ei => ei.Name);
        }


        #region IPersistPropertyBag members
        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classid">
        /// Class ID of the component
        /// </param>
        public void GetClassID(out System.Guid classid)
        {
            classid = new System.Guid("0add01b8-e8de-417a-99f7-73796b91cab4");
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
            val = ReadPropertyBag(pb, "EncodingOut");
            if ((val != null))
            {
                this._encodingOut = Encoding.GetEncoding((int)(val));
            }
            val = ReadPropertyBag(pb, "EncodingIn");
            if ((val != null))
            {
                this._encodingIn = Encoding.GetEncoding((int)(val));
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
            WritePropertyBag(pb, "EncodingOut", this.EncodingOut.CodePage);
            WritePropertyBag(pb, "EncodingIn", this.EncodingIn.CodePage);
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
            catch (System.ArgumentException )
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
            var encodingIn = this._encodingIn;
            var encodingOut = this._encodingOut;

            var val = (object)inmsg.Context.Read(EncodingInProperty.Name.Name, EncodingInProperty.Name.Namespace);
            if ((val != null))
            {
                encodingIn = Encoding.GetEncoding((string)val);
            }
            val = (object)inmsg.Context.Read(EncodingOutProperty.Name.Name, EncodingOutProperty.Name.Namespace);
            if ((val != null))
            {
                encodingOut = Encoding.GetEncoding((string)val);
            }
            inmsg.BodyPart.Data = new TranscodingStream(
                inmsg.BodyPart.GetOriginalDataStream(),
                encodingOut, encodingIn);

            return inmsg;
        }
        #endregion
    }
}

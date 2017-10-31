using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WixSharp
{
    /// <summary>
    /// Base class for WiX projects (e.g. Project, Bundle).
    /// </summary>
    public abstract partial class WixProject : WixEntity
    {
        string sourceBaseDir = "";
        /// <summary>
        /// Base directory for the relative paths of the bootstrapper items (e.g. <see cref="T:WixSharp.Bootstrapper.MsiPackage"></see>).
        /// </summary>
        public string SourceBaseDir
        {
            get { return sourceBaseDir.ExpandEnvVars(); }
            set { sourceBaseDir = value; }
        }


        /// <summary>
        /// The location of the config file for Managed Custom Action.
        /// <para>The config file (CustomAction.config) is the file to be passed to the MakeSfxCA.exe when packing the Custom Action assembly.</para>
        /// </summary>
        public string CAConfigFile = "";

        internal string CustomActionConfig
        {
            get
            {
                var configFile = this.CAConfigFile;
                if (configFile.IsNotEmpty() && !System.IO.Path.IsPathRooted(configFile))
                    return Utils.PathCombine(this.SourceBaseDir, this.CAConfigFile);

                return configFile;
            }
        }

        /// <summary>
        /// Name of the MSI/MSM file (without extension) to be build.
        /// </summary>
        public string OutFileName = "setup";

        string outDir;

        /// <summary>
        /// The output directory. The directory where all msi and temporary files should be assembled. The <c>CurrentDirectory</c> will be used if <see cref="OutDir"/> is left unassigned.
        /// </summary>
        public string OutDir
        {
            get
            {
                return outDir.IsEmpty() ? Environment.CurrentDirectory : outDir.ExpandEnvVars();
            }
            set
            {
                outDir = value;
            }
        }

        /// <summary>
        /// Collection of XML namespaces (e.g. <c>xmlns:iis="http://schemas.microsoft.com/wix/IIsExtension"</c>) to be declared in the XML (WiX project) root.
        /// </summary>
        public List<string> WixNamespaces = new List<string>();

        /// <summary>
        /// Collection of paths to the WiX extensions.
        /// </summary>
        public List<string> WixExtensions
        {
            get
            {
                return wixExtensions;
            }
        }

        List<string> wixExtensions = new List<string>();

        /// <summary>
        /// Collection of paths to the external wsx files containing Fragment(s). 
        /// <para>
        /// At the compile time files will be pases to candle.exe but the referencing them fragments in the primary wxs (XML)
        /// needs to be done from WixSourceGenerated event handler.
        /// </para>
        /// </summary>
        public List<string> WxsFiles = new List<string>();


        /// <summary>
        /// Collection of paths to the external wsxlib files to be passed to the Light linker. 
        /// </summary>
        public List<string> LibFiles = new List<string>();

        /// <summary>
        /// Installation UI Language. If not specified <c>"en-US"</c> will be used.
        /// <para>It is possible to specify multiple languages separated by coma or semicolon. All extra languages will be used 
        /// as additional values for 'Package.Languages' attribute and light.exe '-cultures:' command line parameters.</para>
        /// </summary>
        public string Language = "en-US";

        /// <summary>
        /// WiX linker <c>Light.exe</c> options.
        /// <para>The default value is "-sw1076 -sw1079" (disable warning 1076 and 1079).</para>
        /// </summary>
        public string LightOptions = "";

        /// <summary>
        /// WiX compiler <c>Candle.exe</c> options.
        /// <para>The default value is "-sw1076" (disable warning 1026).</para>
        /// </summary>
        public string CandleOptions = "";

        /// <summary>
        /// Occurs when WiX source code generated. Use this event if you need to modify generated XML (XDocument)
        /// before it is compiled into MSI.
        /// </summary>
        public event XDocumentGeneratedDlgt WixSourceGenerated;

        /// <summary>
        /// Occurs when WiX source file is saved. Use this event if you need to do any post-processing of the generated/saved file.
        /// </summary>
        public event XDocumentSavedDlgt WixSourceSaved;

        /// <summary>
        /// Occurs when WiX source file is formatted and ready to be saved. Use this event if you need to do any custom formatting 
        /// of the XML content before it is saved by the compiler.
        /// </summary>
        public event XDocumentFormatedDlgt WixSourceFormated;

        /// <summary>
        /// Forces <see cref="Compiler"/> to preserve all temporary build files (e.g. *.wxs).
        /// <para>The default value is <c>false</c>: all temporary files are deleted at the end of the build/compilation.</para>
        /// <para>Note: if <see cref="Compiler"/> fails to build MSI the <c>PreserveTempFiles</c>
        /// value is ignored and all temporary files are preserved.</para>
        /// </summary>
        public bool PreserveTempFiles = false;

        internal void InvokeWixSourceGenerated(XDocument doc)
        {
            if (WixSourceGenerated != null)
                WixSourceGenerated(doc);
        }

        internal void InvokeWixSourceSaved(string fileName)
        {
            if (WixSourceSaved != null)
                WixSourceSaved(fileName);
        }

        internal void InvokeWixSourceFormated(ref string content)
        {
            if (WixSourceFormated != null)
                WixSourceFormated(ref content);
        }


        /// <summary>
        /// Adds the specified extension to  <see cref="WixProject"/>
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void IncludeWixExtension(WixExtension extension)
        {
            IncludeWixExtension(extension.Assembly, extension.XmlNamespacePrefix, extension.XmlNamespace);
        }

        /// <summary>
        /// Adds the specified extension to  <see cref="WixProject" />
        /// </summary>
        /// <param name="extensionAssembly">The extension assembly.</param>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <param name="namespace">The namespace.</param>
        public void IncludeWixExtension(string extensionAssembly, string namespacePrefix, string @namespace)
        {
            if (!this.WixExtensions.Contains(extensionAssembly))
                this.WixExtensions.Add(extensionAssembly);

            if (namespacePrefix.IsEmpty())
            {
                var namespaceDeclaration = WixExtension.GetNamespaceDeclaration(namespacePrefix, @namespace);
                //could use detection of duplicate prefixes
                if (!this.WixNamespaces.Contains(namespaceDeclaration))
                    this.WixNamespaces.Add(namespaceDeclaration);
            }
        }
    }
}
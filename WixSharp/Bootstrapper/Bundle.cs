using sys = System.IO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace WixSharp.Bootstrapper
{
    //Useful stuff to have a look at: 
    //http://neilsleightholm.blogspot.com.au/2012/05/wix-burn-tipstricks.html
    //https://wixwpf.codeplex.com/



    /// <summary>
    /// Class for defining a WiX standard Burn-based bootstrapper. By default the bootstrapper is using WiX default WiX bootstrapper UI.
    /// </summary>
    /// <example>The following is an example of defining a bootstrapper for two msi files and .NET Web setup.
    /// <code>
    ///  var bootstrapper =
    ///      new Bundle("My Product",
    ///          new PackageGroupRef("NetFx40Web"),
    ///          new MsiPackage("productA.msi"),
    ///          new MsiPackage("productB.msi"));
    ///          
    /// bootstrapper.AboutUrl = "https://wixsharp.codeplex.com/";
    /// bootstrapper.IconFile = "app_icon.ico";
    /// bootstrapper.Version = new Version("1.0.0.0");
    /// bootstrapper.UpgradeCode = new Guid("6f330b47-2577-43ad-9095-1861bb25889b");
    /// bootstrapper.Application.LogoFile = "logo.png";
    /// 
    /// bootstrapper.Build();
    /// </code>
    /// </example>
    public partial class Bundle : WixProject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bootstrapper"/> class.
        /// </summary>
        public Bundle()
        {
            WixExtensions.Add("WiXNetFxExtension");
            WixExtensions.Add("WiXBalFxExtension");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bootstrapper" /> class.
        /// </summary>
        /// <param name="name">The name of the project. Typically it is the name of the product to be installed.</param>
        /// <param name="items">The project installable items (e.g. directories, files, registry keys, Custom Actions).</param>
        public Bundle(string name, params ChainItem[] items)
        {
            WixExtensions.Add("WiXNetFxExtension");
            WixExtensions.Add("WiXBalExtension");
            Name = name;
            Chain.AddRange(items);
        }

        /// <summary>
        /// The disable rollbackSpecifies whether the bundle will attempt to rollback packages executed in the chain.
        /// If "true" is specified then when a vital package fails to install only that package will rollback and the chain will stop with the error.
        /// The default is "false" which indicates all packages executed during the chain will be rolldback to their previous state when a vital package fails.
        /// </summary>
        public bool? DisableRollback;

        /// <summary>
        /// Specifies whether the bundle will attempt to create a system restore point when executing the chain. If "true" is specified then a system restore 
        /// point will not be created. The default is "false" which indicates a system restore point will be created when the bundle is installed, uninstalled, 
        /// repaired, modified, etc. If the system restore point cannot be created, the bundle will log the issue and continue.
        /// </summary>
        public bool? DisableSystemRestore;

        /// <summary>
        /// Specifies whether the bundle will start installing packages while other packages are still being cached. 
        /// If "true", packages will start executing when a rollback boundary is encountered. The default is "false" 
        /// which dictates all packages must be cached before any packages will start to be installed.
        /// </summary>
        public bool? ParallelCache;

        /// <summary>
        /// The legal copyright found in the version resources of final bundle executable.
        /// If this attribute is not provided the copyright will be set to "Copyright (c) [Bundle/@Manufacturer]. All rights reserved.".
        /// </summary>
        [Xml]
        public string Copyright;

        /// <summary>
        /// A URL for more information about the bundle to display in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml]
        public string AboutUrl;

        /// <summary>
        /// Whether Packages and Payloads not assigned to a container should be added to the default attached container or if they
        /// should be external. The default is yes.
        /// </summary>
        [Xml]
        public bool? Compressed;

        /// <summary>
        /// The condition of the bundle. If the condition is not met, the bundle will refuse to run. Conditions are checked before the
        /// bootstrapper application is loaded (before detect), and thus can only reference built-in variables such as variables which
        /// indicate the version of the OS.
        /// </summary>
        [Xml]
        public string Condition;

        /// <summary>
        /// Determines whether the bundle can be removed via the Programs and Features (also known as Add/Remove Programs). If the value is
        /// "yes" then the "Uninstall" button will not be displayed. The default is "no" which ensures there is an "Uninstall" button to remove
        /// the bundle. If the "DisableModify" attribute is also "yes" or "button" then the bundle will not be displayed in Progams and
        /// Features and another mechanism (such as registering as a related bundle addon) must be used to ensure the bundle can be removed.
        /// </summary>
        [Xml]
        public bool? DisableRemove;

        /// <summary>
        /// Determines whether the bundle can be modified via the Programs and Features (also known as Add/Remove Programs). If the value is
        /// "button" then Programs and Features will show a single "Uninstall/Change" button. If the value is "yes" then Programs and Features
        /// will only show the "Uninstall" button". If the value is "no", the default, then a "Change" button is shown. See the DisableRemove
        /// attribute for information how to not display the bundle in Programs and Features.
        /// </summary>
        [Xml]
        public string DisableModify;

        /// <summary>
        /// A telephone number for help to display in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml]
        public string HelpTelephone;

        /// <summary>
        /// A URL to the help for the bundle to display in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml]
        public string HelpUrl;

        /// <summary>
        /// Path to an icon that will replace the default icon in the final Bundle executable. This icon will also be displayed in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml(Name = "IconSourceFile")]
        public string IconFile;

        /// <summary>
        /// The publisher of the bundle to display in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml]
        public string Manufacturer;

        /// <summary>
        /// The name of the parent bundle to display in Installed Updates (also known as Add/Remove Programs). This name is used to nest or group bundles that will appear as updates. If the
        /// parent name does not actually exist, a virtual parent is created automatically.
        /// </summary>
        [Xml]
        public string ParentName;

        /// <summary>
        /// Path to a bitmap that will be shown as the bootstrapper application is being loaded. If this attribute is not specified, no splash screen will be displayed.
        /// </summary>
        [Xml(Name = "SplashScreenSourceFile")]
        public string SplashScreenSource;

        /// <summary>
        /// Set this string to uniquely identify this bundle to its own BA, and to related bundles. The value of this string only matters to the BA, and its value has no direct
        /// effect on engine functionality.
        /// </summary>
        [Xml]
        public string Tag;

        /// <summary>
        /// A URL for updates of the bundle to display in Programs and Features (also known as Add/Remove Programs).
        /// </summary>
        [Xml]
        public string UpdateUrl;

        /// <summary>
        /// Unique identifier for a family of bundles. If two bundles have the same UpgradeCode the bundle with the highest version will be installed.
        /// </summary>
        [Xml]
        public Guid UpgradeCode = Guid.NewGuid();

        /// <summary>
        /// The version of the bundle. Newer versions upgrade earlier versions of the bundles with matching UpgradeCodes. If the bundle is registered in Programs and Features then this attribute will be displayed in the Programs and Features user interface.
        /// </summary>
        [Xml]
        public Version Version;

        /// <summary>
        /// The sequence of the packages to be installed
        /// </summary>
        public List<ChainItem> Chain = new List<ChainItem>();

        /// <summary>
        /// The instance of the Bootstrapper application class application. By default it is a LicenseBootstrapperApplication object.
        /// </summary>
        public WixStandardBootstrapperApplication Application = new LicenseBootstrapperApplication();

        /// <summary>
        /// Emits WiX XML.
        /// </summary>
        /// <returns></returns>
        public XContainer[] ToXml()
        {
            var result = new List<XContainer>();

            var root = new XElement("Bundle",
                           new XAttribute("Name", Name));

            root.AddAttributes(this.Attributes);
            root.Add(this.MapToXmlAttributes());
            
            if (Application is ManagedBootstrapperApplication)
            {
                var app = Application as ManagedBootstrapperApplication;
                if (app.PrimaryPackageId == null)
                {
                    var lastPackage = Chain.OfType<WixSharp.Bootstrapper.Package>().LastOrDefault();
                    if (lastPackage != null)
                    {
                        lastPackage.EnsureId();
                        app.PrimaryPackageId = lastPackage.Id;
                    }
                }
            }

            //important to call AutoGenerateSources after PrimaryPackageId is set
            Application.AutoGenerateSources(this.OutDir);

            root.Add(Application.ToXml());

            string variabes = this.StringVariablesDefinition +";"+ Application.StringVariablesDefinition;

            foreach (var entry in variabes.ToDictionary())
            {
                root.AddElement("Variable", "Name=" + entry.Key + ";Value=" + entry.Value + ";Persisted=yes;Type=string");
            }

            var xChain = root.AddElement("Chain");
            foreach (var item in this.Chain)
                xChain.Add(item.ToXml());

            xChain.SetAttribute("DisableRollback", DisableRollback);
            xChain.SetAttribute("DisableSystemRestore", DisableSystemRestore);
            xChain.SetAttribute("ParallelCache", ParallelCache);

            result.Add(root);
            return result.ToArray();
        }

        /// <summary>
        /// The Bundle string variables. 
        /// </summary>
        /// <para>The variables are defined as a named values map.</para>
        /// <example>
        /// <code>
        /// new ManagedBootstrapperApplication("ManagedBA.dll")
        /// {
        ///     StringVariablesDefinition = "FullInstall=Yes; Silent=No"
        /// }
        /// </code>
        /// </example>
        public string StringVariablesDefinition = "";

        /// <summary>
        /// Builds WiX Bootstrapper application from the specified <see cref="Bundle" /> project instance.
        /// </summary>
        /// <param name="path">The path to the bootstrapper to be build.</param>
        /// <returns></returns>
        public string Build(string path = null)
        {
            if (Compiler.ClientAssembly.IsEmpty())
                Compiler.ClientAssembly = System.Reflection.Assembly.GetCallingAssembly().Location;

            if (path == null)
                return Compiler.Build(this);
            else
                return Compiler.Build(this, path);
        }

        /// <summary>
        /// Builds the WiX source file and generates batch file capable of building
        /// WiX/MSI bootstrapper with WiX toolset.
        /// </summary>
        /// <param name="path">The path to the batch file to be created.</param>
        /// <returns></returns>
        public string BuildCmd(string path = null)
        {
            if (Compiler.ClientAssembly.IsEmpty())
                Compiler.ClientAssembly = System.Reflection.Assembly.GetCallingAssembly().Location;

            if (path == null)
                return Compiler.BuildCmd(this);
            else
                return Compiler.BuildCmd(this, path);
        }


    }

    /*
      <Bundle Name="My Product"
            Version="1.0.0.0"
            Manufacturer="OSH"
            AboutUrl="https://wixsharp.codeplex.com/"
            IconSourceFile="app_icon.ico"
            UpgradeCode="acaa3540-97e0-44e4-ae7a-28c20d410a60">

        <BootstrapperApplicationRef Id="WixStandardBootstrapperApplication.RtfLicense">
            <bal:WixStandardBootstrapperApplication LicenseFile="readme.txt" LocalizationFile="" LogoFile="app_icon.ico" />
        </BootstrapperApplicationRef>

        <Chain>
            <!-- Install .Net 4 Full -->
            <PackageGroupRef Id="NetFx40Web"/>
            <!--<ExePackage
                Id="Netfx4FullExe"
                Cache="no"
                Compressed="no"
                PerMachine="yes"
                Permanent="yes"
                Vital="yes"
                SourceFile="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bootstrapper\Packages\DotNetFX40\dotNetFx40_Full_x86_x64.exe"
                InstallCommand="/q /norestart /ChainingPackage FullX64Bootstrapper"
                DetectCondition="NETFRAMEWORK35='#1'"
                DownloadUrl="http://go.microsoft.com/fwlink/?LinkId=164193" />-->

            <RollbackBoundary />

            <MsiPackage SourceFile="E:\Galos\Projects\WixSharp\src\WixSharp.Samples\Wix# Samples\Managed Setup\ManagedSetup.msi" Vital="yes" />
        </Chain>
    </Bundle>
     */
}

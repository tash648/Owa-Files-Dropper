#region Licence...

/*
The MIT License (MIT)
Copyright (c) 2014 Oleg Shilo
Permission is hereby granted,
free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

#endregion Licence...

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using WixSharp;
using WixSharp.Controls;
using IO = System.IO;

namespace WixSharp.CommonTasks
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Tasks
    {
        /// <summary>
        /// Builds the bootstrapper.
        /// </summary>
        /// <param name="prerequisiteFile">The prerequisite file.</param>
        /// <param name="primaryFile">The primary setup file.</param>
        /// <param name="outputFile">The output (bootsrtapper) file.</param>
        /// <param name="prerequisiteRegKeyValue">The prerequisite registry key value.
        /// <para>This value is used to determine if the <c>PrerequisiteFile</c> should be launched.</para>
        /// <para>This value must comply with the following pattern: &lt;RegistryHive&gt;:&lt;KeyPath&gt;:&lt;ValueName&gt;.</para>
        /// <code>PrerequisiteRegKeyValue = @"HKLM:Software\My Company\My Product:Installed";</code>
        /// Existence of the specified registry value at runtime is interpreted as an indication of the <c>PrerequisiteFile</c> has been alreday installed.
        /// Thus bootstrapper will execute <c>PrimaryFile</c> without launching <c>PrerequisiteFile</c> first.</param>
        /// <param name="doNotPostVerifyPrerequisite">The flag which allows you to disable verification of <c>PrerequisiteRegKeyValue</c> after the prerequisite setup is completed.
        /// <para>Normally if <c>bootstrapper</c> checks if <c>PrerequisiteRegKeyValue</c>/> exists straight after the prerequisite installation and starts
        /// the primary setup only if it does.</para>
        /// <para>It is possible to instruct bootstrapper to continue with the primary setup regardless of the prerequisite installation outcome. This can be done
        /// by setting DoNotPostVerifyPrerequisite to <c>true</c> (default is <c>false</c>)</para>
        ///</param>
        /// <param name="optionalArguments">The optional arguments for the bootstrapper compiler.</param>
        /// <returns>Path to the built bootstrapper file. Returns <c>null</c> if bootstrapper cannot be built.</returns>
        ///
        /// <example>The following is an example of building bootstrapper <c>Setup.msi</c> for deploying .NET along with the <c>MyProduct.msi</c> file.
        /// <code>
        /// WixSharp.CommonTasks.Tasks.BuildBootstrapper(
        ///         @"C:\downloads\dotnetfx.exe",
        ///         "MainProduct.msi",
        ///         "setup.exe",
        ///         @"HKLM:Software\My Company\My Product:Installed"
        ///         false,
        ///         "");
        /// </code>
        /// </example>
        static public string BuildBootstrapper(string prerequisiteFile, string primaryFile, string outputFile, string prerequisiteRegKeyValue, bool doNotPostVerifyPrerequisite, string optionalArguments)
        {
            var nbs = new NativeBootstrapper
            {
                PrerequisiteFile = prerequisiteFile,
                PrimaryFile = primaryFile,
                OutputFile = outputFile,
                PrerequisiteRegKeyValue = prerequisiteRegKeyValue
            };

            nbs.DoNotPostVerifyPrerequisite = doNotPostVerifyPrerequisite;

            if (!optionalArguments.IsEmpty())
                nbs.OptionalArguments = optionalArguments;

            return nbs.Build();
        }

        /// <summary>
        /// Builds the bootstrapper.
        /// </summary>
        /// <param name="prerequisiteFile">The prerequisite file.</param>
        /// <param name="primaryFile">The primary setup file.</param>
        /// <param name="outputFile">The output (bootsrtapper) file.</param>
        /// <param name="prerequisiteRegKeyValue">The prerequisite registry key value.
        /// <para>This value is used to determine if the <c>PrerequisiteFile</c> should be launched.</para>
        /// <para>This value must comply with the following pattern: &lt;RegistryHive&gt;:&lt;KeyPath&gt;:&lt;ValueName&gt;.</para>
        /// <code>PrerequisiteRegKeyValue = @"HKLM:Software\My Company\My Product:Installed";</code>
        /// Existence of the specified registry value at runtime is interpreted as an indication of the <c>PrerequisiteFile</c> has been already installed.
        /// Thus bootstrapper will execute <c>PrimaryFile</c> without launching <c>PrerequisiteFile</c> first.</param>
        /// <param name="doNotPostVerifyPrerequisite">The flag which allows you to disable verification of <c>PrerequisiteRegKeyValue</c> after the prerequisite setup is completed.
        /// <para>Normally if <c>bootstrapper</c> checkers if <c>PrerequisiteRegKeyValue</c>/> exists straight after the prerequisite installation and starts
        /// the primary setup only if it does.</para>
        /// <para>It is possible to instruct bootstrapper to continue with the primary setup regardless of the prerequisite installation outcome. This can be done
        /// by setting DoNotPostVerifyPrerequisite to <c>true</c> (default is <c>false</c>)</para>
        ///</param>
        /// <returns>Path to the built bootstrapper file. Returns <c>null</c> if bootstrapper cannot be built.</returns>
        ///
        /// <example>The following is an example of building bootstrapper <c>Setup.msi</c> for deploying .NET along with the <c>MyProduct.msi</c> file.
        /// <code>
        /// WixSharp.CommonTasks.Tasks.BuildBootstrapper(
        ///         @"C:\downloads\dotnetfx.exe",
        ///         "MainProduct.msi",
        ///         "setup.exe",
        ///         @"HKLM:Software\My Company\My Product:Installed"
        ///         false);
        /// </code>
        /// </example>
        static public string BuildBootstrapper(string prerequisiteFile, string primaryFile, string outputFile, string prerequisiteRegKeyValue, bool doNotPostVerifyPrerequisite)
        {
            return BuildBootstrapper(prerequisiteFile, primaryFile, outputFile, prerequisiteRegKeyValue, doNotPostVerifyPrerequisite, null);
        }

        /// <summary>
        /// Builds the bootstrapper.
        /// </summary>
        /// <param name="prerequisiteFile">The prerequisite file.</param>
        /// <param name="primaryFile">The primary setup file.</param>
        /// <param name="outputFile">The output (bootsrtapper) file.</param>
        /// <param name="prerequisiteRegKeyValue">The prerequisite registry key value.
        /// <para>This value is used to determine if the <c>PrerequisiteFile</c> should be launched.</para>
        /// <para>This value must comply with the following pattern: &lt;RegistryHive&gt;:&lt;KeyPath&gt;:&lt;ValueName&gt;.</para>
        /// <code>PrerequisiteRegKeyValue = @"HKLM:Software\My Company\My Product:Installed";</code>
        /// Existence of the specified registry value at runtime is interpreted as an indication of the <c>PrerequisiteFile</c> has been already installed.
        /// Thus bootstrapper will execute <c>PrimaryFile</c> without launching <c>PrerequisiteFile</c> first.</param>
        /// <returns>Path to the built bootstrapper file. Returns <c>null</c> if bootstrapper cannot be built.</returns>
        static public string BuildBootstrapper(string prerequisiteFile, string primaryFile, string outputFile, string prerequisiteRegKeyValue)
        {
            return BuildBootstrapper(prerequisiteFile, primaryFile, outputFile, prerequisiteRegKeyValue, false, null);
        }

        /// <summary>
        /// Applies digital signature to a file (e.g. msi, exe, dll) with MS <c>SignTool.exe</c> utility.
        /// </summary>
        /// <param name="fileToSign">The file to sign.</param>
        /// <param name="pfxFile">Specify the signing certificate in a file. If this file is a PFX with a password, the password may be supplied
        /// with the <c>password</c> parameter.</param>
        /// <param name="timeURL">The timestamp server's URL. If this option is not present (pass to null), the signed file will not be timestamped.
        /// A warning is generated if timestamping fails.</param>
        /// <param name="password">The password to use when opening the PFX file. Should be <c>null</c> if no password required.</param>
        /// <param name="optionalArguments">Extra arguments to pass to the <c>SignTool.exe</c> utility.</param>
        /// <param name="wellKnownLocations">The optional ';' separated list of directories where SignTool.exe can be located.
        /// If this parameter is not specified WixSharp will try to locate the SignTool in the built-in well-known locations (system PATH)</param>
        /// <returns>Exit code of the <c>SignTool.exe</c> process.</returns>
        ///
        /// <example>The following is an example of signing <c>Setup.msi</c> file.
        /// <code>
        /// WixSharp.CommonTasks.Tasks.DigitalySign(
        ///     "Setup.msi",
        ///     "MyCert.pfx",
        ///     "http://timestamp.verisign.com/scripts/timstamp.dll",
        ///     "MyPassword",
        ///     null);
        /// </code>
        /// </example>
        static public int DigitalySign(string fileToSign, string pfxFile, string timeURL, string password, string optionalArguments = null, string wellKnownLocations = null)
        {
            //"C:\Program Files\\Microsoft SDKs\Windows\v6.0A\bin\signtool.exe" sign /f "pfxFile" /p password /v "fileToSign" /t timeURL
            //string args = "sign /v /f \"" + pfxFile + "\" \"" + fileToSign + "\"";
            string args = "sign /v /f \"" + pfxFile + "\"";
            if (timeURL != null)
                args += " /t \"" + timeURL + "\"";
            if (password != null)
                args += " /p \"" + password + "\"";
            if (!optionalArguments.IsEmpty())
                args += " " + optionalArguments;

            args += " \"" + fileToSign + "\"";

            var tool = new ExternalTool
            {
                WellKnownLocations = wellKnownLocations ?? @"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin;C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin",
                ExePath = "signtool.exe",
                Arguments = args
            };
            return tool.ConsoleRun();
        }

        /// <summary>
        /// Applies digital signature to a file (e.g. msi, exe, dll) with MS <c>SignTool.exe</c> utility.
        /// <para>If you need to specify extra SignTool.exe parameters or the location of the tool use the overloaded <c>DigitalySign</c> signature </para>
        /// </summary>
        /// <param name="fileToSign">The file to sign.</param>
        /// <param name="pfxFile">Specify the signing certificate in a file. If this file is a PFX with a password, the password may be supplied
        /// with the <c>password</c> parameter.</param>
        /// <param name="timeURL">The timestamp server's URL. If this option is not present, the signed file will not be timestamped.
        /// A warning is generated if timestamping fails.</param>
        /// <param name="password">The password to use when opening the PFX file.</param>
        /// <returns>Exit code of the <c>SignTool.exe</c> process.</returns>
        ///
        /// <example>The following is an example of signing <c>Setup.msi</c> file.
        /// <code>
        /// WixSharp.CommonTasks.Tasks.DigitalySign(
        ///     "Setup.msi",
        ///     "MyCert.pfx",
        ///     "http://timestamp.verisign.com/scripts/timstamp.dll",
        ///     "MyPassword");
        /// </code>
        /// </example>
        static public int DigitalySign(string fileToSign, string pfxFile, string timeURL, string password)
        {
            return DigitalySign(fileToSign, pfxFile, timeURL, password, null);
        }

        /// <summary>
        /// Imports the reg file.
        /// </summary>
        /// <param name="regFile">The reg file.</param>
        /// <returns></returns>
        /// <example>The following is an example of importing registry entries from the *.reg file.
        /// <code>
        /// var project =
        ///     new Project("MyProduct",
        ///         new Dir(@"%ProgramFiles%\My Company\My Product",
        ///             new File(@"readme.txt")),
        ///         ...
        ///
        /// project.RegValues = CommonTasks.Tasks.ImportRegFile("app_settings.reg");
        ///
        /// Compiler.BuildMsi(project);
        /// </code>
        /// </example>
        static public RegValue[] ImportRegFile(string regFile)
        {
            return RegFileImporter.ImportFrom(regFile);
        }

        /// <summary>
        /// Imports the reg file. It is nothing else but an extension method version of the 'plain' <see cref="T:WixSharp.CommonTasks.Tasks.ImportRegFile"/>.
        /// </summary>
        /// <param name="project">The project object.</param>
        /// <param name="regFile">The reg file.</param>
        /// <returns></returns>
        /// <example>The following is an example of importing registry entries from the *.reg file.
        /// <code>
        /// var project =
        ///     new Project("MyProduct",
        ///         new Dir(@"%ProgramFiles%\My Company\My Product",
        ///             new File(@"readme.txt")),
        ///         ...
        ///
        /// project.ImportRegFile("app_settings.reg");
        ///
        /// Compiler.BuildMsi(project);
        /// </code>
        /// </example>
        static public Project ImportRegFile(this Project project, string regFile)
        {
            project.RegValues = ImportRegFile(regFile);
            return project;
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddProperty(this Project project, params Property[] items)
        {
            project.Properties = project.Properties.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the action.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddAction(this Project project, params Action[] items)
        {
            project.Actions = project.Actions.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the dir.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddDir(this Project project, params Dir[] items)
        {
            project.Dirs = project.Dirs.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the registry value.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddRegValue(this Project project, params RegValue[] items)
        {
            project.RegValues = project.RegValues.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the binary.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddBinary(this Project project, params Binary[] items)
        {
            project.Binaries = project.Binaries.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the environment variable.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        [Obsolete("AddEnvironmentVariabl is obsolete as the name has a typo. Please use AddEnvironmentVariable instead.", false)]
        static public Project AddEnvironmentVariabl(this Project project, params EnvironmentVariable[] items)
        {
            return AddEnvironmentVariable(project, items);
        }

        /// <summary>
        /// Adds the environment variable.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Project AddEnvironmentVariable(this Project project, params EnvironmentVariable[] items)
        {
            project.EnvironmentVariables = project.EnvironmentVariables.AddRange(items);
            return project;
        }

        /// <summary>
        /// Adds the assembly reference.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        static public ManagedAction AddRefAssembly(this ManagedAction action, params string[] files)
        {
            action.RefAssemblies = action.RefAssemblies.AddRange(files);
            return action;
        }
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the file association.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public File AddAssociation(this File file, params FileAssociation[] items)
        {
            file.Associations = file.Associations.AddRange(items);
            return file;
        }
        /// <summary>
        /// Adds the shortcut.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public File AddShortcut(this File file, params FileShortcut[] items)
        {
            file.Shortcuts = file.Shortcuts.AddRange(items);
            return file;
        }
        //////////////////////////////////////////////////////////////////
        /// <summary>
        /// Adds the dir.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddDir(this Dir dir, params Dir[] items)
        {
            dir.Dirs = dir.Dirs.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddFile(this Dir dir, params File[] items)
        {
            dir.Files = dir.Files.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Adds the shortcut.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddShortcut(this Dir dir, params ExeFileShortcut[] items)
        {
            dir.Shortcuts = dir.Shortcuts.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Adds the merge module.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddMergeModule(this Dir dir, params Merge[] items)
        {
            dir.MergeModules = dir.MergeModules.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Adds the file collection.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddFileCollection(this Dir dir, params Files[] items)
        {
            dir.FileCollections = dir.FileCollections.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Adds the dir file collection.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        static public Dir AddDirFileCollection(this Dir dir, params DirFiles[] items)
        {
            dir.DirFileCollections = dir.DirFileCollections.AddRange(items);
            return dir;
        }

        /// <summary>
        /// Removes the dialogs between specified two dialogs. It simply connects 'next' button of the start dialog with the
        /// 'NewDialog' action associated with the end dialog. And vise versa for the 'back' button.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        /// <example>The following is an example of the setup that skips License dialog.
        /// <code>
        /// project.UI = WUI.WixUI_InstallDir;
        /// project.RemoveDialogsBetween(Dialogs.WelcomeDlg, Dialogs.InstallDirDlg);
        /// ...
        /// Compiler.BuildMsi(project);
        /// </code>
        /// </example>
        static public Project RemoveDialogsBetween(this Project project, string start, string end)
        {
            if (project.CustomUI == null)
                project.CustomUI = new Controls.DialogSequence();

            project.CustomUI.On(start, Controls.Buttons.Next, new Controls.ShowDialog(end) { Order = Controls.DialogSequence.DefaultOrder });
            project.CustomUI.On(end, Controls.Buttons.Back, new Controls.ShowDialog(start) { Order = Controls.DialogSequence.DefaultOrder });
            return project;
        }

        /// <summary>
        /// Sets the Project version from the file version of the file specified by it's ID.
        /// <para>This method sets project WixSourceGenerated event handler and injects 
        /// "!(bind.FileVersion.&lt;file ID&gt;" into the XML Product's Version attribute.</para>
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        static public Project SetVersionFrom(this Project project, string fileId)
        {
            project.WixSourceGenerated += document =>
                document.FindSingle("Product")
                        .AddAttributes("Version=!(bind.FileVersion." + fileId + ")");
            return project;
        }

        /// <summary>
        /// Injects CLR dialog between MSI dialogs 'prevDialog' and 'nextDialog'.
        /// Passes custom action CLR method name (showDialogMethod) for instantiating and popping up the CLR dialog.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="showDialogMethod">The show dialog method.</param>
        /// <param name="prevDialog">The previous dialog.</param>
        /// <param name="nextDialog">The next dialog.</param>
        /// <returns></returns>
        /// <example>The following is an example of inserting CustomDialog dialog into the UI sequence between MSI dialogs InsallDirDlg and VerifyReadyDlg.
        /// <code>
        /// public class static Script
        /// {
        ///     public static void Main()
        ///     {
        ///         var project = new Project("CustomDialogTest");
        ///
        ///         project.InjectClrDialog("ShowCustomDialog", Dialogs.InstallDirDlg, Dialogs.VerifyReadyDlg);
        ///         Compiler.BuildMsi(project);
        ///     }
        ///
        ///     [CustomAction]
        ///     public static ActionResult ShowCustomDialog(Session session)
        ///     {
        ///         return WixCLRDialog.ShowAsMsiDialog(new CustomDialog(session));
        ///     }
        ///}
        /// </code>
        /// </example>
        static public Project InjectClrDialog(this Project project, string showDialogMethod, string prevDialog, string nextDialog)
        {
            string wixSharpAsm = typeof(Project).Assembly.Location;
            string wixSharpUIAsm = IO.Path.ChangeExtension(wixSharpAsm, ".UI.dll");

            var showClrDialog = new ManagedAction(showDialogMethod)
            {
                Sequence = Sequence.NotInSequence,
            };

            project.DefaultRefAssemblies.Add(wixSharpAsm);
            project.DefaultRefAssemblies.Add(wixSharpUIAsm);

            //Must use WixUI_Common as other UI type has predefined dialogs already linked between each other and WiX does not allow overriding events
            //http://stackoverflow.com/questions/16961493/override-publish-within-uiref-in-wix
            project.UI = WUI.WixUI_Common;

            if (project.CustomUI != null)
                throw new ApplicationException("Project.CustomUI is already initialized. Ensure InjectClrDialog is invoked before any adjustments made to CustomUI.");

            project.CustomUI = new CommomDialogsUI();
            project.Actions = project.Actions.Add(showClrDialog);

            //disconnect prev and next dialogs
            project.CustomUI.UISequence.ForEach(x =>
                                        {
                                            if ((x.Dialog == prevDialog && x.Control == Buttons.Next) || (x.Dialog == nextDialog && x.Control == Buttons.Back))
                                                x.Actions.RemoveAll(a => a is ShowDialog);
                                        });
            project.CustomUI.UISequence.RemoveAll(x => x.Actions.Count == 0);

            //create new dialogs connection with showAction in between
            project.CustomUI.On(prevDialog, Buttons.Next, new ExecuteCustomAction(showClrDialog))
                            .On(prevDialog, Buttons.Next, new ShowDialog(nextDialog, Condition.ClrDialog_NextPressed))
                            .On(prevDialog, Buttons.Next, new CloseDialog("Exit", Condition.ClrDialog_CancelPressed) { Order = 2 })

                            .On(nextDialog, Buttons.Back, new ExecuteCustomAction(showClrDialog))
                            .On(nextDialog, Buttons.Back, new ShowDialog(prevDialog, Condition.ClrDialog_BackPressed));

            var installDir = project.AllDirs.FirstOrDefault(d => d.HastemsToInstall());
            if (installDir != null && project.CustomUI.Properties.ContainsKey("WIXUI_INSTALLDIR"))
                project.CustomUI.Properties["WIXUI_INSTALLDIR"] = installDir.RawId ?? Compiler.AutoGeneration.InstallDirDefaultId;

            return project;
        }

        //not ready yet. Investigation is in progress
        static internal Project InjectClrDialogInFeatureTreeUI(this Project project, string showDialogMethod, string prevDialog, string nextDialog)
        {
            string wixSharpAsm = typeof(Project).Assembly.Location;
            string wixSharpUIAsm = IO.Path.ChangeExtension(wixSharpAsm, ".UI.dll");

            var showClrDialog = new ManagedAction(showDialogMethod)
            {
                Sequence = Sequence.NotInSequence,
                RefAssemblies = new[] { wixSharpAsm, wixSharpUIAsm }
            };

            project.UI = WUI.WixUI_FeatureTree;

            if (project.CustomUI != null)
                throw new ApplicationException("Project.CustomUI is already initialized. Ensure InjectClrDialog is invoked before any adjustments made to CustomUI.");

            project.CustomUI = new DialogSequence();
            project.Actions = project.Actions.Add(showClrDialog);

            //disconnect prev and next dialogs
            project.CustomUI.UISequence.RemoveAll(x => (x.Dialog == prevDialog && x.Control == Buttons.Next) ||
                                                 (x.Dialog == nextDialog && x.Control == Buttons.Back));

            //create new dialogs connection with showAction in between
            project.CustomUI.On(prevDialog, Buttons.Next, new ExecuteCustomAction(showClrDialog))
                            .On(prevDialog, Buttons.Next, new ShowDialog(nextDialog, Condition.ClrDialog_NextPressed))
                            .On(prevDialog, Buttons.Next, new CloseDialog("Exit", Condition.ClrDialog_CancelPressed) { Order = 2 })

                            .On(nextDialog, Buttons.Back, new ExecuteCustomAction(showClrDialog))
                            .On(nextDialog, Buttons.Back, new ShowDialog(prevDialog, Condition.ClrDialog_BackPressed));

            return project;
        }

        /// <summary>
        /// Gets the file version.
        /// </summary>
        /// <param name="file">The path to the file.</param>
        /// <returns></returns>
        static public Version GetFileVersion(string file)
        {
            var info = FileVersionInfo.GetVersionInfo(file);
            //cannot use info.FileVersion as it can include description string
            return new Version(info.FileMajorPart,
                               info.FileMinorPart,
                               info.FileBuildPart,
                               info.FilePrivatePart);
        }

        /// <summary>
        /// Binds the LaunchCondition to the automatically created REQUIRED_NET property which is set to the value of the
        /// Software\Microsoft\NET Framework Setup\NDP\{version}\Install registry entry.
        /// <para>It is a single step equivalent of the "Wix# Samples\LaunchConditions" sample.</para>
        /// <para>Note that the value of the version parameter is a precise sub-key of the corresponding 'Install' registry entry
        /// (e.g. as in 'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4.0\Client\Install').</para>
        /// <para>The typical values are:</para>
        /// <para>   v2.0.50727</para>
        /// <para>   v3.0</para>
        /// <para>   v3.5</para>
        /// <para>   v4\Client</para>
        /// <para>   v4\Full</para>
        /// <para>   v4.0\Client</para>
        /// <para>   ...</para>
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="version">The sub-key of the registry entry of the sub-key of the corresponding registry version of .NET to be checked.
        /// </param>
        /// <param name="errorMessage">The error message to be displayed if .NET version is not present.</param>
        /// <returns></returns>
        [Obsolete("SetClrPrerequisite is obsolete. Please use more reliable SetNetFxPrerequisite instead.", true)]
        static public Project SetClrPrerequisite(this WixSharp.Project project, string version, string errorMessage = null)
        {
            string message = errorMessage ?? "Please install .NET " + version + " first.";

            project.LaunchConditions.Add(new LaunchCondition("REQUIRED_NET=\"#1\"", message));
            project.Properties = project.Properties.Add(new RegValueProperty("REQUIRED_NET",
                                                                              RegistryHive.LocalMachine,
                                                                              @"Software\Microsoft\NET Framework Setup\NDP\" + version,
                                                                              "Install", "0"));
            return project;
        }

        /// <summary>
        /// Binds the LaunchCondition to the <c>version</c> condition based on WiXNetFxExtension properties.
        /// <para>The typical conditions are:</para>
        /// <para>   NETFRAMEWORK20="#1"</para>
        /// <para>   NETFRAMEWORK40FULL="#1"</para>
        /// <para>   NETFRAMEWORK35="#1"</para>
        /// <para>   NETFRAMEWORK30_SP_LEVEL and NOT NETFRAMEWORK30_SP_LEVEL='#0'</para>
        /// <para>   ...</para>
        /// The full list of names and values can be found here http://wixtoolset.org/documentation/manual/v3/customactions/wixnetfxextension.html
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="versionCondition">Condition expression.
        /// </param>
        /// <param name="errorMessage">The error message to be displayed if .NET version is not present.</param>
        /// <returns></returns>
        static public Project SetNetFxPrerequisite(this WixSharp.Project project, string versionCondition, string errorMessage = null)
        {
            var condition = Condition.Create(versionCondition);
            string message = errorMessage ?? "Please install the appropriate .NET version first.";

            project.LaunchConditions.Add(new LaunchCondition(condition, message));

            foreach (var prop in condition.GetDistinctProperties())
                project.Properties = project.Properties.Add(new PropertyRef(prop));

            project.IncludeWixExtension(WixExtension.NetFx);

            return project;
        }

        /// <summary>
        /// Sets the value of the attribute value in the .NET application configuration file according
        /// the specified XPath expression.
        /// <para>
        /// This simple routine is to be used for the customization of the installed config files
        /// (e.g. in the deferred custom actions).
        /// </para>
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <param name="elementPath">The element XPath value. It should include the attribute name.</param>
        /// <param name="value">The value to be set to the attribute.</param>
        ///
        /// <example>The following is an example demonstrates this simple technique:
        /// <code>
        ///  Tasks.SetConfigAttribute(configFile, "//configuration/appSettings/add[@key='AppName']/@value", "My App");
        /// </code>
        /// </example>
        static public void SetConfigAttribute(string configFile, string elementPath, string value)
        {
            XDocument.Load(configFile)
                     .Root
                     .SetConfigAttribute(elementPath, value)
                     .Document
                     .Save(configFile);
        }

        /// <summary>
        /// Sets the value of the attribute value in the .NET application configuration file according
        /// the specified XPath expression.
        /// <para>
        /// This simple routine is to be used for the customization of the installed config files
        /// (e.g. in the deferred custom actions).
        /// </para>
        /// </summary>
        /// <returns></returns>
        /// <param name="config">The configuration file element.</param>
        /// <param name="elementPath">The element XPath value. It should include the attribute name.</param>
        /// <param name="value">The value to be set to the attribute.</param>
        ///
        /// <example>The following is an example demonstrates this simple technique:
        /// <code>
        ///  XDocument.Load(configFile).Root
        ///           .SetConfigAttribute("//configuration/appSettings/add[@key='AppName']/@value", "My App")
        ///           .SetConfigAttribute(...
        ///           .SetConfigAttribute(...
        ///           .Document.Save(configFile);
        /// </code>
        /// </example>
        static public XElement SetConfigAttribute(this XElement config, string elementPath, string value)
        {
            var valueAttr = ((IEnumerable)config.XPathEvaluate(elementPath)).Cast<XAttribute>().FirstOrDefault();

            if (valueAttr != null)
                valueAttr.Value = value;
            return config;
        }

        /// <summary>
        /// Installs the windows service. It uses InstallUtil.exe to complete the actual installation/uninstallation.
        /// During the run for the InstallUtil.exe console window is hidden. /// If any error occurred the console output is captured and embedded into the raised Exception object.
        /// </summary>
        /// <param name="serviceFile">The service file.</param>
        /// <param name="isInstalling">if set to <c>true</c> [is installing].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        static public string InstallService(string serviceFile, bool isInstalling)
        {
            var util = new ExternalTool
            {
                ExePath = IO.Path.Combine(LatestFrameworkDirectory, "InstallUtil.exe"),
                Arguments = string.Format("{1} \"{0}\"", serviceFile, isInstalling ? "" : "/u")
            };

            var buf = new StringBuilder();
            int retval = util.ConsoleRun(line => buf.AppendLine(line));
            string output = buf.ToString();

            string logoLastLine = "Microsoft Corporation.  All rights reserved.";
            int pos = output.IndexOf(logoLastLine);
            if (pos != -1)
                output = output.Substring(pos + logoLastLine.Length).Trim();

            if (retval != 0)
                throw new Exception(output);

            return output;
        }

        /// <summary>
        /// Starts the windows service. It uses sc.exe to complete the action.  During the action console window is hidden.
        /// If any error occurred the console output is captured and embedded into the raised Exception object.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns></returns>
        static public string StartService(string service, bool throwOnError = true)
        {
            return ServiceDo("start", service, throwOnError);
        }

        /// <summary>
        /// Stops the windows service. It uses sc.exe to complete the action.  During the action console window is hidden.
        /// If any error occurred the console output is captured and embedded into the raised Exception object.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns></returns>
        static public string StopService(string service, bool throwOnError = true)
        {
            return ServiceDo("stop", service, throwOnError);
        }

        static string ServiceDo(string action, string service, bool throwOnError)
        {
            var util = new ExternalTool { ExePath = "sc.exe", Arguments = action + " \"" + service + "\"" };

            var buf = new StringBuilder();
            int retval = util.ConsoleRun(line => buf.AppendLine(line));

            if (retval != 0 && throwOnError)
                throw new Exception(buf.ToString());
            return buf.ToString();
        }

        /// <summary>
        /// Gets the directory of the latest installed verion of .NET framework.
        /// </summary>
        /// <value>
        /// The latest framework directory.
        /// </value>
        public static string LatestFrameworkDirectory
        {
            get
            {
                string currentVersionDir = IO.Path.GetDirectoryName(typeof(string).Assembly.Location);
                string rootDir = IO.Path.GetDirectoryName(currentVersionDir);
                return IO.Directory.GetDirectories(rootDir, "v*.*")
                                   .OrderByDescending(x => x)
                                   .FirstOrDefault();
            }
        }
    }

}

internal class ExternalTool
{
    public string ExePath { set; get; }

    public string Arguments { set; get; }

    public string WellKnownLocations { set; get; }

    public int WinRun()
    {
        string systemPathOriginal = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", systemPathOriginal + ";" + Environment.ExpandEnvironmentVariables(this.WellKnownLocations ?? ""));

            var process = new Process();
            process.StartInfo.FileName = this.ExePath;
            process.StartInfo.Arguments = this.Arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            process.WaitForExit();
            return process.ExitCode;
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", systemPathOriginal);
        }
    }

    public int ConsoleRun()
    {
        return ConsoleRun(Console.WriteLine);
    }

    public int ConsoleRun(Action<string> onConsoleOut)
    {
        string systemPathOriginal = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", Environment.ExpandEnvironmentVariables(this.WellKnownLocations ?? "") + ";" + "%WIXSHARP_PATH%;" + systemPathOriginal);

            string exePath = GetFullPath(this.ExePath);

            if (exePath == null)
            {
                Console.WriteLine("Error: Cannot find " + this.ExePath);
                Console.WriteLine("Make sure it is in the System PATH or WIXSHARP_PATH environment variables or WellKnownLocations member/parameter is initialized properly. ");
                return 1;
            }

            Console.WriteLine("Execute:\n\"" + this.ExePath + "\" " + this.Arguments);

            var process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = this.Arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.CreateNoWindow = true;
            process.Start();

            if (onConsoleOut != null)
            {
                string line = null;
                while (null != (line = process.StandardOutput.ReadLine()))
                {
                    onConsoleOut(line);
                }

                string error = process.StandardError.ReadToEnd();
                if (!error.IsEmpty())
                    onConsoleOut(error);
            }
            process.WaitForExit();
            return process.ExitCode;
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", systemPathOriginal);
        }
    }

    string GetFullPath(string path)
    {
        if (IO.File.Exists(path))
            return IO.Path.GetFullPath(path);

        foreach (string dir in Environment.GetEnvironmentVariable("PATH").Split(';'))
        {
            if (IO.Directory.Exists(dir))
            {
                string fullPath = IO.Path.Combine(Environment.ExpandEnvironmentVariables(dir).Trim(), path);
                if (IO.File.Exists(fullPath))
                    return fullPath;
            }
        }

        return null;
    }
}

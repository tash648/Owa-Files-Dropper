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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IO = System.IO;
using Path = System.IO.Path;

namespace WixSharp
{
    /// <summary>
    /// Automatically insert elements required for satisfy odd MSI restrictions.
    /// <para>- You must set KeyPath you install in the user profile.</para>
    /// <para>- You must use a registry key under HKCU as component's KeyPath, not a file. </para>
    /// <para>- The Component element cannot have multiple key path set.  </para>
    /// <para>- The project must have at least one directory element.  </para>
    /// <para>- All directories installed in the user profile must have corresponding RemoveDirectory
    /// elements.  </para>
    /// <para>...</para>
    /// <para>
    /// The MSI always wants registry keys as the key paths for per-user components.
    /// It has to do with the way profiles work with advertised content in enterprise deployments.
    /// The fact that you do not want to install any registry doesn't matter. MSI is the boss.
    /// </para>
    /// <para>The following link is a good example of the technique:
    /// http://stackoverflow.com/questions/16119708/component-testcomp-installs-to-user-profile-it-must-use-a-registry-key-under-hk</para>
    /// </summary>
    public static class AutoElements
    {
        /// <summary>
        /// The disable automatic insertion of <c>CreateFolder</c> element.
        /// Required for: NativeBootstrapper, EmbeddedMultipleActions,  EmptyDirectories, InstallDir, Properties,
        /// ReleaseFolder, Shortcuts and WildCardFiles samples.
        /// <para>Can also be managed by disabling ICE validation via Light.exe command line arguments.</para>
        /// <para>
        /// This flag is a heavier alternative of DisableAutoKeyPath. 
        /// See: http://stackoverflow.com/questions/10358989/wix-using-keypath-on-components-directories-files-registry-etc-etc 
        /// for some background info.
        ///  
        /// </para>
        /// </summary>
        public static bool DisableAutoCreateFolder = true;

        /// <summary>
        /// The disable automatic insertion of <c>KeyPath=yes</c> attribute for the Component element.
        /// Required for: NativeBootstrapper, EmbeddedMultipleActions,  EmptyDirectories, InstallDir, Properties,
        /// ReleaseFolder, Shortcuts and WildCardFiles samples.
        /// <para>Can also be managed by disabling ICE validation via Light.exe command line arguments.</para>
        /// <para>
        /// This flag is a lighter alternative of DisableAutoCreateFolder. 
        /// See: http://stackoverflow.com/questions/10358989/wix-using-keypath-on-components-directories-files-registry-etc-etc 
        /// for some background info.
        /// </para>
        /// </summary>
        public static bool DisableAutoKeyPath = false;

        /// <summary>
        /// Disables automatic insertion of user profile registry elements.
        /// Required for: AllInOne, ConditionalInstallation, CustomAttributes, ReleaseFolder, Shortcuts,
        /// Shortcuts (advertised), Shortcuts-2, WildCardFiles samples.
        /// <para>Can also be managed by disabling ICE validation via Light.exe command line arguments.</para>
        /// </summary>
        public static bool DisableAutoUserProfileRegistry = false;

        static void InsertRemoveFolder(XElement xDir, XElement xComponent, string when = "uninstall")
        {
            if (!xDir.IsUserProfileRoot())
                xComponent.Add(new XElement("RemoveFolder",
                                   new XAttribute("Id", xDir.Attribute("Id").Value),
                                   new XAttribute("On", when)));
        }

        internal static XElement InsertUserProfileRemoveFolder(this XElement xComponent)
        {
            var xDir = xComponent.Parent("Directory");
            if (!xDir.Descendants("RemoveFolder").Any() && !xDir.IsUserProfileRoot())
                xComponent.Add(new XElement("RemoveFolder",
                                   new XAttribute("Id", xDir.Attribute("Id").Value),
                                   new XAttribute("On", "uninstall")));

            return xComponent;
        }

        static void InsertCreateFolder(XElement xComponent)
        {
            //"Empty Directories" sample demonstrates the need for CreateFolder
            if (!DisableAutoCreateFolder)
            {
                //prevent adding more than 1 CreateFolder elements - elements that don't specify @Directory
                if (xComponent.Elements("CreateFolder")
                              .All(element => element.HasAttribute("Directory")))
                    xComponent.Add(new XElement("CreateFolder"));
            }

            if (!DisableAutoKeyPath)
            {
                //a component must have KeyPath set on itself or on a single (just one) nested element
                if (!xComponent.HasKeyPathElements())
                    xComponent.SetAttribute("KeyPath=yes");
            }
        }


        internal static bool HasKeyPathElements(this XElement xComponent)
        {
            return xComponent.Descendants()
                             .Where(e => e.HasKeyPathSet())
                             .Any();
        }

        internal static XElement ClearKeyPath(this XElement element)
        {
            return element.SetAttribute("KeyPath", null);
        }

        internal static bool HasKeyPathSet(this XElement element)
        {
            var attr = element.Attribute("KeyPath");

            if (attr != null && attr.Value == "yes")
                return true;
            return false;
        }

        internal static XElement InsertUserProfileRegValue(this XElement xComponent)
        {
            //UserProfileRegValue has to be a KeyPath fo need to remove any KeyPath on other elements
            var keyPathes = xComponent.Descendants()
                                      .ForEach(e => e.ClearKeyPath());

            xComponent.ClearKeyPath();

            xComponent.Add(
                        new XElement("RegistryKey",
                            new XAttribute("Root", "HKCU"),
                            new XAttribute("Key", @"Software\WixSharp\Used"),
                            new XElement("RegistryValue",
                                new XAttribute("Value", "0"),
                                new XAttribute("Type", "string"),
                                new XAttribute("KeyPath", "yes")))); 
            return xComponent;
        }

        static void InsertDummyUserProfileRegistry(XElement xComponent)
        {
            if (!DisableAutoUserProfileRegistry)
            {
                InsertUserProfileRegValue(xComponent);
            }
        }

        static void SetFileKeyPath(XElement element, bool isKeyPath = true)
        {
            if (element.Attribute("KeyPath") == null)
                element.Add(new XAttribute("KeyPath", isKeyPath ? "yes" : "no"));
        }

        static bool ContainsDummyUserProfileRegistry(this XElement xComponent)
        {
            return (from e in xComponent.Elements("RegistryKey")
                    where e.Attribute("Key") != null && e.Attribute("Key").Value == @"Software\WixSharp\Used"
                    select e).Count() != 0;
        }

        static bool ContainsAnyRemoveFolder(this XElement xDir)
        {
            return (xDir.Descendants("RemoveFolder").Count() != 0);
        }

        static bool ContainsFiles(this XElement xComp)
        {
            return xComp.Elements("File").Count() != 0;
        }

        static bool ContainsComponents(this XElement xDir)
        {
            return xDir.Elements("Component").Any();
        }

        static bool ContainsAdvertisedShortcuts(this XElement xComp)
        {
            var advertisedShortcuts = from e in xComp.Descendants("Shortcut")
                                      where e.Attribute("Advertise") != null && e.Attribute("Advertise").Value == "yes"
                                      select e;

            return (advertisedShortcuts.Count() != 0);
        }

        static bool ContainsNonAdvertisedShortcuts(this XElement xComp)
        {
            var nonAdvertisedShortcuts = from e in xComp.Descendants("Shortcut")
                                         where e.Attribute("Advertise") == null || e.Attribute("Advertise").Value == "no"
                                         select e;

            return (nonAdvertisedShortcuts.Count() != 0);
        }

        static XElement CrteateComponentFor(this XDocument doc, XElement xDir)
        {
            string compId = xDir.Attribute("Id").Value;
            XElement xComponent = xDir.AddElement(
                              new XElement("Component",
                                  new XAttribute("Id", compId),
                                  new XAttribute("Guid", WixGuid.NewGuid(compId))));

            foreach (XElement xFeature in doc.Root.Descendants("Feature"))
                xFeature.Add(new XElement("ComponentRef",
                    new XAttribute("Id", xComponent.Attribute("Id").Value)));

            return xComponent;
        }

        private static string[] GetUserProfileFolders()
        {
            return new[]
                    {
                        "ProgramMenuFolder",
                        "AppDataFolder",
                        "LocalAppDataFolder",
                        "TempFolder",
                        "PersonalFolder",
                        "DesktopFolder"
                    };
        }

        static bool InUserProfile(this XElement xDir)
        {
            string[] userProfileFolders = GetUserProfileFolders();

            XElement xParentDir = xDir;
            do
            {
                if (xParentDir.Name == "Directory")
                {
                    var attrName = xParentDir.Attribute("Name").Value;

                    if (userProfileFolders.Contains(attrName))
                        return true;
                }
                xParentDir = xParentDir.Parent;
            }
            while (xParentDir != null);

            return false;
        }

        static bool IsUserProfileRoot(this XElement xDir)
        {
            string[] userProfileFolders = GetUserProfileFolders();

            return userProfileFolders.Contains(xDir.Attribute("Name").Value);
        }

        internal static void InjectShortcutIcons(XDocument doc)
        {
            var shortcuts = from s in doc.Root.Descendants("Shortcut")
                            where s.HasAttribute("Icon")
                            select s;

            int iconIndex = 1;

            var icons = new Dictionary<string, string>();
            foreach (var iconFile in (from s in shortcuts
                                      select s.Attribute("Icon").Value).Distinct())
            {
                icons.Add(iconFile,
                    "IconFile" + (iconIndex++) + "_" + IO.Path.GetFileName(iconFile).Expand());
            }

            foreach (XElement shortcut in shortcuts)
            {
                string iconFile = shortcut.Attribute("Icon").Value;
                string iconId = icons[iconFile];
                shortcut.Attribute("Icon").Value = iconId;
            }

            XElement product = doc.Root.Select("Product");

            foreach (string file in icons.Keys)
                product.AddElement(
                    new XElement("Icon",
                        new XAttribute("Id", icons[file]),
                        new XAttribute("SourceFile", file)));
        }

        static void InjectPlatformAttributes(XDocument doc)
        {
            var is64BitPlatform = doc.Root.Select("Product/Package").HasAttribute("Platform", val => val == "x64");

            if (is64BitPlatform)
                doc.Descendants("Component")
                   .ForEach(comp => comp.SetAttributeValue("Win64", "yes"));
        }

        static void ExpandCustomAttributes(XDocument doc)
        {
            foreach (XAttribute instructionAttr in doc.Root.Descendants().Select(x => x.Attribute("WixSharpCustomAttributes")).Where(x => x != null))
            {
                XElement sourceElement = instructionAttr.Parent;

                foreach (string item in instructionAttr.Value.Split(';'))
                    if (item.IsNotEmpty())
                    {
                        if (!ExpandCustomAttribute(sourceElement, item))
                            throw new ApplicationException("Cannot resolve custom attribute definition:" + item);
                    }

                instructionAttr.Remove();
            }
        }

        static Func<XElement, string, bool> ExpandCustomAttribute = DefaultExpandCustomAttribute;

        static bool DefaultExpandCustomAttribute(XElement source, string item)
        {
            var attrParts = item.Split('=');
            var keyParts = attrParts.First().Split(':');

            string element = keyParts.First();
            string key = keyParts.Last();
            string value = attrParts.Last();

            if (element == "Component")
            {
                XElement destElement = source.Parent("Component");
                if (destElement != null)
                {
                    destElement.SetAttributeValue(key, value);
                    return true;
                }
            }

            if (element == "Icon" && source.Name.LocalName == "Property")
            {
                source.Parent("Product")
                      .SelectOrCreate("Icon")
                      .SetAttributeValue(key, value);
                return true;
            }

            if (element == "Custom" && source.Name.LocalName == "CustomAction")
            {
                string id = source.Attribute("Id").Value;
                var elements = source.Document.Descendants("Custom").Where(e => e.Attribute("Action").Value == id);
                if (elements.Any())
                {
                    elements.ForEach(e => e.SetAttributeValue(key, value));
                    return true;
                }
            }

            if (key.StartsWith("xml_include"))
            {
                var parts = value.Split('|');

                string parentName = parts[0];
                string xmlFile = parts[1];

                var placement = source;
                if (!parentName.IsEmpty())
                    placement = source.Parent(parentName);

                if (placement != null)
                {
                    placement.Add(new XProcessingInstruction("include", xmlFile));
                    return true;
                }
            }

            return false;
        }

        internal static void InjectAutoElementsHandler(XDocument doc)
        {
            InjectPlatformAttributes(doc);
            ExpandCustomAttributes(doc);
            InjectShortcutIcons(doc);

            XElement installDir = doc.Root.Select("Product").Element("Directory").Element("Directory");

            XAttribute installDirName = installDir.Attribute("Name");
            if (IO.Path.IsPathRooted(installDirName.Value))
            {
                var product = installDir.Parent("Product");
                string absolutePath = installDirName.Value;

                installDirName.Value = "ABSOLUTEPATH";

                //<SetProperty> is an attractive approach but it doesn't allow conditional setting of 'ui' and 'execute' as required depending on UI level
                // it is ether hard coded 'both' or hard coded both 'ui' or 'execute'
                // <SetProperty Id="INSTALLDIR" Value="C:\My Company\MyProduct" Sequence="both" Before="AppSearch">

                product.Add(new XElement("CustomAction",
                                new XAttribute("Id", "Set_INSTALLDIR_AbsolutePath"),
                                new XAttribute("Property", installDir.Attribute("Id").Value),
                                new XAttribute("Value", absolutePath)));

                product.SelectOrCreate("InstallExecuteSequence").Add(
                       new XElement("Custom", "(NOT Installed) AND (UILevel < 5)",
                           new XAttribute("Action", "Set_INSTALLDIR_AbsolutePath"),
                           new XAttribute("Before", "CostFinalize")));

                product.SelectOrCreate("InstallUISequence").Add(
                      new XElement("Custom", "(NOT Installed) AND (UILevel = 5)",
                          new XAttribute("Action", "Set_INSTALLDIR_AbsolutePath"),
                          new XAttribute("Before", "CostFinalize")));
            }

            foreach (XElement xDir in doc.Root.Descendants("Directory").ToArray())
            {
                var dirComponents = xDir.Elements("Component");

                if (dirComponents.Any())
                {
                    var componentsWithNoFiles = dirComponents.Where(x => !x.ContainsFiles()).ToArray();

                    foreach (XElement item in componentsWithNoFiles)
                    {
                        if (!item.Attribute("Id").Value.EndsWith(".EmptyDirectory"))
                            InsertCreateFolder(item);
                        else if (!xDir.ContainsAnyRemoveFolder())
                            InsertRemoveFolder(xDir, item, "both"); //to keep WiX/compiler happy and allow removal of the dummy directory
                    }
                }

                foreach (XElement xComp in dirComponents)
                {
                    if (xDir.InUserProfile())
                    {
                        if (!xDir.ContainsAnyRemoveFolder())
                            InsertRemoveFolder(xDir, xComp);

                        if (!xComp.ContainsDummyUserProfileRegistry())
                            InsertDummyUserProfileRegistry(xComp);
                    }
                    else
                    {
                        if (xComp.ContainsNonAdvertisedShortcuts())
                            if (!xComp.ContainsDummyUserProfileRegistry())
                                InsertDummyUserProfileRegistry(xComp);
                    }

                    foreach (XElement xFile in xComp.Elements("File"))
                        if (xFile.ContainsAdvertisedShortcuts() && !xComp.ContainsDummyUserProfileRegistry())
                            SetFileKeyPath(xFile);
                }

                if (!xDir.ContainsComponents() && xDir.InUserProfile())
                {
                    if (!xDir.IsUserProfileRoot())
                    {
                        XElement xComp1 = doc.CrteateComponentFor(xDir);
                        if (!xDir.ContainsAnyRemoveFolder())
                            InsertRemoveFolder(xDir, xComp1);

                        if (!xComp1.ContainsDummyUserProfileRegistry())
                            InsertDummyUserProfileRegistry(xComp1);
                    }
                }
            }
        }


        internal static void NormalizeFilePaths(XDocument doc, string sourceBaseDir, bool emitRelativePaths)
        {
            string rootDir = sourceBaseDir;
            if (rootDir.IsEmpty())
                rootDir = Environment.CurrentDirectory;

            rootDir = IO.Path.GetFullPath(rootDir);

            Action<IEnumerable<XElement>, string> normalize = (elements, attributeName) =>
                {
                    elements.Where(e => e.HasAttribute(attributeName))
                            .ForEach(e =>
                                {
                                    var attr = e.Attribute(attributeName);
                                    if (emitRelativePaths)
                                        attr.Value = Utils.MakeRelative(attr.Value, rootDir);
                                    else
                                        attr.Value = Path.GetFullPath(attr.Value);
                                });

                };

            normalize(doc.Root.FindAll("Icon"), "SourceFile");
            normalize(doc.Root.FindAll("File"), "Source");
            normalize(doc.Root.FindAll("Merge"), "SourceFile");
            normalize(doc.Root.FindAll("Binary"), "SourceFile");
            normalize(doc.Root.FindAll("EmbeddedUI"), "SourceFile");
            normalize(doc.Root.FindAll("Payload"), "SourceFile");
            normalize(doc.Root.FindAll("MsiPackage"), "SourceFile");
            normalize(doc.Root.FindAll("ExePackage"), "SourceFile");
        }
    }
}

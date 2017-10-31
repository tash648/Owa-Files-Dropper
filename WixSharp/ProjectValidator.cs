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
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using IO = System.IO;
using Reflection = System.Reflection;

namespace WixSharp
{
    class ProjectValidator
    {
        static bool IsValidVersion(string versionText)
        {
            if (string.IsNullOrEmpty(versionText))
                return true;

            if (versionText == "%this%")
                return true;

            try
            {
                new Version(versionText);
                return true;
            }
            catch
            { return false; }
        }

        public static void Validate(Project project)
        {
            if (project.MajorUpgradeStrategy != null)
            {
                if (project.MajorUpgradeStrategy.UpgradeVersions == null && project.MajorUpgradeStrategy.PreventDowngradingVersions == null)
                {
                    throw new UpgradeStrategyValidationException("Project MajorUpgradeStrategy.UpgradeVersions and PreventDowngradingVersions are not defined.");
                }

                if (project.MajorUpgradeStrategy.UpgradeVersions != null)
                {
                    if (!IsValidVersion(project.MajorUpgradeStrategy.UpgradeVersions.Minimum))
                        throw new UpgradeStrategyValidationException("Project MajorUpgradeStrategy.UpgradeVersions.Minimum value is invalid.");

                    if (!IsValidVersion(project.MajorUpgradeStrategy.UpgradeVersions.Maximum))
                        throw new UpgradeStrategyValidationException("Project MajorUpgradeStrategy.UpgradeVersions.Maximum value is invalid.");
                }

                if (project.MajorUpgradeStrategy.PreventDowngradingVersions != null)
                {
                    if (!IsValidVersion(project.MajorUpgradeStrategy.PreventDowngradingVersions.Minimum))
                        throw new UpgradeStrategyValidationException("Project MajorUpgradeStrategy.PreventDowngradingVersions.Minimum value is invalid.");

                    if (!IsValidVersion(project.MajorUpgradeStrategy.PreventDowngradingVersions.Maximum))
                        throw new UpgradeStrategyValidationException("Project MajorUpgradeStrategy.PreventDowngradingVersions.Maximum value is invalid.");
                }
            }

            foreach (Dir dir in project.AllDirs)
                if (dir.Name.StartsWith("%") || dir.Name.EndsWith("%"))
                    if (!Compiler.EnvironmentConstantsMapping.ContainsKey(dir.Name))
                        throw new ValidationException("WixSharp.Dir.Name is set to unknown environment constant '" + dir.Name + "'.\n" +
                                                      "For the list of supported constants analyze WixSharp.Compiler.EnvironmentConstantsMapping.Keys.");


            var incosnistentRefAsmActions =
                      project.Actions.OfType<ManagedAction>()
                                     .GroupBy(a => a.ActionAssembly)
                                     .Where(g => g.Count() > 1)
                                     .Select(g => new
                                     {
                                         Assembly = g.Key,
                                         Info = g.Select(a => new { Name = a.MethodName, RefAsms = a.RefAssemblies.Select(r => Path.GetFileName(r)).ToArray() }).ToArray(),
                                         IsInconsistent = g.Select(action => action.GetRefAssembliesHashCode(project.DefaultRefAssemblies)).Distinct().Count() > 1,
                                     })
                                     .Where(x => x.IsInconsistent)
                                     .FirstOrDefault();

            if (incosnistentRefAsmActions != null)
            {
                var errorInfo = new StringBuilder();
                errorInfo.Append(">>>>>>>>>>>>\n");
                errorInfo.Append("Asm: " + incosnistentRefAsmActions.Assembly + "\n");
                foreach (var item in incosnistentRefAsmActions.Info)
                {
                    errorInfo.Append("    ----------\n");
                    errorInfo.Append("    Action: " + item.Name+"\n");
                    errorInfo.AppendFormat("    RefAsms: {0} items\n", item.RefAsms.Length);
                    foreach (var name in item.RefAsms)
                        errorInfo.Append("       - " + name + "\n");
                }
                errorInfo.Append(">>>>>>>>>>>>\n");

                throw new ApplicationException(string.Format("Assembly '{0}' is used by multiple ManagedActions but with the inconsistent set of referenced assemblies. " +
                                                             "Ensure that all declarations have the same referenced assemblies by either using identical declarations or by using " +
                                                             "Project.DefaultRefAssemblies.\n{1}", incosnistentRefAsmActions.Assembly, errorInfo));
            }

            //https://wixsharp.codeplex.com/discussions/646085
            //Have to disable validation as it only considers 'install' but not 'uninstall'.
            //Possible solution is to annalyse the action.condition and determine if it is 
            //install /uninstall but it is impossible to do. At least with the adequate accuracy.  
            //var incosnistentInstalledFileActions = project.Actions
                                                          //.OfType<InstalledFileAction>()
                                                          //.Where(x => x.When != When.After || x.Step != Step.InstallExecute)
                                                          //.Any();
            //if (incosnistentInstalledFileActions)
                //try
                //{
                    //var msg = "Warning: InstalledFileAction should be scheduled for after InstallExecute. Otherwise it may produce undesired side effects.";
                    //Debug.WriteLine(msg);
                    //Console.WriteLine(msg);
                //}
                //catch { }
        }
        
        public static void ValidateCAAssembly(string file)
        {
            //need to do it in a separate domain as we do not want to lock the assembly
            Utils.ExecuteInTempDomain<AsmReflector>(asmReflector =>
                {
                    asmReflector.ValidateCAAssembly(file, typeof(CustomActionAttribute).Assembly.Location);
                });
        }

        public static void ValidateAssemblyCompatibility(Reflection.Assembly assembly)
        {
            //this validation is no longer crytical as Wix# MAnagedSetup now fully supports .NET4.0
            //if (!assembly.ImageRuntimeVersion.StartsWith("v2."))
            //    try
            //    {
            //        var msg = string.Format("Warning: assembly '{0}' is compiled for {1} runtime, which may not be compatible with the CLR version hosted by MSI. "+
            //                                "The incompatibility is particularly possible for the Embedded UI scenarios. " +
            //                                "The safest way to solve the problem is to compile the assembly for v3.5 Target Framework.",
            //                                assembly.GetName().Name, assembly.ImageRuntimeVersion);
            //        Debug.WriteLine(msg);
            //        Console.WriteLine(msg);
            //    }
            //    catch { }
        }
    }

    class AsmReflector : MarshalByRefObject
    {
        public string OriginalAssemblyFile(string file)
        {
            string dir = IO.Path.GetDirectoryName(IO.Path.GetFullPath(file));

            System.Reflection.Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            {
                try
                {
                    return a.Location.SamePathAs(file); //some domain assemblies may throw when accessing .Locatioon
                }
                catch
                {
                    return false;
                }
            });

            if (asm == null)
                asm = System.Reflection.Assembly.ReflectionOnlyLoadFrom(file);

            return IO.Path.Combine(dir, asm.ManifestModule.ScopeName);
        }

        public void ValidateCAAssembly(string file, string dtfAsm)
        {
            ResolveEventHandler resolver = (sender, args) =>
            {
                return System.Reflection.Assembly.LoadFrom(dtfAsm);
            };

            AppDomain.CurrentDomain.AssemblyResolve += resolver;
            ValidateCAAssemblyImpl(file, dtfAsm);
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;
        }

        void ValidateCAAssemblyImpl(string file, string refAsms)
        {
            //Debug.Assert(false);
            try
            {
                var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static;

                //var assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(file); //cannot prelaod all required assemblies
                var assembly = System.Reflection.Assembly.LoadFrom(file);

                var caMembers = assembly.GetTypes().SelectMany(t =>
                                        t.GetMembers(bf)
                                         .Where(mem =>
                                                mem.GetCustomAttributes(false)
                                                   .Where(x => x.ToString() == "Microsoft.Deployment.WindowsInstaller.CustomActionAttribute").Any())).ToArray();

                var invalidMembers = new List<string>();
                foreach (MemberInfo mi in caMembers)
                {
                    string fullName = mi.DeclaringType.FullName + "." + mi.Name;

                    if (!mi.DeclaringType.IsPublic)
                        if (!invalidMembers.Contains(fullName))
                            invalidMembers.Add(fullName);

                    if (mi.MemberType != MemberTypes.Method)
                    {
                        if (!invalidMembers.Contains(fullName))
                            invalidMembers.Add(fullName);
                    }
                    else
                    {
                        var method = (mi as MethodInfo);
                        if (!method.IsPublic || !method.IsStatic)
                            if (!invalidMembers.Contains(fullName))
                                invalidMembers.Add(fullName);
                    }
                }
                if (invalidMembers.Any())
                {
                    Console.Write("Warning: some of the type members are marked with [CustomAction] attribute but they don't meet the MakeSfxCA criteria of being public static method of a public type:\n");
                    foreach (var member in invalidMembers)
                        Console.WriteLine("  " + member);
                    Console.WriteLine();
                }
            }
            catch { }
        }
    }
}

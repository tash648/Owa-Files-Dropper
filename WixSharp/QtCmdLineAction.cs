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

using IO = System.IO;

namespace WixSharp
{
    /// <summary>
    /// Defines WiX <c>QtExecCmdLineAction</c> CustomAction. 
    /// <para><see cref="QtCmdLineAction"/> executes specified application with optional arguments.
    /// You do not have to specify full path to the application to be executed as long as its directory
    /// is well-known (e.g. listed in system environment variable <c>PATH</c>) on the target system.</para>
    /// <remarks>
    /// <see cref="QtCmdLineAction"/> often needs to be executed with the elevated privileges. Thus after instantiation it will have 
    /// <see cref="Action.Impersonate"/> set to <c>false</c> and <see cref="Action.Execute"/> set to <c>Execute.deferred</c> to allow elevating.
    /// </remarks>
    /// </summary>
    /// 
    /// <example>The following is a complete setup script defining <see cref="QtCmdLineAction"/> for
    /// opening <c>bool.ini</c> file in <c>Notepad.exe</c>:
    /// <code>
    ///static public void Main(string[] args)
    ///{
    ///    var project = new Project()
    ///    {
    ///        UI = WUI.WixUI_ProgressOnly,
    ///        Name = "CustomActionTest",
    ///        Actions = new[] { new ("notepad.exe", @"C:\boot.ini") },
    ///     };
    ///     
    ///     Compiler.BuildMsi(project);
    /// }
    /// </code>
    /// </example>
    public partial class QtCmdLineAction : Action
    {
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        public QtCmdLineAction(string appPath, string args)
            : base()
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
            Return = Return.check;
        }
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="QtCmdLineAction"/> instance.</param>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        public QtCmdLineAction(Id id, string appPath, string args)
            : base(id)
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
            Return = Return.check;
        }
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="QtCmdLineAction"/>.</param>
        public QtCmdLineAction(string appPath, string args, Return returnType, When when, Step step, Condition condition)
            : base(returnType, when, step, condition)
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
        }
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="QtCmdLineAction"/> instance.</param>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="QtCmdLineAction"/>.</param>
        public QtCmdLineAction(Id id, string appPath, string args, Return returnType, When when, Step step, Condition condition)
            : base(id, returnType, when, step, condition)
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
        }
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="QtCmdLineAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public QtCmdLineAction(string appPath, string args, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(returnType, when, step, condition, sequence)
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
        }
        /// <summary>
        /// Executes a new instance of the <see cref="QtCmdLineAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="QtCmdLineAction"/> instance.</param>
        /// <param name="appPath">Path to the application to be executed. This can be a file name only if the location of the application is well-known.</param>
        /// <param name="args">The arguments to be passed to the application during the execution.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="QtCmdLineAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public QtCmdLineAction(Id id, string appPath, string args, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(id, returnType, when, step, condition, sequence)
        {
            AppPath = appPath;
            Args = args;
            Name = "Action" + (++count) + "_QtCmdLine_" + IO.Path.GetFileName(appPath);
        }

        /// <summary>
        /// Path to the application to be executed. This can be a file name only if the location of the application is well-known.
        /// </summary>
        public string AppPath = "";
        /// <summary>
        /// The arguments to be passed to the application during the execution.
        /// </summary>
        public string Args = "";
    }
}

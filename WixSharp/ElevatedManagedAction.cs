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
using System.Linq;
using System.Text;
namespace WixSharp
{
    /// <summary>
    /// Defines WiX Managed CustomAction, which is to be run with elevated privileges (UAC). 
    /// <para>
    /// Any CustomAction, which needs elevation must be run with  <see cref="Action.Impersonate"/> set to 
    /// <c>false</c> and <see cref="Action.Execute"/> set to <c>Execute.deferred</c>. Thus <see cref="ElevatedManagedAction"/> is 
    /// a full equivalent of <see cref="T:WixSharp.ManagedAction"/> with appropriately adjusted <see cref="Action.Execute"/> and 
    /// <see cref="Action.Impersonate"/> during the instantiation:
    /// </para>
    /// <code>
    ///  public ElevatedManagedAction() : base()
    ///  {
    ///      Impersonate = false;
    ///      Execute = Execute.deferred;
    ///  }
    /// </code>
    /// </summary>summary>
    public partial class ElevatedManagedAction : ManagedAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class.
        /// </summary>
        public ElevatedManagedAction()
            : base()
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties="INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        public ElevatedManagedAction(string name)
            : base(name)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        public ElevatedManagedAction(Id id, string name)
            : base(id, name)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        public ElevatedManagedAction(string name, string actionAssembly)
            : base(name, actionAssembly)
        {
            Impersonate = false;
            Execute = Execute.deferred;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        public ElevatedManagedAction(Id id, string name, string actionAssembly)
            : base(id, name, actionAssembly)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        public ElevatedManagedAction(string name, Return returnType, When when, Step step, Condition condition)
            : base(name, returnType, when, step, condition)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        public ElevatedManagedAction(string name, string actionAssembly, Return returnType, When when, Step step, Condition condition)
            : base(name, actionAssembly, returnType, when, step, condition)
        {
            Impersonate = false;
            Execute = Execute.deferred;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        public ElevatedManagedAction(Id id, string name, Return returnType, When when, Step step, Condition condition)
            : base(id, name, returnType, when, step, condition)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        public ElevatedManagedAction(Id id, string name, string actionAssembly, Return returnType, When when, Step step, Condition condition)
            : base(id, name, actionAssembly, returnType, when, step, condition)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public ElevatedManagedAction(string name, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(name, returnType, when, step, condition, sequence)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public ElevatedManagedAction(string name, string actionAssembly, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(name, actionAssembly, returnType, when, step, condition, sequence)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public ElevatedManagedAction(Id id, string name, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(id, name, returnType, when, step, condition, sequence)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatedManagedAction"/> class with properties/fields initialized with specified parameters.
        /// </summary>
        /// <param name="id">The explicit <see cref="Id"></see> to be associated with <see cref="ElevatedManagedAction"/> instance.</param>
        /// <param name="name">Name of the CustomAction. The name should match the method implementing the custom action functionality.</param>
        /// <param name="actionAssembly">Path to the assembly containing the CustomAction implementation. Specify <c>"%this%"</c> if the assembly 
        /// is in the Wix# script.</param>
        /// <param name="returnType">The return type of the action.</param>
        /// <param name="when"><see cref="T:WixSharp.When"/> the action should be executed with respect to the <paramref name="step"/> parameter.</param>
        /// <param name="step"><see cref="T:WixSharp.Step"/> the action should be executed before/after during the installation.</param>
        /// <param name="condition">The launch condition for the <see cref="ManagedAction"/>.</param>
        /// <param name="sequence">The MSI sequence the action belongs to.</param>
        public ElevatedManagedAction(Id id, string name, string actionAssembly, Return returnType, When when, Step step, Condition condition, Sequence sequence)
            : base(id, name, actionAssembly, returnType, when, step, condition, sequence)
        {
            Impersonate = false;
            Execute = Execute.deferred;
            UsesProperties = "INSTALLDIR";
        }
    }
}

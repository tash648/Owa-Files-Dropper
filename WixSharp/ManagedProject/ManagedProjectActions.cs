﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Deployment.WindowsInstaller;

namespace WixSharp
{
    /// <summary>
    /// 
    /// </summary>
    public class ManagedProjectActions
    {
        /// <summary>
        /// Internal ManagedProject action. It must be public for the DTF accessibility but it is not to be used by the user/developer. 
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult WixSharp_InitRuntime_Action(Session session)
        {
            //Debugger.Launch();
            return ManagedProject.Init(session);
        }

        /// <summary>
        /// Internal ManagedProject action. It must be public for the DTF accessibility but it is not to be used by the user/developer. 
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult WixSharp_Load_Action(Session session)
        {
            //Debugger.Launch();
            return ManagedProject.InvokeClientHandlers(session, "Load");
        }

        /// <summary>
        /// Internal ManagedProject action. It must be public for the DTF accessibility but it is not to be used by the user/developer. 
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult WixSharp_BeforeInstall_Action(Session session)
        {
            return ManagedProject.InvokeClientHandlers(session, "BeforeInstall");
        }

        /// <summary>
        /// Internal ManagedProject action. It must be public for the DTF accessibility but it is not to be used by the user/developer. 
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult WixSharp_AfterInstall_Action(Session session)
        {
            return ManagedProject.InvokeClientHandlers(session, "AfterInstall");
        }
    }


    internal static class Reflect
    {
        public static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;

                    string supername = null;
                    if (memberExpression.Expression != null)
                        supername = GetMemberName(memberExpression.Expression);

                    if (String.IsNullOrEmpty(supername))
                        return memberExpression.Member.Name;

                    return String.Concat(supername, '.', memberExpression.Member.Name);

                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    return callExpression.Method.Name;

                case ExpressionType.Convert:
                    var unaryExpression = (UnaryExpression)expression;
                    return GetMemberName(unaryExpression.Operand);

                case ExpressionType.Constant:
                case ExpressionType.Parameter:
                    return String.Empty;

                default:
                    throw new ArgumentException("The expression is not a member access or method call expression");
            }
        }

        public static string NameOf<T>(Expression<Func<T>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string StringOf(Expression<System.Action> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string PropertyName<T>(Expression<Func<T>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)expression.Body).Member.Name;
            throw new ArgumentException("The expression is not a property access expression");
        }

        public static string of<T>()
        {
            return typeof(T).Name;
        }
    }
}

#region Apache-v2.0

//    Copyright 2017 Will Hopkins - Moonrise Media Ltd.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Moonrise.Utils.Test
{
    /// <summary>
    ///     Allows access to private methods of a "wrapped object"
    /// </summary>
    /// Usage:
    /// dynamic wrapper = PrivateWrapper.Create{ClassWithPrivateMethods}(constructor args)
    /// wrapper.PrivateMethod();
    /// <para>
    ///     based on but tweaked from http://www.amazedsaint.com/2010/05/accessprivatewrapper-c-40-dynamic.html
    /// </para>
    /// You will also need to add a reference to System.ServiceModel!
    public class PrivateWrapper : DynamicObject
    {
        /// <summary>
        ///     Specify the flags for accessing members
        /// </summary>
        private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance
                                                     | BindingFlags.Static | BindingFlags.Public;

        /// <summary>
        ///     The object we are going to wrap
        /// </summary>
        private readonly object _wrapped;

        /// <summary>
        ///     Create a simple private wrapper
        /// </summary>
        /// <param name="o">The object to be wrapped</param>
        public PrivateWrapper(object o)
        {
            _wrapped = o;
        }

        /// <summary>
        ///     Creates a wrapped object whose private methods and properties can be accessed.
        /// </summary>
        /// <typeparam name="T">Type to wrap</typeparam>
        /// <param name="args">Any parameters to pass to the constructor</param>
        /// <returns>The wrapped object</returns>
        public static dynamic Create<T>(params object[] args)
        {
            Type type = typeof(T);
            string typeName = type.Name;
            return FromType(type.GetTypeInfo().Assembly, typeName, args);
        }

        /// <summary>
        ///     Creates a dynamic object from the named type in an identified assembly
        /// </summary>
        /// <param name="asm">The assembly containing the type</param>
        /// <param name="type">The name of the type</param>
        /// <param name="args">Any parameters to pass to its constructor</param>
        /// <returns>The created dynamic object</returns>
        public static dynamic FromType(Assembly asm, string type, params object[] args)
        {
            Type[] allt = asm.GetTypes();
            Type t = allt.First(item => item.Name == type);

            IEnumerable<Type> types = from a in args
                                      select a.GetType();

            // Get the constructor matching the specified set of args
//#if DotNetCore
            ConstructorInfo ctor = t.GetConstructor(types.ToArray());

//#else
//            ConstructorInfo ctor = t.GetConstructor(Flags, null, types.ToArray(), null);
//#endif

            if (ctor != null)
            {
                object instance = ctor.Invoke(args);
                return new PrivateWrapper(instance);
            }

            return null;
        }

        /// <summary>
        ///     Tries to get a property or field with the given name
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member on which the dynamic operation is performed. For example, for the
        ///     Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived
        ///     from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The
        ///     binder.IgnoreCase property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="result">
        ///     The result of the get operation. For example, if the method is called for a property, you can
        ///     assign the property value to <paramref name="result" />.
        /// </param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behaviour. (In most cases, a run-time exception is thrown.)
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            bool retVal = false;

            // Try getting a property of that name
            PropertyInfo prop = _wrapped.GetType().GetProperty(binder.Name, Flags);

            try
            {
                if (prop == null)
                {
                    // Try getting a field of that name
                    FieldInfo fld = _wrapped.GetType().GetField(binder.Name, Flags);

                    if (fld != null)
                    {
                        result = fld.GetValue(_wrapped);
                        retVal = true;
                    }
                    else
                    {
                        retVal = base.TryGetMember(binder, out result);
                    }
                }
                else
                {
                    result = prop.GetValue(_wrapped, null);
                    retVal = true;
                }
            }
            catch (Exception excep)
            {
                // We propogate any generated exceptions from the invoked method!
                if (excep.InnerException != null)
                {
                    throw excep.InnerException;
                }

                throw;
            }

            return retVal;
        }

        /// <summary>
        ///     Try invoking a method
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the dynamic operation. The binder.Name property provides the name of
        ///     the member on which the dynamic operation is performed. For example, for the statement
        ///     sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleMethod". The binder.IgnoreCase
        ///     property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="args">
        ///     The arguments that are passed to the object member during the invoke operation. For example, for the
        ///     statement sampleObject.SampleMethod(100), where sampleObject is derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, args[0] is equal to 100.
        /// </param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behaviour. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            bool retVal = false;

            IEnumerable<Type> types = from a in args
                                      select a.GetType();

//#if DotNetCore
            MethodInfo method = _wrapped.GetType().GetTypeInfo().GetMethod(binder.Name, types.ToArray());

//#else
//            MethodInfo method = _wrapped.GetType().GetTypeInfo().GetMethod(binder.Name, Flags, null, types.ToArray(), null);
//#endif

            try
            {
                if (method == null)
                {
                    retVal = base.TryInvokeMember(binder, args, out result);
                }
                else
                {
                    result = method.Invoke(_wrapped, args);
                    retVal = true;
                }
            }
            catch (Exception excep)
            {
                // We propogate any generated exceptions from the invoked method!
                if (excep.InnerException != null)
                {
                    throw excep.InnerException;
                }

                throw;
            }

            return retVal;
        }

        /// <summary>
        ///     Tries to set a property or field with the given name
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member to which the value is being assigned. For example, for the statement
        ///     sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase
        ///     property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="value">
        ///     The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where
        ///     sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the
        ///     <paramref name="value" /> is "Test".
        /// </param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behaviour. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            bool retVal = false;

            PropertyInfo prop = _wrapped.GetType().GetProperty(binder.Name, Flags);

            try
            {
                if (prop == null)
                {
                    FieldInfo fld = _wrapped.GetType().GetField(binder.Name, Flags);

                    if (fld != null)
                    {
                        fld.SetValue(_wrapped, value);
                        retVal = true;
                    }
                    else
                    {
                        retVal = base.TrySetMember(binder, value);
                    }
                }
                else
                {
                    prop.SetValue(_wrapped, value, null);
                    retVal = true;
                }
            }
            catch (Exception excep)
            {
                // We propogate any generated exceptions from the invoked method!
                if (excep.InnerException != null)
                {
                    throw excep.InnerException;
                }

                throw;
            }

            return retVal;
        }
    }
}

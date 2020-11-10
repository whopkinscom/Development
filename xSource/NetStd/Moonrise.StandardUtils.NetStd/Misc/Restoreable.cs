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
using System.Linq.Expressions;
using System.Reflection;
using Force.DeepCloner;

namespace Moonrise.Utils.Standard.Misc
{
    /// <summary>
    ///     Class to define the extension method in.
    /// </summary>
    public static class RestorableExtension
    {
        /// <summary>
        ///     Grabs a property in it's constructor and resets it to the value it had when passed as it disposes.
        /// </summary>
        /// <typeparam name="T">Here to allow for the Func&lt;T&gt;</typeparam>
        /// <seealso cref="System.IDisposable" />
        private class RestoreableValue<T> : IDisposable
        {
            /// <summary>
            ///     The instance whose value will be reset
            /// </summary>
            private readonly object instance;

            /// <summary>
            ///     The original VALUE of the property
            /// </summary>
            private readonly object originalValue;

            /// <summary>
            ///     The property information for the property that needs to be restoreable
            /// </summary>
            private readonly PropertyInfo propInfo;

            /// <summary>
            ///     Initializes a new instance of the <see cref="RestoreableValue{T}" /> class.
            ///     <para>
            ///         Note: If you use this class YOU NEED TO NUGET "DeepCloner by force"!
            ///     </para>
            /// </summary>
            /// <param name="_instance">The _instance whose property is being passed.</param>
            /// <param name="property">The property to be restored on <see cref="Dispose" />.</param>
            /// <param name="deepCopy">
            ///     Indicates if a deep copy is performed on the property - you usually will want this - though
            ///     there is only an effect on properties that are an object
            /// </param>
            /// <exception cref="ArgumentException">The property HAS to belong to the type</exception>
            public RestoreableValue(object _instance, Expression<Func<T>> property, bool deepCopy = true)
            {
                propInfo = ((MemberExpression)property.Body).Member as PropertyInfo;

                if (propInfo.DeclaringType != _instance.GetType())
                {
                    throw new ArgumentException(string.Format("Property ({0}.{1}) must be a property of the instance ({2})!",
                                                              propInfo.DeclaringType,
                                                              propInfo.Name,
                                                              _instance.GetType()));
                }

                instance = _instance;

                // We get the current actual value by compiling the lambda expression
                originalValue = property.Compile()();

                if (deepCopy)
                {
                    // We want originalValue to be THE SAME INSTANCE, so we set the property to be the cloned value
                    SetOriginalProperty(originalValue.DeepClone());
                }
            }

            /// <summary>
            ///     Restores the instance's property value back to what it was
            /// </summary>
            public void Dispose()
            {
                SetOriginalProperty(originalValue);
            }

            /// <summary>
            ///     Sets the original property.
            /// </summary>
            /// <param name="value">The value.</param>
            private void SetOriginalProperty(object value)
            {
                MethodInfo info = propInfo.GetSetMethod();
                info.Invoke(instance,
                            new[]
                            {
                                value
                            });
            }
        }

        /// <summary>
        ///     Restores a property to its original value when the using scope is exited.
        ///     <para>
        ///         Usage:
        ///     </para>
        ///     <para>
        ///         using (instance.Restorable(()=>instance.Property))
        ///     </para>
        ///     <para>
        ///         Note: If you use this extension method then YOU NEED TO NUGET "GeorgeCloney"!
        ///     </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ext">The ext.</param>
        /// <param name="property">The property.</param>
        /// <param name="deepCopy">
        ///     Indicates if a deep copy is performed on the property - you usually will want this - though
        ///     there is only an effect on properties that are an object
        /// </param>
        /// <returns></returns>
        public static IDisposable Restoreable<T>(this object ext, Expression<Func<T>> property, bool deepCopy = true)
        {
            // The extension method is simply syntactic sugar
            return new RestoreableValue<T>(ext, property, deepCopy);
        }
    }
}

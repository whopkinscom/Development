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
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Moonrise.Utils.Standard.Extensions
{
    /// <summary>
    ///     Extension methods that apply to classes
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        ///     Gets the name from any applied <see cref="DisplayAttribute" />.
        /// </summary>
        /// <typeparam name="TClass">The class type being extended - typically a model class</typeparam>
        /// <typeparam name="TProperty">The property type whose display name we want</typeparam>
        /// <param name="model">The class type being extended - typically a model class</param>
        /// <param name="propertyExpression">The property type whose display name we want</param>
        /// <returns>The display name or null</returns>
        public static string DisplayName<TClass, TProperty>(
            this TClass model,
            Expression<Func<TClass, TProperty>> propertyExpression)
            where TClass : class
        {
            Type type = typeof(TClass);
            MemberExpression memberExpression = (MemberExpression)propertyExpression.Body;
            string propertyName = memberExpression.Member is PropertyInfo ? memberExpression.Member.Name : null;

            DisplayAttribute attr = type.GetProperty(propertyName)?.GetCustomAttribute<DisplayAttribute>();

            return attr?.Name;
        }

        /// <summary>
        ///     Gives the caller's fully qualified method name.
        ///     <para>
        ///         Usage: this.FQMethodName()
        ///     </para>
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="caller">The caller.</param>
        /// <returns>The fully qualifed method name</returns>
        public static string FQMethodName(this object instance, [CallerMemberName] string caller = null) => $"{instance.GetType().FullName}.{caller}";

        /// <summary>
        ///     Gives the caller's class qualified method name.
        ///     <para>
        ///         Usage: this.MethodName()
        ///     </para>
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="caller">The caller.</param>
        /// <returns>The class qualifed method name</returns>
        public static string MethodName(this object instance, [CallerMemberName] string caller = null) => $"{instance.GetType().Name}.{caller}";
    }
}
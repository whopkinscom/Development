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
using System.Reflection;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Checks that whatever it is applied to is NOT the default value. If you apply this to ints etc, then you are stating
    ///     they are NOT to be zero!
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    public class NonDefaultAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NonDefaultAttribute" /> class.
        /// </summary>
        public NonDefaultAttribute() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NonDefaultAttribute" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public NonDefaultAttribute(string errorMessage) : base(errorMessage) { }

        /// <summary>
        ///     Determines if a given object is equal to its default value
        /// </summary>
        /// <param name="value">the object concerned</param>
        /// <returns>True if it has the default value</returns>
        public static bool IsDefault(object value)
        {
            if (value != null)
            {
                if (value.GetType().GetTypeInfo().IsValueType)
                {
                    // The only way to determine the default value, of a value type, when you only have an object is to
                    // create a new instance - which will get the default value. You can't use the "default" keyword without
                    // explicitly knowing the type!
                    object defaultValue = Activator.CreateInstance(value.GetType());

                    if (Equals(value, defaultValue))
                    {
                        // It's a value type that is equal to its default
                        return true;
                    }

                    // It's a value type that isn't equal to its default
                    return false;
                }

                // It's not a value type and it isn't null
                return false;
            }

            // It's null
            return true;
        }

        /// <summary>
        ///     Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The context of the validation.</param>
        /// <returns>true or false!</returns>
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (IsDefault(value))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}

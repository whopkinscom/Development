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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Validation attribute to indicate that nulls should not be allowed to be stored in an IEnumerable
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property |
                    AttributeTargets.Field)]
    public class ListContentValidationAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ListContentValidationAttribute" /> class.
        /// </summary>
        /// <param name="errorMessageAccessor">The function that enables access to validation resources.</param>
        public ListContentValidationAttribute(Func<string> errorMessageAccessor) : base(errorMessageAccessor) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ListContentValidationAttribute" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message to associate with a validation control.</param>
        public ListContentValidationAttribute(string errorMessage) : base(errorMessage) { }

        /// <summary>
        ///     Determines if null elements are allowed in the "list"
        /// </summary>
        public bool AllowNulls { get; set; }

        /// <summary>
        ///     Indicates whether further details about the invalidity are appended. e.g. needs X elements, following [x],[y] are
        ///     null, etc. Defaults to false.
        /// </summary>
        public bool AppendFurtherErrorDetails { get; set; }

        /// <summary>
        ///     The maximum number of elements expected
        /// </summary>
        public int MaxElements { get; set; } = int.MinValue;

        /// <summary>
        ///     The minimum number of elements expected
        /// </summary>
        public int MinElements { get; set; } = int.MaxValue;

        /// <summary>Validates the specified value with respect to the current validation attribute.</summary>
        /// <returns>An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class. </returns>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult retVal = ValidationResult.Success;

            // As far as THIS attribute is concerned a null "value" is OK as it can't be checked any further!
            if (value == null)
            {
                return retVal;
            }

            // This is only relevant to IEnumerables
            if (!(value is IEnumerable))
            {
                return base.IsValid(value, validationContext);
            }

            int i = 0;
            bool nullsPresent = false;
            List<string> nullMembers = new List<string>();

            foreach (object o in (IEnumerable)value)
            {
                if (o == null)
                {
                    nullsPresent = true;
                    nullMembers.Add($"{validationContext.MemberName ?? validationContext.DisplayName}[{i}]");
                }

                i++;
            }

            string errorMessage = string.Empty;

            if ((MinElements != int.MaxValue) && (i < MinElements))
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = ErrorMessageString;
                }

                if (AppendFurtherErrorDetails)
                {
                    errorMessage += "\r\n\t";
                    errorMessage += string.Format("{0} had less than the required minimum of {1} elements.",
                                                  validationContext.DisplayName,
                                                  MinElements);
                }
            }

            if ((MaxElements != int.MinValue) && (i > MaxElements))
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = ErrorMessageString;
                }

                if (AppendFurtherErrorDetails)
                {
                    errorMessage += "\r\n\t";
                    errorMessage += string.Format("{0} had more than the required maximum of {1} elements.",
                                                  validationContext.DisplayName,
                                                  MaxElements);
                }
            }

            if (!AllowNulls && nullsPresent)
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = ErrorMessageString;
                }

                if (AppendFurtherErrorDetails)
                {
                    errorMessage += "\r\n\t";
                    errorMessage += string.Format("{0}: The following elements were null;",
                                                  validationContext.MemberName ?? validationContext.DisplayName);
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                if (nullMembers.Count > 0)
                {
                    // Make sure the member itself being validated (not just its contents) is mentioned in dispatches!
                    nullMembers.Insert(0, validationContext.MemberName ?? validationContext.DisplayName);
                    retVal = new ValidationResult(errorMessage, nullMembers);
                }
                else
                {
                    retVal = new ValidationResult(errorMessage,
                                                  new List<string>
                                                  {
                                                      validationContext.MemberName ?? validationContext.DisplayName
                                                  });
                }
            }

            return retVal;
        }
    }
}

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
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Used on a "composite property", i.e. a class, it indicates that if ANY validation on the child members fails, then
    ///     this "parent"
    ///     validation will also fail.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateChildAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatedByParentAttribute" /> class.
        /// </summary>
        /// <param name="message">The additional message to display if ANY of the class members have validation that fails.</param>
        public ValidateChildAttribute(string message) : base(message) { }

        /// <summary>
        ///     Indicates if the message from the child's <see cref="ValidatedByParentAttribute" /> should be appended to the end
        ///     of this attribute's
        ///     <see cref="ValidationAttribute.ErrorMessage" />. Defaults to true.
        /// </summary>
        public bool AppendChildMessage { get; set; } = true;

        /// <summary>
        ///     The name of the member/field to highlight on invalidity.
        /// </summary>
        public string MemberToHighlight { get; set; }

        /// <summary>
        ///     Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult retVal = ValidationResult.Success;

            // Just check this attribute hasn't been assigned to the wrong type of property type.
            if ((value == null) || value.GetType().GetTypeInfo().IsPrimitive)
            {
                return retVal;
            }

            // Look for all validation attributes on all properties of this object
            Type valueType = value.GetType();

            foreach (PropertyInfo propertyInfo in valueType.GetProperties())
            {
                foreach (ValidationAttribute validationAttribute in propertyInfo.GetCustomAttributes<ValidationAttribute>(true))
                {
                    object propertyValue = propertyInfo.GetValue(value);
                    ValidationResult attributeValidationResult =
                        validationAttribute.GetValidationResult(propertyValue, validationContext);

                    if (attributeValidationResult != null)
                    {
                        retVal = CreateValidationResult(propertyInfo);
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Creates the validation result.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The created validation result!</returns>
        private ValidationResult CreateValidationResult(PropertyInfo propertyInfo)
        {
            // The validation for at least one of the validation attributes on one of the properties failed.
            // This is all that THIS validator needs to know in order to fail.
            string memberToHighlight = MemberToHighlight;

            // Now we need to work out the member to report as having failed - as this can affect what if anything gets highlighted by any classes that do field highlighting.
            ValidatedByParentAttribute validatedByParentAttribute =
                propertyInfo.GetCustomAttribute<ValidatedByParentAttribute>();

            string childMessage = string.Empty;

            if (validatedByParentAttribute != null)
            {
                childMessage = validatedByParentAttribute.ErrorMessage;

                if (!string.IsNullOrEmpty(validatedByParentAttribute.MemberToHighlight))
                {
                    memberToHighlight = validatedByParentAttribute.MemberToHighlight;
                }
            }

            if (string.IsNullOrEmpty(memberToHighlight))
            {
                memberToHighlight = propertyInfo.Name;
            }

            ValidationResult retVal = new ValidationResult(
                $"{ErrorMessageString}{(AppendChildMessage && !string.IsNullOrWhiteSpace(childMessage) ? $" - {childMessage}" : string.Empty)}",
                new List<string>
                {
                    memberToHighlight
                });

            return retVal;
        }
    }
}

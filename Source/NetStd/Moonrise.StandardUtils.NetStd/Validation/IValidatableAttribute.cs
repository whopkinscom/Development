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
using System.Linq;
using System.Reflection;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Causes the containing class' <see cref="IValidatableObject.Validate" /> to be invoked, allowing for later reported
    ///     errors (in MVC) to be reported earlier! THIS ONLY WORKS IF THE ELEMENT ATTRIBUTED CAN RETURN ONE ERROR, i.e. is
    ///     unlikely to be a Composite!
    ///     <para>
    ///         Note: Any validation errors must return both the member name AND the display name. e.g.
    ///     </para>
    ///     <para>
    ///         yield return new <see cref="ValidationResult" />("Message xxxx",
    ///     </para>
    ///     <para>
    ///         new[]{ nameof(XxxProperty),
    ///     </para>
    ///     <para>
    ///         ((XxxViewModel)validationContext.ObjectInstance).DisplayName(m=>m.XxxProperty)}
    ///     </para>
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class IValidatableAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IValidatableAttribute" /> class.
        /// </summary>
        public IValidatableAttribute() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IValidatableAttribute" /> class.
        ///     <para>
        ///         The message will only be displayed IF the failed validation in <see cref="IValidatableObject.Validate" />
        ///         returns a null or empty string in ITS message. Sometimes it will make sense to specify the message within the
        ///         attribute but other times it may make more sense to specify the message where the check is being done. Your
        ///         choice!
        ///     </para>
        /// </summary>
        /// <param name="message">
        ///     The message to display IF the failed validation in <see cref="IValidatableObject.Validate" />
        ///     returns a null or empty string in IT'S message.
        /// </param>
        public IValidatableAttribute(string message)
            : base(message) { }

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

            if (validationContext.ObjectType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IValidatableObject)))
            {
                retVal = ((IValidatableObject)validationContext.ObjectInstance)
                         .Validate(validationContext)
                         .FirstOrDefault(vr => vr.MemberNames.Contains(validationContext.MemberName ?? validationContext.DisplayName));

                if (retVal != null && string.IsNullOrEmpty(retVal.ErrorMessage))
                {
                    retVal.ErrorMessage = ErrorMessageString;
                }
            }

            return retVal;
        }
    }
}
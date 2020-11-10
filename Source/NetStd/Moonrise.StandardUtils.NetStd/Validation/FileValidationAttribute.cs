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
using System.IO;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Validates a filepath string
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property |
                    AttributeTargets.Field)]
    public class FileValidationAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Backing store for <see cref="Exists" />
        /// </summary>
        private bool _exists;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileValidationAttribute" /> class.
        /// </summary>
        public FileValidationAttribute()
        {
            Exists = true;
        }

        /// <summary>
        ///     Determines if the file exists
        /// </summary>
        public bool Exists
        {
            get
            {
                return _exists;
            }
            set
            {
                _exists = value;
                ErrorMessage = _exists ? "The file must exist" : "The file must not already exist";
            }
        }

        /// <summary>
        ///     Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <returns>An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class. </returns>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (!(value is string))
            {
                throw new ValidationException("The FileValidationAttribute is only valid for use on strings!");
            }

            ValidationResult retVal = ValidationResult.Success;

            string filepath = (string)value;

            if (File.Exists(filepath) != Exists)
            {
                retVal = new ValidationResult(ErrorMessage);
            }

            return retVal;
        }
    }
}

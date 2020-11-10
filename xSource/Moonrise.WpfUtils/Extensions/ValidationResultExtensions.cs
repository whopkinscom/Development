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
using System.Collections.Generic;
using System.Windows.Controls;

namespace Moonrise.Utils.Wpf.Extensions
{
    /// <summary>
    /// Extensions for validation results
    /// </summary>
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Converts a list of validation results into a list of, well validation results!
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static List<ValidationResult> ConvertToControlValidationResults(
            this List<System.ComponentModel.DataAnnotations.ValidationResult> original)
        {
            List<ValidationResult> newOne = new List<ValidationResult>(original.Count);

            foreach (System.ComponentModel.DataAnnotations.ValidationResult result in original)
            {
                newOne.Add(new ValidationResult(false, result.ErrorMessage));
            }

            return newOne;
        }
    }
}

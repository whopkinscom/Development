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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Moonrise.Utils.Standard.Validation;
using Moonrise.Utils.Wpf.Extensions;

namespace Moonrise.Utils.Wpf.Validation
{
    /// <summary>
    /// Allows validation to be done against interface validation rules
    /// </summary>
    public class InterfaceValidationRule : ValidationRule
    {
        /// <summary>
        /// Constructs an InterfaceValidationRule
        /// </summary>
        public InterfaceValidationRule()
        {
            // This way we can access the DataContext in the Validate method.
            ValidationStep = ValidationStep.UpdatedValue;

            // This enforces validation at startup.
            ValidatesOnTargetUpdated = true;
        }

        /// <summary>
        /// Validates a binding expression
        /// </summary>
        /// <param name="value">The binding expression to validate</param>
        /// <param name="cultureInfo">The cultureInfo to apply</param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is BindingExpression))
            {
                return ValidationResult.ValidResult;
            }

            BindingExpression bindingExpression = (BindingExpression)value;

            string property = bindingExpression.ResolvedSourcePropertyName;
            object dataContext = bindingExpression.ResolvedSource;

            // OK, need to validate the named property against it's interface or own attribute against the data item.
            List<System.ComponentModel.DataAnnotations.ValidationResult> results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            InterfaceValidator.ValidateProperty(dataContext, property, results);
            List<ValidationResult> wpfValidationResults = results.ConvertToControlValidationResults();
            SetValidationProperty(dataContext, wpfValidationResults.Count == 0);

            if (wpfValidationResults.Count == 0)
            {
                wpfValidationResults.Add(new ValidationResult(true, null));
            }

            // This will fail if it is valid!
            return wpfValidationResults[0];
        }

        /// <summary>
        /// Finds (all of) the properties of the dataContext that have an attribute indicating that they should receive the result of validating and sets that result value.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="valid">The validity value to set.</param>
        private void SetValidationProperty(object dataContext, bool valid)
        {
            Type instanceType = dataContext != null ? dataContext.GetType() : null;

            if (instanceType != null)
            {
                List<PropertyInfo> props = instanceType.GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(ValidationResultAttribute))).ToList();

                foreach (Type implementedInterface in instanceType.GetTypeInfo().ImplementedInterfaces)
                {
                    List<PropertyInfo> additionalProps = implementedInterface.GetProperties().Where(
                        prop => Attribute.IsDefined(
                            prop,
                            typeof(ValidationResultAttribute))).ToList();

                    foreach (PropertyInfo additionalProp in additionalProps)
                    {
                        props.Add(instanceType.GetProperty(additionalProp.Name));
                    }
                }

                foreach (PropertyInfo prop in props)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        prop.SetValue(dataContext, valid);
                    }
                }

                DependencyProperty fred = System.Windows.Controls.Validation.HasErrorProperty;
            }
        }
    }
}

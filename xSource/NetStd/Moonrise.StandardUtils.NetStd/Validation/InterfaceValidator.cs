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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Enables validation of an instance against an interface
    /// </summary>
    public class InterfaceValidator
    {
        /// <summary>
        ///     Collates the validation messages.
        /// </summary>
        /// <param name="results">The validation results.</param>
        /// <returns>A list of just the message strings</returns>
        public static List<string> CollateValidationMessages(ICollection<ValidationResult> results)
        {
            List<string> retVal = null;

            if (results != null)
            {
                retVal = new List<string>();

                foreach (ValidationResult validationResult in results)
                {
                    retVal.Add(validationResult.ErrorMessage);

                    foreach (string memberName in validationResult.MemberNames)
                    {
                        retVal.Add("   " + memberName);
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Iterates through the interface definition, finding the matching implemented property and validates the instance
        ///     value against any validation attributes of the interface definition
        /// </summary>
        /// <param name="instance">The instance of the object implementing the interface.</param>
        /// <param name="results">The validation results.</param>
        /// <returns>True if valid</returns>
        public static bool Validate(object instance, ICollection<ValidationResult> results)
        {
            bool retVal = Validate(instance, string.Empty, results);
            return retVal;
        }

        /// <summary>
        ///     Iterates through the interface definition, finding the matching implemented property and validates the instance
        ///     value against any validation attributes of the interface definition
        /// </summary>
        /// <typeparam name="I">The interface to validate against</typeparam>
        /// <param name="instance">The instance of the object implementing the interface.</param>
        /// <param name="results">The validation results.</param>
        /// <returns>True if valid</returns>
        public static bool ValidateAgainstInterface<I>(object instance, ICollection<ValidationResult> results)
        {
            Type interfaceType = typeof(I);
            bool retVal = ValidateAgainstInterface(instance, interfaceType, string.Empty, results);
            return retVal;
        }

        /// <summary>
        ///     Validates a property of an instance object against any validation attributes on that property or on the interface
        ///     that the container of that property implements.
        /// </summary>
        /// <param name="instance">The instance object.</param>
        /// <param name="propertyName">Name of the property in the instance object.</param>
        /// <param name="results">The results.</param>
        /// <param name="locationContext">The location context.</param>
        /// <returns>Whether the porperty is valid or not</returns>
        public static bool ValidateProperty(object instance, string propertyName, ICollection<ValidationResult> results, string locationContext = "")
        {
            Type instanceType = instance != null ? instance.GetType() : null;

            bool retVal = true;

            if (instanceType != null)
            {
                // Iterate through the properties (only)
                PropertyInfo prop = instanceType.GetProperty(propertyName);
                retVal &= ValidatePropertyAgainstInterface(prop, instance, locationContext, true, results);
            }

            return retVal;
        }

        /// <summary>
        ///     Collates validation results as a single string.
        /// </summary>
        /// <param name="validationResults">The validation results.</param>
        /// <returns>The said single string!</returns>
        public static string ValidationMessagesAsString(ICollection<ValidationResult> validationResults)
        {
            List<string> results = CollateValidationMessages(validationResults);
            string retVal = string.Join("\r\n", results);
            return retVal;
        }

        /// <summary>
        ///     Validates an instance of an implementation against interface.
        /// </summary>
        /// <returns>True if valid</returns>
        /// <param name="instance">The instance of the object implementing the interface.</param>
        /// <param name="locationContext">The location context.</param>
        /// <param name="results">The validation results.</param>
        /// <returns>True if valid</returns>
        /// <exception cref="System.ArgumentException">The instance MUST implement the interfaceType</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        private static bool Validate(object instance,
                                     string locationContext,
                                     ICollection<ValidationResult> results)
        {
            Type instanceType = instance != null ? instance.GetType() : null;

            bool retVal = true;

            if (instanceType != null)
            {
                // Iterate through the properties (only)
                PropertyInfo[] props = instanceType.GetProperties();

                foreach (PropertyInfo prop in props)
                {
                    retVal &= ValidatePropertyAgainstInterface(prop, instance, locationContext, true, results);
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Validates an instance of an implementation against interface.
        /// </summary>
        /// <returns>True if valid</returns>
        /// <param name="instance">The instance of the object implementing the interface.</param>
        /// <param name="interfaceType">Type of the interface to validate against.</param>
        /// <param name="locationContext">The location context.</param>
        /// <param name="results">The validation results.</param>
        /// <returns>True if valid</returns>
        /// <exception cref="System.ArgumentException">The instance MUST implement the interfaceType</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        private static bool ValidateAgainstInterface(object instance,
                                                     Type interfaceType,
                                                     string locationContext,
                                                     ICollection<ValidationResult> results)
        {
            Type instanceType = instance != null ? instance.GetType() : null;

            bool retVal = true;

            // First check that the instance actually implements the interface
            if ((instanceType != null) && !interfaceType.IsAssignableFrom(instanceType.GetTypeInfo().UnderlyingSystemType))
            {
                throw new ArgumentException(string.Format("{0} does not implement {1}!", instanceType.Name, interfaceType.Name));
            }

            if (instanceType != null)
            {
                // Iterate through the properties (only)
                PropertyInfo[] props = interfaceType.GetProperties();

                foreach (PropertyInfo prop in props)
                {
                    retVal &= ValidatePropertyAgainstInterface(prop, instance, locationContext, false, results);
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Validates the property against interface.
        /// </summary>
        /// <param name="prop">The interface property information.</param>
        /// <param name="instance">The instance of that property in the implementation.</param>
        /// <param name="locationContext">The location context (this is used for reporting where in the overall structure we are).</param>
        /// <param name="checkAllInterfaces">
        ///     Determines if we're checking all of the interfaces on a property - essentially we were
        ///     passed the object's property
        /// </param>
        /// <param name="results">The validation results.</param>
        /// <returns>
        ///     True if valid
        /// </returns>
        /// <exception cref="System.ArgumentException">IEnumerable has to be generic</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        private static bool ValidatePropertyAgainstInterface(PropertyInfo prop,
                                                             object instance,
                                                             string locationContext,
                                                             bool checkAllInterfaces,
                                                             ICollection<ValidationResult> results)
        {
            bool retVal = true;

            // Find the implementation of this interface property
            object instanceValue = null;

            if (instance != null)
            {
                Type instanceType = instance.GetType();
                PropertyInfo instanceProp = instanceType.GetProperty(prop.Name);
                instanceValue = instanceProp.GetValue(instance);
            }

            // Get the validation attribute(s) for this property
            List<ValidationAttribute> attributes = ((ValidationAttribute[])prop.GetCustomAttributes(typeof(ValidationAttribute), true)).ToList();

            // And if we need to check all interfaces on the instance
            if (checkAllInterfaces)
            {
                foreach (Type implementedInterface in prop.DeclaringType.GetTypeInfo().ImplementedInterfaces)
                {
                    PropertyInfo potentialProperty = implementedInterface.GetTypeInfo().GetDeclaredProperty(prop.Name);

                    if (potentialProperty != null)
                    {
                        List<ValidationAttribute> interfaceAttributes =
                            ((ValidationAttribute[])potentialProperty.GetCustomAttributes(typeof(ValidationAttribute), true)).ToList();
                        attributes.AddRange(interfaceAttributes);
                    }
                }
            }

            ValidationContext vc = new ValidationContext(prop)
                                   {
                                       DisplayName = string.IsNullOrWhiteSpace(locationContext) ? prop.Name : locationContext + "." + prop.Name
                                   };

            foreach (ValidationAttribute attribute in attributes)
            {
                ValidationResult result = attribute.GetValidationResult(instanceValue, vc);

                if (result != null)
                {
                    if (results != null)
                    {
                        results.Add(result);
                    }

                    retVal = false;
                }
            }

            if ((instanceValue != null) && (prop.PropertyType.GetTypeInfo().IsInterface ||
                                            (prop.PropertyType.GetTypeInfo().IsClass && !typeof(string).IsAssignableFrom(prop.PropertyType))))
            {
                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    // Enumerables need each of the elements validating
                    int index = 0;

                    foreach (object element in (IEnumerable)instanceValue)
                    {
                        // Here we just check that we have a single generic IEnumerable<T>
                        if (prop.PropertyType.GenericTypeArguments.Length == 0)
                        {
                            throw new ArgumentException(string.Format("{0} must be generic with this version, sorry!", prop.Name));
                        }

                        if (prop.PropertyType.GenericTypeArguments.Length > 1)
                        {
                            throw new ArgumentException(string.Format("{0} can only have a single generic with this version, sorry!", prop.Name));
                        }

                        Type enumerableContentType = prop.PropertyType.GenericTypeArguments[0];

                        retVal &= ValidateAgainstInterface(element, enumerableContentType, string.Format("{0}[{1}]", vc.DisplayName, index), results);
                        index++;
                    }
                }
                else
                {
                    // Interfaces are validated recursively
                    retVal &= ValidateAgainstInterface(instanceValue, prop.PropertyType, vc.DisplayName, results);
                }
            }

            return retVal;
        }
    }
}

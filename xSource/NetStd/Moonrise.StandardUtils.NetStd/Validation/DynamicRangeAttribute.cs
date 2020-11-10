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

namespace Moonrise.Utils.Standard.Validation
{
    /// <summary>
    ///     Allows range limits to be set at runtime, rather than only compile time.
    ///     <remarks>
    ///         This attribute is NOT able to be fully dynamic as there is no passing of any context in. Typically used for
    ///         ranges that are configured ONCE, say in a config file or database, typically at application startup.
    ///     </remarks>
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property |
                    AttributeTargets.Field)]
    public class DynamicRangeAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Default implementation of <see cref="IDynamicValidationValues" />. You will need to create a sub-class that simply
        ///     defines a static property, by default call it Instance, that is initialised in your constructor to itself. Then
        ///     set the appropriate properties to the dynamic values you require.
        ///     <para>
        ///         Ideally use a ThreadLocal implementation of the static property - see the tests on this class for an example.
        ///     </para>
        /// </summary>
        public abstract class DynamicValidationValues : IDynamicValidationValues
        {
            /// <summary>
            /// The maximum byte value
            /// </summary>
            public byte MaxByte { get; set; }

            /// <summary>
            /// The maximum DateTime value
            /// </summary>
            public DateTime MaxDateTime { get; set; }

            /// <summary>
            /// The maximum DateTimeOffset value
            /// </summary>
            public DateTimeOffset MaxDateTimeOffset { get; set; }

            /// <summary>
            /// The maximum double value
            /// </summary>
            public double MaxDouble { get; set; }

            /// <summary>
            /// The maximum int value
            /// </summary>
            public int MaxInt { get; set; }

            /// <summary>
            /// The maximum string value
            /// </summary>
            public string MaxString { get; set; }

            /// <summary>
            /// The minimum byte value
            /// </summary>
            public byte MinByte { get; set; }

            /// <summary>
            /// The minimum DateTime value
            /// </summary>
            public DateTime MinDateTime { get; set; }

            /// <summary>
            /// The minimum DateTimeOffset value
            /// </summary>
            public DateTimeOffset MinDateTimeOffset { get; set; }

            /// <summary>
            /// The minimum double value
            /// </summary>
            public double MinDouble { get; set; }

            /// <summary>
            /// The minimum int value
            /// </summary>
            public int MinInt { get; set; }

            /// <summary>
            /// The minimum string value
            /// </summary>
            public string MinString { get; set; }
        }

        /// <summary>
        ///     Represents the different Min/Max values that can be set to validate against.
        /// </summary>
        public interface IDynamicValidationValues
        {
            /// <summary>
            /// The maximum byte value
            /// </summary>
            byte MaxByte { get; set; }

            /// <summary>
            /// The maximum DateTime value
            /// </summary>
            DateTime MaxDateTime { get; set; }

            /// <summary>
            /// The maximum DateTimeOffset value
            /// </summary>
            DateTimeOffset MaxDateTimeOffset { get; set; }

            /// <summary>
            /// The maximum double value
            /// </summary>
            double MaxDouble { get; set; }

            /// <summary>
            /// The maximum int value
            /// </summary>
            int MaxInt { get; set; }

            /// <summary>
            /// The maximum string value
            /// </summary>
            string MaxString { get; set; }

            /// <summary>
            /// The minimum byte value
            /// </summary>
            byte MinByte { get; set; }

            /// <summary>
            /// The minimum DateTime value
            /// </summary>
            DateTime MinDateTime { get; set; }

            /// <summary>
            /// The minimum DateTimeOffset value
            /// </summary>
            DateTimeOffset MinDateTimeOffset { get; set; }

            /// <summary>
            /// The minimum double value
            /// </summary>
            double MinDouble { get; set; }

            /// <summary>
            /// The minimum int value
            /// </summary>
            int MinInt { get; set; }

            /// <summary>
            /// The minimum string value
            /// </summary>
            string MinString { get; set; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicRangeAttribute" /> class.
        /// </summary>
        /// <param name="validationValuesImplementation">The validation values implementation.</param>
        public DynamicRangeAttribute(Type validationValuesImplementation)
        {
            InstanceName = "Instance";
            ErrorMessage = "Value must be between {0} and {1}";
            DynamicValidationValuesImplementation = validationValuesImplementation;
        }

        //public object ContextSource { get; set; }

        /// <summary>
        ///     The name of the static property on the ValidationValues class that returns the instance the values will be set to.
        ///     Defaults to "Instance".
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        ///     Property "address" representing the maximum byte value
        /// </summary>
        public Expression<Func<byte>> MaxByte { get; set; }

        /// <summary>
        ///     Property "address" representing the maximum int value
        /// </summary>
        public Expression<Func<int>> MaxInt { get; set; }

        /// <summary>
        ///     Property "address" representing the minimum byte value
        /// </summary>
        public Expression<Func<byte>> MinByte { get; set; }

        /// <summary>
        ///     Property "address" representing the minimum int value
        /// </summary>
        public Expression<Func<int>> MinInt { get; set; }

        private Type DynamicValidationValuesImplementation { get; }

        /// <summary>
        ///     Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <returns>An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class. </returns>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult retVal = ValidationResult.Success;
            IDynamicValidationValues supplier = GetValidationSupplier();

            if (value is byte)
            {
                byte byteValue = (byte)value;

                // Invoke the properties
                byte min = supplier.MinByte;
                byte max = supplier.MaxByte;

                if ((byteValue > max) || (byteValue < min))
                {
                    retVal = new ValidationResult(string.Format(ErrorMessage, min, max));
                }
            }
            else if (value is int)
            {
                int intValue = (int)value;

                // Invoke the properties
                int min = supplier.MinInt;
                int max = supplier.MaxInt;

                if ((intValue > max) || (intValue < min))
                {
                    retVal = new ValidationResult(string.Format(ErrorMessage, min, max));
                }
            }
            else if (value is double)
            {
                double doubleValue = (double)value;

                // Invoke the properties
                double min = supplier.MinDouble;
                double max = supplier.MaxDouble;

                if ((doubleValue > max) || (doubleValue < min))
                {
                    retVal = new ValidationResult(string.Format(ErrorMessage, min, max));
                }
            }
            else if (value is string)
            {
                string stringValue = (string)value;

                // Invoke the properties
                string min = supplier.MinString;
                string max = supplier.MaxString;

                if ((stringValue.CompareTo(max) > 0) || (stringValue.CompareTo(min) < 0))
                {
                    retVal = new ValidationResult(string.Format(ErrorMessage, min, max));
                }
            }
            else if (value is DateTime)
            {
                DateTime dateTimeValue = (DateTime)value;

                // Invoke the properties
                DateTime min = supplier.MinDateTime;
                DateTime max = supplier.MaxDateTime;

                if ((dateTimeValue > max) || (dateTimeValue < min))
                {
                    retVal = new ValidationResult(string.Format(ErrorMessage, min, max));
                }
            }
            else
            {
                throw new ValidationException(string.Format("{0} does not currently support range validation for {1}",
                                                            GetType().Name,
                                                            value.GetType().Name));
            }

            return retVal;
        }

        /// <summary>
        ///     Gets the validation supplier which will provide the validation ranges dynamically.
        /// </summary>
        /// <returns>The instance returned by the static method named as the InstanceName</returns>
        /// <exception cref="ValidationException">An exception will be thrown if the type does not contain such a static method</exception>
        private IDynamicValidationValues GetValidationSupplier()
        {
            PropertyInfo propInfo = DynamicValidationValuesImplementation.GetProperty(InstanceName);

            if (!typeof(IDynamicValidationValues).IsAssignableFrom(propInfo.PropertyType))
            {
                throw new ValidationException(string.Format("{0}.{1} needs to return an instance that implements {2}",
                                                            DynamicValidationValuesImplementation.Name,
                                                            InstanceName,
                                                            typeof(IDynamicValidationValues).Name));
            }

            IDynamicValidationValues retVal = propInfo.GetMethod.Invoke(null, null) as IDynamicValidationValues;

            return retVal;
        }
    }
}

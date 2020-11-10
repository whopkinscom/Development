#region Apache-v2.0

//    Copyright 2016 Will Hopkins - Moonrise Media Ltd.
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Validation;

namespace MoonriseStandardUtilsTests.Validation
{
    [TestClass]
    public class DynamicRangeAttributeTests
    {
        public class DateTimeTestData
        {
            [DynamicRange(typeof(ValidationValues), InstanceName = "Instance")]
            public DateTime SomeValue { get; set; }
        }

        public class DoubleTestData
        {
            [DynamicRange(typeof(ValidationValues), InstanceName = "Instance")]
            public double SomeValue { get; set; }
        }

        public class DoubleTestDataWrongInterface
        {
            [DynamicRange(typeof(ValidationValuesWrongInterface))]
            public double SomeValue { get; set; }
        }

        public class IntegerTestData
        {
            [DynamicRange(typeof(ValidationValues))]
            public int SomeValue { get; set; }
        }

        public class StringTestData
        {
            [DynamicRange(typeof(ValidationValues), InstanceName = "Instance")]
            public string SomeValue { get; set; }
        }

        public class ValidationValues : DynamicRangeAttribute.DynamicValidationValues
        {
            private static readonly ThreadLocal<ValidationValues> Instances = new ThreadLocal<ValidationValues>();

            public ValidationValues()
            {
                Instance = this;
            }

            /// <summary>
            ///     Thread-aware implementation to return whatever the current thread's instance is.
            /// </summary>
            public static ValidationValues Instance
            {
                get
                {
                    return Instances.Value;
                }
                set
                {
                    Instances.Value = value;
                }
            }
        }

        public class ValidationValuesWrongInterface
        {
            public ValidationValuesWrongInterface()
            {
                Instance = this;
            }

            public static ValidationValuesWrongInterface Instance { get; set; }

            public double MaxDouble { get; set; }

            public double MinDouble { get; set; }
        }

        [TestMethod]
        public void DynamicDateTimeValidatedInRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinDateTime = DateTime.Now.AddDays(-5),
                                        MaxDateTime = DateTime.Now.AddDays(5)
                                    };

            DateTimeTestData td = new DateTimeTestData
                                  {
                                      SomeValue = DateTime.Now
                                  };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void DynamicDateTimeValidatedOutOfRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinDateTime = DateTime.Now.AddDays(-5),
                                        MaxDateTime = DateTime.Now.AddDays(5)
                                    };

            DateTimeTestData td = new DateTimeTestData
                                  {
                                      SomeValue = DateTime.Now.AddDays(15)
                                  };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(string.Format("Value must be between {0} and {1}", fred.MinDateTime, fred.MaxDateTime), results[0].ErrorMessage);
        }

        [TestMethod]
        public void DynamicDoubleValidatedInRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinDouble = 38.0,
                                        MaxDouble = 96.0
                                    };

            DoubleTestData td = new DoubleTestData
                                {
                                    SomeValue = 48.0
                                };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void DynamicDoubleValidatedOutOfRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinDouble = 38.0,
                                        MaxDouble = 96.0
                                    };

            DoubleTestData td = new DoubleTestData
                                {
                                    SomeValue = 118.0
                                };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Value must be between 38 and 96", results[0].ErrorMessage);
        }

        [TestMethod]
        public void DynamicIntegerValidatedInRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinInt = 38,
                                        MaxInt = 96
                                    };

            IntegerTestData td = new IntegerTestData
                                 {
                                     SomeValue = 48
                                 };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void DynamicIntegerValidatedOutOfRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinInt = 38,
                                        MaxInt = 96
                                    };

            IntegerTestData td = new IntegerTestData
                                 {
                                     SomeValue = 118
                                 };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Value must be between 38 and 96", results[0].ErrorMessage);
        }

        [TestMethod]
        public void DynamicStringValidatedInRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinString = "AAAA",
                                        MaxString = "HHHH"
                                    };

            StringTestData td = new StringTestData
                                {
                                    SomeValue = "CCCC"
                                };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void DynamicStringValidatedOutOfRange()
        {
            ValidationValues fred = new ValidationValues
                                    {
                                        MinString = "AAAA",
                                        MaxString = "HHHH"
                                    };

            StringTestData td = new StringTestData
                                {
                                    SomeValue = "118"
                                };

            List<ValidationResult> results = new List<ValidationResult>();

            InterfaceValidator.Validate(td, results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Value must be between AAAA and HHHH", results[0].ErrorMessage);
        }

        [TestMethod]
        public void ValidationValuesNeedsToImplementCorrectInterface()
        {
            ValidationValuesWrongInterface fred = new ValidationValuesWrongInterface
                                                  {
                                                      MinDouble = 38.0,
                                                      MaxDouble = 96.0
                                                  };

            DoubleTestDataWrongInterface td = new DoubleTestDataWrongInterface
                                              {
                                                  SomeValue = 48.0
                                              };

            List<ValidationResult> results = new List<ValidationResult>();

            try
            {
                InterfaceValidator.Validate(td, results);
                Assert.Fail("Expected a ValidationException to have been thrown!");
            }
            catch (ValidationException excep)
            {
                Assert.IsTrue(excep.Message.Contains("ValidationValuesWrongInterface.Instance"));
            }
        }
    }
}

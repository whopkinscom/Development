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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Logging;
using Moonrise.Utils.Standard.Misc;
using Moonrise.Utils.Test.ObjectCreation;

namespace MoonriseStandardUtilsTests.Misc
{
    [TestClass]
    public class RestoreableValueTests
    {
        public class DeepClass
        {
            public ExampleClass example { get; set; }

            public int IntProp { get; set; }
        }

        public class ExampleClass
        {
            public bool BoolProp { get; set; }

            public int IntegerProp { get; set; }

            public string StringProp { get; set; }
        }

        [TestMethod]
        public void RestoreableValueChecksPropertyBelongsToInstance()
        {
            Creator creator = new Creator(0);
            ExampleClass sut = creator.CreateFilled<ExampleClass>();

            try
            {
                using (creator.Restoreable(() => sut.StringProp))
                {
                    Assert.Fail("No ArgumentException was thrown");
                }
            }
            catch (ArgumentException excep)
            {
                Assert.IsTrue(excep.Message.Contains("must be a property of the instance"));
            }
        }

        [TestMethod]
        public void RestoreableValueDeepCopyDeepCopies()
        {
            Creator creator = new Creator(0);
            DeepClass sut = creator.CreateFilled<DeepClass>();
            Logger.Enabled = true;
            Logger.Debug(sut);

            string strProp = sut.example.StringProp;
            Assert.AreEqual(strProp, sut.example.StringProp);
            string tempStr;

            // We use the default restoreable that DOES do a deep copy
            using (sut.Restoreable(() => sut.example))
            {
                tempStr = creator.GetRandomString();

                // We can change the nested property
                sut.example.StringProp = tempStr;

                Logger.Debug(sut);
                Assert.AreEqual(tempStr, sut.example.StringProp);
            }

            // And the original is restored - because it's actually the original
            Logger.Debug(sut);
            Assert.AreEqual(strProp, sut.example.StringProp);
            Assert.AreNotEqual(tempStr, sut.example.StringProp);
        }

        [TestMethod]
        public void RestoreableValueDeepCopyRestoresSameInstance()
        {
            Creator creator = new Creator(0);
            DeepClass sut = creator.CreateFilled<DeepClass>();

            ExampleClass example = sut.example;
            Assert.IsTrue(ReferenceEquals(example, sut.example));

            using (sut.Restoreable(() => sut.example))
            {
                sut.example = creator.CreateFilled<ExampleClass>();
                Assert.IsFalse(ReferenceEquals(example, sut.example));

                // The original nested is now re-instated
            }

            Assert.IsTrue(ReferenceEquals(example, sut.example));
        }

        [TestMethod]
        public void RestoreableValueIsRestored()
        {
            Creator creator = new Creator(0);
            ExampleClass sut = creator.CreateFilled<ExampleClass>();

            int intValue = sut.IntegerProp;
            bool boolValue = sut.BoolProp;
            string strValue = sut.StringProp;
            Logger.Enabled = true;
            Logger.Debug(sut);

            using (sut.Restoreable(() => sut.BoolProp))
            {
                bool tempBool = !boolValue;
                sut.BoolProp = tempBool;

                Logger.Debug(sut);
                Assert.AreEqual(tempBool, sut.BoolProp);
            }

            Logger.Debug(sut);
            Assert.AreEqual(boolValue, sut.BoolProp);

            using (sut.Restoreable(() => sut.IntegerProp))
            {
                int tempInt = creator.GetRandomInt();
                sut.IntegerProp = tempInt;

                Logger.Debug(sut);
                Assert.AreEqual(tempInt, sut.IntegerProp);
            }

            Logger.Debug(sut);
            Assert.AreEqual(intValue, sut.IntegerProp);

            using (sut.Restoreable(() => sut.StringProp))
            {
                string tempStr = creator.GetRandomString();
                sut.StringProp = tempStr;

                Logger.Debug(sut);
                Assert.AreEqual(tempStr, sut.StringProp);
            }

            Logger.Debug(sut);
            Assert.AreEqual(strValue, sut.StringProp);
        }

        [TestMethod]
        public void RestoreableValueNoDeepCopyDoesntDeepCopy()
        {
            Creator creator = new Creator(0);
            DeepClass sut = creator.CreateFilled<DeepClass>();

            string strProp = sut.example.StringProp;
            Assert.AreEqual(strProp, sut.example.StringProp);
            string tempStr;

            // We don't do a deep copy and so we can change contents of nested
            using (sut.Restoreable(() => sut.example, false))
            {
                tempStr = creator.GetRandomString();

                // we might have saved off a copy of the nested but we haven't changed the sut's nested property
                // and we are now changing one of it's properties
                sut.example.StringProp = tempStr;
                Assert.AreEqual(tempStr, sut.example.StringProp);

                // The original nested is now re-instated
            }

            // But we didn't change the sut's property and so did actually change the value from the original
            Assert.AreNotEqual(strProp, sut.example.StringProp);

            // to the new value.
            Assert.AreEqual(tempStr, sut.example.StringProp);
        }

        [TestMethod]
        public void RestoreableValueNoDeepCopyRestoresInstance()
        {
            Creator creator = new Creator(0);
            DeepClass sut = creator.CreateFilled<DeepClass>();

            string strProp = sut.example.StringProp;
            Assert.AreEqual(strProp, sut.example.StringProp);
            string tempStr;

            // We don't do a deep copy and so we can change contents of nested
            using (sut.Restoreable(() => sut.example, false))
            {
                tempStr = creator.GetRandomString();

                // except we actually change the nested itself (it will have been saved off)
                sut.example = new ExampleClass();

                // And now we are changing properties on the NEW nested 
                sut.example.StringProp = tempStr;

                Assert.AreEqual(tempStr, sut.example.StringProp);

                // The original nested is now re-instated
            }

            // So the original property is still as it was
            Assert.AreEqual(strProp, sut.example.StringProp);

            // And the changed one on the new, temporary nested, is lost, as expected.
            Assert.AreNotEqual(tempStr, sut.example.StringProp);
        }
    }
}

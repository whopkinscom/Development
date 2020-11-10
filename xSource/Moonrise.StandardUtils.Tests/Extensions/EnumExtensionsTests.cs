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
using Moonrise.Utils.Standard.Extensions;
using System.ComponentModel.DataAnnotations;

namespace MoonriseStandardUtilsTests.Extensions
{
    public enum TestE
    {
        [System.ComponentModel.Description("Xero")]
        Zero,

        [System.ComponentModel.Description("Won")]
        [Display(Name = "One", Description = "Whan")]
        Juan,

        [Display(Name = "One", Description = "Whan")]
        Juan1,

        [Display(Name = "One")]
        Juan2,

        WillThisSplit
    }

    [TestClass]
    public class EnumExtensionsTests
    {

        [TestMethod]
        public void FromString()
        {
            TestE testE = EnumExtensions.FromString<TestE>("One");
            Assert.AreEqual(TestE.Juan, testE);
            testE = EnumExtensions.FromString<TestE>("Whan");
            Assert.AreEqual(TestE.Juan, testE);
            testE = EnumExtensions.FromString<TestE>("Juan");
            Assert.AreEqual(TestE.Juan, testE);
            testE = EnumExtensions.FromString<TestE>("Won");
            Assert.AreEqual(TestE.Juan, testE);
            testE = EnumExtensions.FromString<TestE>("1");
            Assert.AreEqual(TestE.Juan, testE);
        }

        [TestMethod]
        public void Description()
        {
            Assert.AreEqual("Xero", TestE.Zero.Description());
            Assert.AreEqual("Won", TestE.Juan.Description());
            Assert.AreEqual("Whan", TestE.Juan1.Description());
            Assert.AreEqual("One", TestE.Juan2.Description());
            Assert.AreEqual("Will This Split", TestE.WillThisSplit.Description());
        }

        [TestMethod]
        public void InvalidEnumValue()
        {
            TestE enumVar = (TestE)999;
            Assert.AreEqual("999", enumVar.Description());
        }
    }
}

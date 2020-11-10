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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Extensions;

namespace MoonriseStandardUtilsTests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [System.ComponentModel.Description("This should work!")]
        public class TestClass { }

        [TestMethod]
        public void DescriptionDescribes()
        {
            TestClass sut = new TestClass();
            Assert.AreEqual("This should work!", sut.Description());
        }

        [TestMethod]
        public void DescriptionDescribesTypes()
        {
            TestClass sut = new TestClass();
            Assert.AreEqual("This should work!", sut.GetType().Description());
            Assert.AreEqual("StringExtensionsTests", typeof(StringExtensionsTests).Description());
        }

        [TestMethod]
        public void IndexOfDoesntFindWithWhitespace()
        {
            string test = "F   ox   y l a d y f o x";
            int pos = test.IndexOf("do x", IndexOfOptions.IgnoreCaseAndWhitespace);
            Assert.AreEqual(-1, pos);
        }

        [TestMethod]
        public void IndexOfFindsCaseInsensitiveWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.IndexOf("Fox", IndexOfOptions.IgnoreCaseAndWhitespace);
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        public void IndexOfFindsLaterWithWhitespace()
        {
            string test = "F   ox   y l a d y f o x";
            int pos = test.IndexOf("fo x", IndexOfOptions.IgnoreCaseAndWhitespace);
            Assert.AreEqual(19, pos);
        }

        [TestMethod]
        public void IndexOfFindsWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.IndexOf("fox", IndexOfOptions.IgnoreWhitespace);
            Assert.AreEqual(23, pos);
        }

        [TestMethod]
        public void IndexOfWithWhitespaceFindsWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.IndexOf("Fo x", IndexOfOptions.IgnoreWhitespace);
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        public void LastIndexOfDoesntFindWithWhitespace()
        {
            string test = "F   ox   y l a d y f o x";
            int pos = test.LastIndexOf("do x", IndexOfOptions.IgnoreCaseAndWhitespace);
            Assert.AreEqual(-1, pos);
        }

        [TestMethod]
        public void LastIndexOfFindsCaseInsensitiveWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.LastIndexOf("Fox", IndexOfOptions.IgnoreCaseAndWhitespace);
            Assert.AreEqual(23, pos);
        }

        [TestMethod]
        public void LastIndexOfFindsWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.LastIndexOf("fox", IndexOfOptions.IgnoreWhitespace);
            Assert.AreEqual(23, pos);
        }

        [TestMethod]
        public void LastIndexOfWithWhitespaceFindsWithWhitespace()
        {
            string test = "F   o    x   y l a d y fo x";
            int pos = test.LastIndexOf("Fo x", IndexOfOptions.IgnoreWhitespace);
            Assert.AreEqual(0, pos);
        }

        [TestMethod]
        public void Trim_DoesNotTrim()
        {
            string test = "FoxyOK, so we have a string Foxy";
            string result = test.Trim("Chile");
            Assert.AreEqual(test, result);
            result = test.TrimStart("have");
            Assert.AreEqual(test, result);
        }

        [TestMethod]
        public void Trim_Trims()
        {
            string test = "FoxyOK, so we have a string Foxy";
            string result = test.Trim("Foxy");
            Assert.AreEqual("OK, so we have a string ", result);
        }

        [TestMethod]
        public void TrimEnd_DoesNotTrimEnd()
        {
            string test = "OK, so we have a string";
            string result = test.TrimEnd("GEORGE");
            Assert.AreEqual(test, result);
            result = test.TrimStart("have");
            Assert.AreEqual(test, result);
        }

        [TestMethod]
        public void TrimEnd_TrimsEnd()
        {
            string test = "OK, so we have a string";
            string result = test.TrimEnd("ring");
            Assert.AreEqual("OK, so we have a st", result);
        }

        [TestMethod]
        public void TrimStart_DoesNotTrimStart()
        {
            string test = "OK, so we have a string";
            string result = test.TrimStart("Fred");
            Assert.AreEqual(test, result);
            result = test.TrimStart("have");
            Assert.AreEqual(test, result);
        }

        [TestMethod]
        public void TrimStart_TrimsStart()
        {
            string test = "OK, so we have a string";
            string result = test.TrimStart("OK, ");
            Assert.AreEqual("so we have a string", result);
        }
    }
}

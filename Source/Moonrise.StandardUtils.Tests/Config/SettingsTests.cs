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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Config;

namespace Moonrise.StandardUtils.Tests.Config
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SettingsTests
    {
        public class TestConfigWithEnum
        {
            public TestEnum Enum { get; set; }

            public int Number { get; set; }

            public string String { get; set; }
        }

        public class TestConfigWithoutEnum
        {
            public int Number { get; set; }

            public string String { get; set; }
        }

        public enum TestEnum
        {
            Default,

            [System.ComponentModel.Description("Blueish")]
            Blue
        }

        public TestConfigWithEnum ConfigProperty { get; set; }

        [TestMethod]
        public void BasicEncryptedSettingRead()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            string base64Str = "|#Ovncfs#;2-#Tusjoh#;#uftu!tusjoh#~";
            base64Str = Convert.ToBase64String(Encoding.Unicode.GetBytes(base64Str));
            testProvider.Add("TestConfig", $"{Settings.EncryptionOpeningIdentifier}{base64Str}");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            Settings.Application.SettingsEncryptor = new StringOffsetSettingsEncryptor();
            TestConfigWithoutEnum testClass = new TestConfigWithoutEnum();
            Settings.Application.Read("TestConfig", ref testClass);
            Assert.AreEqual(testClass.Number, 1);
            Assert.AreEqual(testClass.String, "test string");
        }

        [TestMethod]
        public void ClassWithEnumDescriptionsRead()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            testProvider.Add("TestConfig", "{\"Number\":1,\"String\":\"test string\",\"Enum\":\"Blue\"}");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            TestConfigWithEnum testClass = new TestConfigWithEnum();
            Settings.Application.Read("TestConfig", ref testClass);
            Assert.AreEqual(testClass.Number, 1);
            Assert.AreEqual(testClass.String, "test string");
            Assert.AreEqual(testClass.Enum, TestEnum.Blue);
        }

        [TestMethod]
        public void ClassWithoutEnumDescriptionsRead()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            testProvider.Add("TestConfig", "{\"Number\":1,\"String\":\"test string\"}");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            TestConfigWithoutEnum testClass = new TestConfigWithoutEnum();
            Settings.Application.Read("TestConfig", ref testClass);
            Assert.AreEqual(testClass.Number, 1);
            Assert.AreEqual(testClass.String, "test string");
        }

        [TestMethod]
        public void EncryptSimpleSetting()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            string base64Str = "|#Ovncfs#;2-#Tusjoh#;#uftu!tusjoh#~";
            base64Str = Convert.ToBase64String(Encoding.Unicode.GetBytes(base64Str));
            string encrypted = $"{Settings.EncryptionOpeningIdentifier}{base64Str}";
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            Settings.Application.SettingsEncryptor = new StringOffsetSettingsEncryptor();
            TestConfigWithoutEnum testClass = new TestConfigWithoutEnum
                                              {
                                                  Number = 1,
                                                  String = "test string"
                                              };
            Settings.Application.Write("TestConfig", testClass, true);

            //Assert.AreEqual(encrypted, testProvider.settingsDictionary["TestConfig"]);

            TestConfigWithoutEnum readClass = new TestConfigWithoutEnum();
            Settings.Application.Read("TestConfig", ref readClass);
            Assert.AreEqual(testClass.Number, readClass.Number);
            Assert.AreEqual(testClass.String, readClass.String);
        }

        [TestMethod]
        public void EnumDescriptionsRead()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            testProvider.Add("Fred", "Blueish");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            TestEnum fred = TestEnum.Default;
            Settings.Application.ReadEnum("Fred", ref fred);
            Assert.AreEqual(fred, TestEnum.Blue);
        }

        [TestMethod]
        public void NullPropertyDetected()
        {
            ConfigProperty = null;
            StringSettingsProvider testProvider = new StringSettingsProvider();
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            try
            {
                Settings.Application.Read("Not There", this, () => ConfigProperty);
                Assert.Fail();
            }
            catch (ArgumentException excep)
            {
                Assert.AreEqual("property", excep.ParamName);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PartialEncryptedSettingRead()
        {
            StringSettingsProvider testProvider = new StringSettingsProvider();
            string base64Str = "#uftu!tusjoh#";
            base64Str = Convert.ToBase64String(Encoding.Unicode.GetBytes(base64Str));
            testProvider.Add("TestConfig", $"{{\"Number\":1,\"String\":{Settings.EncryptionOpeningIdentifier}{base64Str}\",\"Enum\":\"Blue\"}}");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = testProvider;
            Settings.Application.SettingsEncryptor = new StringOffsetSettingsEncryptor();
            TestConfigWithEnum testClass = new TestConfigWithEnum();
            Settings.Application.Read("TestConfig", ref testClass);
            Assert.AreEqual(testClass.Number, 1);
            Assert.AreEqual(testClass.String, "test string");
            Assert.AreEqual(testClass.Enum, TestEnum.Blue);
        }
    }
}

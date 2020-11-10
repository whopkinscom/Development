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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Config;

namespace Moonrise.StandardUtils.Tests.Config
{
    /// <summary>
    ///     Tests for <see cref="JsonConfigSettingsProvider" />
    /// </summary>
    /// Note: Each test should use a different settings filename so that the tests CAN run in parallel and not clash!
    /// We also set the provider to null before setting properly to deal with issues of constantly setting the provider in different threads.
    /// Normally only the first setter is accepted, so we make a special case for setting to null to allow that to override, typically for 
    /// testing purposes - since you wouldn't typically use different PROVIDERS across the application, different instances for different threads,
    /// yes, but that's handled within the Settings class anyway.
    [TestClass]
    public class JsonConfigSettingsProviderTests
    {
        public class TestConfig
        {
            public string One { get; set; } = "Uno";

            public int Three { get; set; } = 333;

            public bool Two { get; set; } = true;
        }

        [TestMethod]
        public void AccessByContext()
        {
            Settings.Application.SettingsProvider = null;
            JsonConfigSettingsProvider provider = new JsonConfigSettingsProvider("appsettings.json", "..\\..\\Config", null);
            Settings.Application.SettingsProvider = provider;
            string testSetting = string.Empty;
            Settings.Application.Read("ContainerStartup:Logging:LogLevel", ref testSetting);
            Assert.AreEqual("Debug", testSetting);
        }

        [TestMethod]
        public void EncryptByContext()
        {
            if (File.Exists("..\\..\\Config\\appsettings.json.enc"))
            {
                File.Delete("..\\..\\Config\\appsettings.json.enc");
            }

            if (File.Exists("..\\..\\Config\\appsettings.json.clear"))
            {
                File.Delete("..\\..\\Config\\appsettings.json");
                File.Move("..\\..\\Config\\appsettings.json.clear", "..\\..\\Config\\appsettings.json");
            }

            Settings.Application.SettingsProvider = null;
            JsonConfigSettingsProvider provider = new JsonConfigSettingsProvider("appsettings.json", "..\\..\\Config", null);
            Settings.Application.SettingsProvider = provider;
            Settings.Application.SettingsEncryptor = new DpApiSettingsEncryptor(DpApiSettingsEncryptor.ProtectionScope.User);
            string testSetting = string.Empty;

//            Settings.Application.SettingsEncryptor = new StringOffsetSettingsEncryptor();
            List<string> logTags = new List<string>();
            string logtag = "";
            Settings.Application.Read("ContainerStartup:Logging:LogTags[2]", ref logtag);
            Settings.Application.Read("ContainerStartup:Logging:LogLevel", ref testSetting);
            Settings.Application.Read("ContainerStartup:Logging:LogTags", ref logTags);
            File.Move("..\\..\\Config\\appsettings.json", "..\\..\\Config\\appsettings.json.clear");
            Settings.Application.Write("ContainerStartup:Logging:LogLevel", testSetting, true);
            Settings.Application.Write("ContainerStartup:Logging:LogTags[2]", logtag, true);
            Settings.Application.RefreshAnyCaches();
            List<string> newLogTags = new List<string>();
            Settings.Application.Read("ContainerStartup:Logging:LogTags", ref newLogTags);
            Settings.Application.Read("ContainerStartup:Logging:LogLevel", ref testSetting);
            Assert.AreEqual("Debug", testSetting);
            File.Move("..\\..\\Config\\appsettings.json", "..\\..\\Config\\appsettings.json.enc");
            File.Move("..\\..\\Config\\appsettings.json.clear", "..\\..\\Config\\appsettings.json");
        }

        [TestMethod]
        public void EncryptionWorks()
        {
            // This could obviously be any class, but why not create an anonymous one?
            var configNeeded = new
                               {
                                   Imaginary = "Not There",
                                   When = DateTime.Now,
                                   HowOften = 58,
                                   Repeatable = true
                               };

            var configRead = new
                             {
                                 Imaginary = "",
                                 When = DateTime.Now,
                                 HowOften = 0,
                                 Repeatable = false
                             };

            File.Delete("..\\..\\EWappsettings.json");
            Settings.Application.SettingsProvider = null;
            JsonConfigSettingsProvider provider = new JsonConfigSettingsProvider("EWappsettings.json", "..\\..\\");
            provider.SettingsEncryptor = new SampleEncryptor();
            Settings.Application.SettingsProvider = provider;
            Settings.Application.Write("SampleClassConfig", configNeeded);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("SampleClassConfig", ref configRead);
            Assert.AreEqual(configNeeded.Imaginary, configRead.Imaginary);
        }

        [TestMethod]
        public void EncryptionWorksWithoutApplicationConfig()
        {
            // This could obviously be any class, but why not create an anonymous one?
            var configNeeded = new
                               {
                                   Imaginary = "Not There",
                                   When = DateTime.Now,
                                   HowOften = 58,
                                   Repeatable = true
                               };

            var configRead = new
                             {
                                 Imaginary = "",
                                 When = DateTime.Now,
                                 HowOften = 0,
                                 Repeatable = false
                             };

            File.Delete("..\\..\\EWWACappsettings.json");
            Settings.Application.SettingsProvider = null;
            JsonConfigSettingsProvider provider = new JsonConfigSettingsProvider("EWWACappsettings.json", "..\\..\\", null);
            provider.SettingsEncryptor = new SampleEncryptor();
            Settings.Application.SettingsProvider = provider;
            string[] EncryptedSections =
            {
                "",
                ""
            };

            Settings.Application.Write("SampleClassConfig", configNeeded);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("SampleClassConfig", ref configRead);
            Assert.AreEqual(configNeeded.Imaginary, configRead.Imaginary);
        }

        /// <summary>
        ///     Illustrates how you can create an initial settings file, typically in a "test" in your project.
        /// </summary>
        [TestMethod]
        public void IllustrativeSettingsCreationCall()
        {
            // This could obviously be any class, but why not create an anonymous one?
            var configNeeded = new
                               {
                                   Imaginary = "Not There",
                                   When = DateTime.Now,
                                   HowOften = 58,
                                   Repeatable = true
                               };

            File.Delete("..\\..\\sampleGeneratedAppsettings.json");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("sampleGeneratedAppsettings.json", "..\\..\\");
            Settings.Application.Write("SampleClassConfig", configNeeded);
        }

        [TestMethod]
        public void SettingsCanBeAdded()
        {
            string envVar = "TEST_ENV_VAR";
            Environment.SetEnvironmentVariable(envVar, "Testy");
            var sourceConfig = new
                               {
                                   One = "Onesy",
                                   Two = true,
                                   Three = 456,
                                   Deeper = new
                                            {
                                                DeepOne = 15,
                                                DeepTwo = "Thunderbird",
                                                JohnnyDeep = new
                                                             {
                                                                 DeepDeepOne = "At One With"
                                                             }
                                            }
                               };

            var overrideConfig = new
                                 {
                                     One = "Twosey",
                                     Deeper = new
                                              {
                                                  DeepTwo = "Are Go",
                                                  DeepThree = "Blue Sea",
                                                  JohnnyDeep = new
                                                               {
                                                                   DeepDeepOne = "Everything"
                                                               }
                                              },
                                     Four = "Foresight"
                                 };

            var readConfig = new
                             {
                                 One = "",
                                 Two = false,
                                 Three = 147,
                                 Deeper = new
                                          {
                                              JohnnyDeep = new
                                                           {
                                                               DeepDeepOne = ""
                                                           },
                                              DeepOne = 0,
                                              DeepTwo = "",
                                              DeepThree = ""
                                          },
                                 Four = ""
                             };

            File.Delete("..\\..\\SCBAappsettings.json");
            File.Delete("..\\..\\SCBAappsettings.Testy.json");
            File.Delete("..\\..\\SCBAappsettings.Original.json");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("SCBAappsettings.json", "..\\..\\", null, null, envVar);
            Settings.Application.Write("Test Config", overrideConfig);
            Settings.Application.Write("New High Level", 158);

            // Move the appsettings we just wrote to be the override settings
            File.Move("..\\..\\SCBAappsettings.json", "..\\..\\SCBAappsettings.Testy.json");

            // Write the "main" settings
            Settings.Application.Write("Test Config", sourceConfig);

            // Cause the settings to be re-read, i.e. This SHOULD read the "combined" settings, i.e. main/original plus any overrides
            Settings.Application.RefreshAnyCaches();

            // Now that the settings have been read, take a copy of the original
            File.Copy("..\\..\\SCBAappsettings.json", "..\\..\\SCBAappsettings.Original.json");

            // The settings themselves have been read - by the refresh - so now simply "hydrate" the config object with the combined settings
            Settings.Application.Read("Test Config", ref readConfig);

            // Just so we can see them expressed in a JSON way, write the combined settings back out to the "main" settings file. You can have a look after the test!
            Settings.Application.Write("Test Config", readConfig);

            Assert.AreNotEqual(sourceConfig.One, readConfig.One);
            Assert.AreEqual(overrideConfig.One, readConfig.One);
            Assert.AreEqual(sourceConfig.Two, readConfig.Two);
            Assert.AreEqual(sourceConfig.Three, readConfig.Three);
            Assert.AreEqual(sourceConfig.Deeper.DeepOne, readConfig.Deeper.DeepOne);
            Assert.AreEqual(overrideConfig.Deeper.DeepTwo, readConfig.Deeper.DeepTwo);
            Assert.AreEqual(overrideConfig.Deeper.JohnnyDeep.DeepDeepOne, readConfig.Deeper.JohnnyDeep.DeepDeepOne);
            Assert.AreEqual(overrideConfig.Deeper.DeepThree, readConfig.Deeper.DeepThree);
            Assert.AreEqual(overrideConfig.Four, readConfig.Four);

            int inty = 0;
            Settings.Application.Read("New High Level", ref inty);
            Assert.AreEqual(158, inty);
        }

        [TestMethod]
        public void SettingsCanBeOveridden()
        {
            string envVar = "TEST_ENV_VAR";
            Environment.SetEnvironmentVariable(envVar, "TastyTesty");
            var sourceConfig = new
                               {
                                   One = "Onesy",
                                   Two = true,
                                   Three = 456,
                                   Deeper = new
                                            {
                                                DeepOne = 15,
                                                DeepTwo = "Thunderbird"
                                            }
                               };

            var overrideConfig = new
                                 {
                                     One = "Twosey",
                                     Deeper = new
                                              {
                                                  DeepTwo = "Are Go"
                                              }
                                 };

            var readConfig = sourceConfig;
            File.Delete("..\\..\\SCBOappsettings.json");
            File.Delete("..\\..\\SCBOappsettings.TastyTesty.json");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("SCBOappsettings.json", "..\\..\\", null, null, envVar);
            Settings.Application.Write("Test Config", overrideConfig);
            File.Move("..\\..\\SCBOappsettings.json", "..\\..\\SCBOappsettings.TastyTesty.json");
            Settings.Application.Write("Test Config", sourceConfig);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("Test Config", ref readConfig);
            Assert.AreNotEqual(sourceConfig.One, readConfig.One);
            Assert.AreEqual(overrideConfig.One, readConfig.One);
            Assert.AreEqual(sourceConfig.Two, readConfig.Two);
            Assert.AreEqual(sourceConfig.Three, readConfig.Three);
            Assert.AreEqual(sourceConfig.Deeper.DeepOne, readConfig.Deeper.DeepOne);
            Assert.AreEqual(overrideConfig.Deeper.DeepTwo, readConfig.Deeper.DeepTwo);
        }

        [TestMethod]
        public void SettingsClassCanBeWritten()
        {
            TestConfig sourceConfig = new TestConfig
                                      {
                                          One = "Ahab",
                                          Two = false,
                                          Three = 789
                                      };

            TestConfig readConfig = new TestConfig();
            File.Delete("..\\..\\SCCBWappsettings.json");

            //Settings.SettingsProvider = null;
            //Settings.SettingsProvider = new JsonConfigSettingsProvider("SCCBWappsettings.json", "..\\..\\");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("SCCBWappsettings.json", "..\\..\\");
            Settings.Application.Write("Test Config", sourceConfig);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("Test Config", ref readConfig);
            Assert.AreEqual(sourceConfig.One, readConfig.One);
            Assert.AreEqual(sourceConfig.Two, readConfig.Two);
            Assert.AreEqual(sourceConfig.Three, readConfig.Three);
        }

        [TestMethod]
        public void SingleSettingsIntCanBeWritten()
        {
            int sourceConfig = 456123;
            int readConfig = 0;
            File.Delete("..\\..\\SSICBWappsettings.json");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("SSICBWappsettings.json", "..\\..\\");
            Settings.Application.Write("Test Config", sourceConfig);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("Test Config", ref readConfig);
            Assert.AreEqual(sourceConfig, readConfig);
        }

        [TestMethod]
        public void SingleSettingsStringCanBeWritten()
        {
            string sourceConfig = "Suzie";
            string readConfig = string.Empty;
            File.Delete("..\\..\\SSSCBWappsettings.json");
            Settings.Application.SettingsProvider = null;
            Settings.Application.SettingsProvider = new JsonConfigSettingsProvider("SSSCBWappsettings.json", "..\\..\\");
            Settings.Application.Write("Test Config", sourceConfig);
            Settings.Application.RefreshAnyCaches();
            Settings.Application.Read("Test Config", ref readConfig);
            Assert.AreEqual(sourceConfig, readConfig);
        }

        //[TestMethod]
        //public void EncryptExample()
        //{
        //    JsonConfigSettingsProvider provider = new JsonConfigSettingsProvider("Unenc.json", "..\\..\\", null);
        //    Settings.SettingsProvider = provider;
        //    Settings.Application.RefreshAnyCaches();
        //    provider.SettingsEncryptor = new SampleEncryptor();
        //    Settings.Application.Flush();

        //}
    }
}

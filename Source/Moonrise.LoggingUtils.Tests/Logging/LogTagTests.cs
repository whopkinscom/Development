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
using Moonrise.Utils.Test.Logging;
using System;

namespace Moonrise.Logging.Tests.Logging
{
    [TestClass]
    public class LogTagTests
    {
        private LogTag tag1 = new LogTag("tag one");

        [TestMethod]
        public void LogTagsGetAdded()
        {
            TestLogAuditProvider logProvider = new TestLogAuditProvider();
            Logger.LogProvider = null;
            Logger.LogProvider = logProvider;
            Logger.ActivateLogTag("tag one");
            Logger.ActivateLogTag("tag two");
            Logger.ActivateLogTag("tag three");
            Logger.LogActiveLogTags();
            Assert.IsTrue(logProvider.LogBuffer[1].Message.Contains("tag one"));
            Assert.IsTrue(logProvider.LogBuffer[1].Message.Contains("tag two"));
            Assert.IsTrue(logProvider.LogBuffer[1].Message.Contains("tag three"));
        }

        [TestMethod]
        public void LogTagDoDoes()
        {
            TestLogAuditProvider logProvider = new TestLogAuditProvider();
            Logger.LogProvider = null;
            Logger.LogProvider = logProvider;
            Logger.OutputLevel = Logger.ReportingLevel.All;
            Logger.ActivateLogTag("tag one");

            try
            {
                tag1.Do(() =>
                {
                    Logger.Info("Fairy message");
                    int fred = 0;
                    int jim = 5 / fred;
                });
            }
            catch (Exception)
            { }

            Assert.IsTrue(logProvider.LogBuffer[0].Message.Contains("Fairy"));
            Assert.IsTrue(logProvider.LogBuffer[1].Message.Contains("LogTagDoDoes"));
        }
    }
}

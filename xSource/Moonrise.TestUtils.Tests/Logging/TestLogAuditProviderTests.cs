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
using Moonrise.Logging;
using Moonrise.Utils.Test.Logging;

namespace Moonrise.TestUtils.Tests.Logging
{
    [TestClass]
    public class TestLogAuditProviderTests
    {
        [TestMethod]
        public void BufferRetainsLogs()
        {
            TestLogAuditProvider sut = new TestLogAuditProvider();
            Logger.LogProvider = sut;
            Logger.OutputLevel = Logger.ReportingLevel.Debug;
            string logMsg = "Message 1";

            Logger.Debug(logMsg);

            Assert.AreEqual(1, sut.LogBuffer.Count);
            Assert.AreEqual(logMsg, sut.LogBuffer[0].Message);
            Assert.AreEqual(LoggingLevel.Debug, sut.LogBuffer[0].Level);
        }
    }
}

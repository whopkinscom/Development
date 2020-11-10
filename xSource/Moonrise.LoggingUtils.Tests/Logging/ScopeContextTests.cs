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
using Moonrise.Utils.Test.Logging;

namespace Moonrise.Logging.Tests.Logging
{
    [TestClass]
    public class ScopeContextTests
    {
        [TestMethod]
        public void ScopeContextGetsAdded()
        {
            TestLogAuditProvider logProvider = new TestLogAuditProvider();
            Logger.LogProvider = null;
            Logger.LogProvider = logProvider;
            Logger.OutputLevel = Logger.ReportingLevel.All;
            Logger.MethodInfo();
            Logger.MethodInfo(Logger.Args(1));
            Logger.Info("Zero");
            using (Logger.Context())
            {
                Logger.Info("One");

                using (Logger.Context("Test 2"))
                {
                    Logger.Info("Two");
                    using (Logger.Context(Logger.Args("a", 123, DateTime.Now)))
                    {
                        Logger.Info("Yeeha");
                    }
                }

                Logger.Info("Three");
            }

            Logger.Info("Four");
        }

        [TestMethod]
        public void ScopeContextGetsLogged()
        {
            TestLogAuditProvider logProvider = new TestLogAuditProvider();
            Logger.LogProvider = null;
            Logger.LogProvider = logProvider;
            Logger.OutputLevel = Logger.ReportingLevel.All;
            Logger.UseContext = true;
            Logger.Info("Zero");

            using (Logger.Context("Test 1"))
            {
                Logger.Info("One");

                using (Logger.Context("Test 2"))
                {
                    Logger.Info("Two");
                }

                Logger.Info("Three");
            }

            Logger.Info("Four");
        }
    }
}

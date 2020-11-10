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

namespace Moonrise.Logging.Tests.Logging
{
    [TestClass]
    public class WindowsEventLogProviderTests
    {
#if OnlyRunLocally /// <summary>
/// Requires a manual check to ensure the messages have been written as expected.
/// </summary>
        [TestMethod]
        public void LogsToWindowsLogsApplication()
        {
            WindowsEventLogProvider provider = new WindowsEventLogProvider("WindowsEventLogProviderTests");
            Logger.LogProvider = null;
            Logger.LogProvider = provider;
            Logger.OutputLevel = Logger.ReportingLevel.Debug;
            Logger.Debug("Debug Message");
            Logger.Info("Info Message");
            Logger.Warning("Warning Message");
            Logger.Error("Error Message");
        }

        /// <summary>
        /// Requires a manual check to ensure the messages have been written as expected.
        /// </summary>
        [TestMethod]
        public void LogsToApplicationLogs()
        {
            WindowsEventLogProvider provider = new WindowsEventLogProvider("Tests", "WindowsEventLogProviderTest");
            Logger.LogProvider = null;
            Logger.LogProvider = provider;
            Logger.OutputLevel = Logger.ReportingLevel.Debug;
            Logger.Debug("DebugMessage");
            Logger.Info("InfoMessage");
            Logger.Warning("WarningMessage");
            Logger.Error("ErrorMessage");
        }
#endif
    }
}

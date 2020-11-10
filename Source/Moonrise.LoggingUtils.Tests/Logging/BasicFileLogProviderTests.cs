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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Logging.LoggingProviders;

namespace Moonrise.Logging.Tests.Logging
{
    [TestClass]
    public class BasicFileLogProviderTests
    {
#if OnlyRunLocally /// <summary>
///     Creates multiple log files by running up multiple threads. NOTE That depending on how the threads get allocated out
///     there may well be less files than you expect but there SHOULD only ever be the same thread id represented in each
///     file, i.e. threads get re-used!
/// </summary>
        [TestMethod]
        public void ThreadedLoggingCreatesMultipleFiles()
        {
            BasicFileLogProvider provider = new BasicFileLogProvider(@"c:\temp\BasicFileLogProviderTests.log",
                                                                     "{0: HH:mm:ss.fff} : ",
                                                                     BasicFileLogProvider.Cycle.Always);
            BasicFileLogProvider auditor = new BasicFileLogProvider(@"c:\temp\BasicFileLogProviderTests-Audit.log",
                                                                     "{0: HH:mm:ss.fff} : ",
                                                                     BasicFileLogProvider.Cycle.Always);
            Logger.ReplaceLoggingProvider(provider);
            Logger.AuditProvider = auditor;
            Logger.OutputLevel = Logger.ReportingLevel.Debug;
            int workers,
                completion;
            ThreadPool.GetMaxThreads(out workers, out completion);
            Logger.Info("Workers = {0}, Completion = {1}", workers, completion);

            using (CountdownEvent allOverFolks = new CountdownEvent(1))
            {
                for (int i = 0; i < 10; i++)
                {
                    allOverFolks.AddCount();
                    Logger.Info("Kicking off a thread, id = {0}", i);

                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        try
                        {
                            ThreadedLogging((int)s);
                        }
                        finally
                        {
                            allOverFolks.Signal();
                            Logger.MainLogProvider.LogThis(LoggingLevel.Debug, "Signalled " + s + " id=" + Thread.CurrentThread.ManagedThreadId);
                        }
                    },
                                                 i);
                }

                // Right, we've finished
                allOverFolks.Signal();
                Logger.Info("All threads kicked off");

                // And now wait for the others
                allOverFolks.Wait();
                Logger.Info("All work finished");
            }
        }

        private void ThreadedLogging(int i)
        {
            Logger.Info("Thread number - {0}, id = {1}", i, Thread.CurrentThread.ManagedThreadId);

            for (int j = 0; j < 1000; j++)
            {
                Logger.Debug("Thread {0}: Count = {1}", i, j);
            }

            Logger.Info("Thread loop finished  - {0}, id = {1}", i, Thread.CurrentThread.ManagedThreadId);
            Logger.Audit("Auditing the loop, id = {0}, threadid = {1}", i, Thread.CurrentThread.ManagedThreadId);
        }
#endif
    }
}

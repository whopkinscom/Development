#region Apache-v2.0

//    Copyright 2016 Will Hopkins - Moonrise Media Ltd.
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

namespace Moonrise.Utils.Standard.Threading.Tests
{
    [TestClass]
    public class ScopedNestableThreadGlobalSingletonTests
    {
        public class SUT : ScopedNestableThreadGlobalSingleton<string>
        {
            public SUT(string value)
                : base(value) { }
        }

        [TestMethod]
        public void BasicNestingWorks()
        {
            Assert.AreEqual(SUT.CurrentValue, null);

            using (new SUT("test1"))
            {
                Assert.AreEqual(SUT.CurrentValue, "test1");

                using (new SUT("antifreeze"))
                {
                    Assert.AreEqual(SUT.CurrentValue, "antifreeze");
                }

                Assert.AreEqual(SUT.CurrentValue, "test1");
            }
        }

        [TestMethod]
        public void ThreadedNestingWorks()
        {
            using (new SUT("Outermost"))
            {
                using (CountdownEvent allOverFolks = new CountdownEvent(1))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Assert.AreEqual(SUT.CurrentValue, "Outermost");

                        using (new SUT("Innermost-ish"))
                        {
                            allOverFolks.AddCount();
                            ThreadPool.QueueUserWorkItem(s =>
                                                         {
                                                             try
                                                             {
                                                                 ThreadedNested((int)s);
                                                             }
                                                             finally
                                                             {
                                                                 allOverFolks.Signal();
                                                             }
                                                         },
                                                         i);

                            Assert.AreEqual(SUT.CurrentValue, "Innermost-ish");
                        }
                    }

                    // Right, we've finished
                    allOverFolks.Signal();

                    // And now wait for the others
                    allOverFolks.Wait();
                }

                Assert.AreEqual(SUT.CurrentValue, "Outermost");
            }
        }

        private void ThreadedNested(int i)
        {
            // In a new thread the current value should be null until we set upp a
            Assert.AreEqual(SUT.CurrentValue, null);

            string str1 = i.ToString();
            string str2 = i + i.ToString();

            using (new SUT(str1))
            {
                Assert.AreEqual(SUT.CurrentValue, str1);

                using (new SUT(str2))
                {
                    Assert.AreEqual(SUT.CurrentValue, str2);
                }

                Assert.AreEqual(SUT.CurrentValue, str1);
            }
        }
    }
}

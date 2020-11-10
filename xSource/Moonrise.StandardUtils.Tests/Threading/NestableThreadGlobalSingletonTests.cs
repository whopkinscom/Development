// <copyright file="NestableThreadGlobalSingletonTests.cs" company="Moonrise Media Ltd.">
// Originally written by WillH - with any acknowledgements as required. Once checked in to your version control you have full rights except for selling the source!
// </copyright>

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moonrise.Utils.Standard.Threading.Tests
{
    [TestClass]
    public class NestableThreadGlobalSingletonTests
    {
        public class SUT : NestableThreadGlobalSingleton<string>
        {
            public SUT(string value)
                : base(value)
            {
            }
        }

        [TestMethod]
        public void BasicNestingWorks()
        {
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

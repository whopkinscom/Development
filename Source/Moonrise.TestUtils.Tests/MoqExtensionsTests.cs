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
using Moonrise.Utils.Test.Extensions;
using Moq;

namespace Moonrise.TestUtils.Tests
{
    [TestClass]
    public class MoqExtensionsTests
    {
        public interface IOutParams
        {
            void Out1(out int one);

            bool ReturnsTake3Out3(int i, int j, int k, out int x, out bool y, out string z);

            void Take3Out3(int i, int j, int k, out int x, out bool y, out string z);
        }

        [TestMethod]
        public void OutCallback_CanDoStuffInTheCallbackThough()
        {
            Mock<IOutParams> mocked = new Mock<IOutParams>();
            int theOne = 998;
            mocked.Setup(op => op.Out1(out theOne)).OutCallback((out int o) => o = theOne + 1);

            int otherOne;
            mocked.Object.Out1(out otherOne);

            Assert.AreEqual(999, otherOne);
        }

        [TestMethod]
        public void OutCallback_NoReturnTakes3Sets3()
        {
            Mock<IOutParams> mocked = new Mock<IOutParams>();
            int x;
            bool y;
            string z;
            mocked.Setup(op => op.Take3Out3(1, 2, 3, out x, out y, out z)).OutCallback(
                (int i, int j, int k, out int X, out bool Y, out string Z) =>
                {
                    X = 999;
                    Y = true;
                    Z = "Fred";
                });

            mocked.Object.Take3Out3(1, 2, 3, out x, out y, out z);

            Assert.AreEqual(999, x);
            Assert.IsTrue(y);
            Assert.AreEqual("Fred", z);
        }

        [TestMethod]
        public void OutCallback_ReturnsTakes3Sets3()
        {
            Mock<IOutParams> mocked = new Mock<IOutParams>();
            int x;
            bool y;
            string z;
            mocked.Setup(op => op.ReturnsTake3Out3(1, 2, 3, out x, out y, out z)).OutCallback(
                (int i, int j, int k, out int X, out bool Y, out string Z) =>
                {
                    X = 999;
                    Y = true;
                    Z = "Fred";
                }).Returns(true);

            bool res = mocked.Object.ReturnsTake3Out3(1, 2, 3, out x, out y, out z);

            Assert.IsTrue(res);
            Assert.AreEqual(999, x);
            Assert.IsTrue(y);
            Assert.AreEqual("Fred", z);
        }

        [TestMethod]
        public void OutCallback_Sets1()
        {
            Mock<IOutParams> mocked = new Mock<IOutParams>();
            int theOne;
            mocked.Setup(op => op.Out1(out theOne)).OutCallback((out int o) => o = 999);

            int otherOne;
            mocked.Object.Out1(out otherOne);

            Assert.AreEqual(999, otherOne);
        }

        [TestMethod]
        public void OutCallback_ToJustSetIsNotNeeded()
        {
            Mock<IOutParams> mocked = new Mock<IOutParams>();
            int theOne = 100;
            mocked.Setup(op => op.Out1(out theOne));

            int otherOne;
            mocked.Object.Out1(out otherOne);

            Assert.AreEqual(100, otherOne);
        }
    }
}

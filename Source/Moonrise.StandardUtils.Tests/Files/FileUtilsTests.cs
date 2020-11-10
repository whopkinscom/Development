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
using Moonrise.Utils.Standard.Files;

namespace Moonrise.StandardUtils.Tests.Files
{
    /// <summary>
    ///     Summary description for FileUtilsTests
    /// </summary>
    [TestClass]
    public class FileUtilsTests
    {
        [TestMethod]
        public void GetParentDetectsFolderParent()
        {
            string path = "C:\\abba\\dionne\\fred\\";

            string result = FileUtils.GetParentDirectory(path);
            Assert.AreEqual("C:\\abba\\dionne", result);
        }

        [TestMethod]
        public void GetParentDetectsNoParent()
        {
            string path = "C:\\fred.txt";

            string result = FileUtils.GetParentDirectory(path);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetParentDetectsParent()
        {
            string path = "C:\\abba\\dionne\\fred.txt";

            string result = FileUtils.GetParentDirectory(path);
            Assert.AreEqual("C:\\abba", result);
        }

        [TestMethod]
        public void LinuxGetParentDetectsFolderParent()
        {
            string path = "C:/abba/dionne/fred/";

            string result = FileUtils.GetParentDirectory(path);
            Assert.AreEqual("C:\\abba\\dionne", result);
        }

        [TestMethod]
        public void LinuxGetParentDetectsNoParent()
        {
            string path = "C:/fred.txt";

            string result = FileUtils.GetParentDirectory(path);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void LinuxGetParentDetectsParent()
        {
            string path = "C:/abba/dionne/fred.txt";

            string result = FileUtils.GetParentDirectory(path);
            Assert.AreEqual("C:\\abba", result);
        }
    }
}

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

namespace Moonrise.Utils.Standard.Networking.Tests
{
    [TestClass]
    public class NetworkUtilsTests
    {
        [TestMethod]
        public void IpAddressFullDotNotationRequiredIsInvalid()
        {
            Assert.IsFalse(NetworkUtils.IsIPAddress("100:100", true, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100:100", true, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100.100:100", true, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100.100.100:100", true, false));
        }

        [TestMethod]
        public void IpAddressFullDotNotationRequiredIsValid()
        {
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100.100.100:100", true));
        }

        [TestMethod]
        public void IpAddressFullDotNotationRequiredPortNotAllowedIsInvalid()
        {
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100.100.100:100", true, false));
        }

        [TestMethod]
        public void IpAddressIsValid()
        {
            Assert.IsTrue(NetworkUtils.IsIPAddress("100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100.100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100.100.100"));
        }

        [TestMethod]
        public void IpAddressPortAllowedIsValid()
        {
            Assert.IsTrue(NetworkUtils.IsIPAddress("100:100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100:100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100.100:100"));
            Assert.IsTrue(NetworkUtils.IsIPAddress("100.100.100.100:100"));
        }

        [TestMethod]
        public void IpAddressPortNotAllowedIsInvalid()
        {
            Assert.IsFalse(NetworkUtils.IsIPAddress("100:100", false, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100:100", false, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100.100:100", false, false));
            Assert.IsFalse(NetworkUtils.IsIPAddress("100.100.100.100:100", false, false));
        }

        [TestMethod]
        public void IpV6AddressIsValid()
        {
            Assert.IsTrue(NetworkUtils.IsIPAddress("2001:db8:1f70::999:de8:7648:6e8"));
        }
    }
}

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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.DatesTimes;
using Moonrise.Utils.Standard.Extensions;

namespace MoonriseStandardUtilsTests.DatesTimes
{
    [TestClass]
    public class DateTimeOffsetProviderTests
    {
        public class FutureTimeProvider : IDateTimeOffsetProvider
        {
            public DateTimeOffset Now
            {
                get
                {
                    // Give the time as 5 days in the future
                    return DateTimeOffset.Now.AddDays(5.0);
                }
            }
        }

        public class PastTimeProvider : IDateTimeOffsetProvider
        {
            public DateTimeOffset Now
            {
                get
                {
                    // Give the time as 5 days ago
                    return DateTimeOffset.Now.AddDays(-5.0);
                }
            }
        }

        [TestMethod]
        public void GoingBackInTime()
        {
            Assert.IsTrue(DateTimeOffsetProvider.Now.Within(1, LastFew.Seconds));

            DateTimeOffsetProvider.Provider = new PastTimeProvider();

            // We can't check on EXACTLY 5 days ago as a minus 5 days puts us so close that time can have moved on by a microsecond.
            Assert.IsTrue(DateTimeOffsetProvider.Now.Within(6, LastFew.Days));
            Assert.IsFalse(DateTimeOffsetProvider.Now.Within(1, LastFew.Seconds));
            Assert.IsFalse(DateTimeOffsetProvider.Now.Within(4, LastFew.Days));

            DateTimeOffsetProvider.Provider = null;
        }

        [TestMethod]
        public void GoingForwardInTime()
        {
            Assert.IsTrue(DateTimeOffsetProvider.Now.Within(1, LastFew.Seconds));
            Assert.IsFalse(DateTimeOffsetProvider.Now.AddDays(-5).Within(1, LastFew.Seconds));

            DateTimeOffsetProvider.Provider = new FutureTimeProvider();

            DateTimeOffset futureNow = DateTimeOffsetProvider.Now.AddDays(-5);
            Assert.IsTrue(futureNow.Within(1, LastFew.Seconds));

            DateTimeOffsetProvider.Provider = null;
        }

        [TestMethod]
        public void SettingNoProviderSuppliesCurrentTime()
        {
            Assert.IsTrue(DateTimeOffsetProvider.Now.Within(1, LastFew.Seconds));
        }
    }
}

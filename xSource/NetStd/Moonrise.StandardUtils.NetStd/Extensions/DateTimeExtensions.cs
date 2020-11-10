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
using System.Diagnostics.CodeAnalysis;

namespace Moonrise.Utils.Standard.Extensions
{
    /// <summary>
    ///     Enum for use with <see cref="DateTimeExtensions.Within" />.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Obvious")]
#pragma warning disable 1591
    public enum LastFew
    {
        Seconds,
        Minutes,
        Hours,
        Days,
        Months,
        Years
    }
#pragma warning restore 1591

    /// <summary>
    ///     Extensions for the <see cref="DateTime" /> class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Converts value of DateTime to local time and optionally appends time zone info
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="includeTimezone">True to append time zone info</param>
        /// <returns>
        ///     A localised DateTime
        /// </returns>
        public static string ToLocalTime(this DateTime dateTime, bool includeTimezone)
        {
            if (!includeTimezone)
            {
                return dateTime.ToLocalTime().ToString();
            }

            return string.Format("{0:dd/MM/yyyy HH:mm:ss} {1}",
                                 dateTime.ToLocalTime(false),
                                 TimeZoneInfo.Local.IsDaylightSavingTime(dateTime) ? "BST" : "GMT");
        }

        /// <summary>
        ///     Determines if the <see cref="DateTime" /> is within the specified number of whatever <see cref="LastFew" /> units.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="number">The number.</param>
        /// <param name="units">The <see cref="LastFew" /> units.</param>
        /// <returns>True if the relevant date time is within the last few units of now.</returns>
        public static bool Within(this DateTime dateTime, int number, LastFew units)
        {
            bool retVal = false;

            DateTime current = DateTime.Now;
            DateTime noLessThan;
            double subtractor = -1.0 * number;

            switch (units)
            {
                case LastFew.Seconds:
                    noLessThan = current.AddSeconds(subtractor);
                    break;
                case LastFew.Minutes:
                    noLessThan = current.AddMinutes(subtractor);
                    break;
                case LastFew.Hours:
                    noLessThan = current.AddHours(subtractor);
                    break;
                case LastFew.Days:
                    noLessThan = current.AddDays(subtractor);
                    break;
                case LastFew.Months:
                    noLessThan = current.AddMonths(-1 * number);
                    break;
                case LastFew.Years:
                    noLessThan = current.AddYears(-1 * number);
                    break;
                default:
                    noLessThan = current;
                    break;
            }

            retVal = (dateTime >= noLessThan) && (dateTime <= current);

            return retVal;
        }
    }
}

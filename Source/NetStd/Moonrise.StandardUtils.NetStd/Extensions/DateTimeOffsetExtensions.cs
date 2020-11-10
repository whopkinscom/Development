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
    ///     Contains extension methods for DateTimeOffset.
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        ///     What to trim off
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Obvious")]
#pragma warning disable 1591
        public enum TrimOff
        {
            Milliseconds,
            Seconds,
            Minutes,
            Hours,
            Days,
            Months
        }
#pragma warning restore 1591

        /// <summary>
        ///     Trims a DateTimeOffset to a whole number of seconds
        /// </summary>
        /// <param name="dto">The DateTimeOffset value to truncate</param>
        /// <param name="trim">The part of the DTO to trim off</param>
        /// <returns>A new DateTimeOffset with 0 milliseconds</returns>
        public static DateTimeOffset Truncate(this DateTimeOffset dto, TrimOff trim)
        {
            switch (trim)
            {
                case TrimOff.Milliseconds:
                    return new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, 0);
                case TrimOff.Seconds:
                    return new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, 0, 0);
                case TrimOff.Minutes:
                    return new DateTime(dto.Year, dto.Month, dto.Day, dto.Hour, 0, 0, 0);
                case TrimOff.Hours:
                    return new DateTime(dto.Year, dto.Month, dto.Day, 0, 0, 0, 0);
                case TrimOff.Days:
                    return new DateTime(dto.Year, dto.Month, 0, 0, 0, 0, 0);
                case TrimOff.Months:
                    return new DateTime(dto.Year, 0, 0, 0, 0, 0, 0);
                default:
                    return dto;
            }
        }

        /// <summary>
        ///     Determines if the <see cref="DateTimeOffset" /> is within the specified number of whatever <see cref="LastFew" />
        ///     units.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="number">The number.</param>
        /// <param name="units">The <see cref="LastFew" /> units.</param>
        /// <returns>True if the relevant date time is within the last few units of now.</returns>
        public static bool Within(this DateTimeOffset dateTime, int number, LastFew units)
        {
            bool retVal = false;

            DateTimeOffset current = DateTimeOffset.Now;
            DateTimeOffset noLessThan;
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

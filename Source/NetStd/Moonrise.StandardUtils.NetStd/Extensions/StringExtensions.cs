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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Moonrise.Utils.Standard.Extensions
{
    /// <summary>
    ///     Options to apply then using the overloaded IndexOf/LastIndexOf that can ignore whitespace in the target string
    /// </summary>
    public enum IndexOfOptions
    {
        /// <summary>
        ///     Ignore the case - this results in the default IndexOf/LastIndexOf being used
        /// </summary>
        IgnoreCase,

        /// <summary>
        ///     Ignore both the case and whitespace. So looking for "fox" would match both "Fo\r\n\txy lady" AND "she was a foxy
        ///     lady"
        /// </summary>
        IgnoreCaseAndWhitespace,

        /// <summary>
        ///     Ignore whitespace in the target. So looking for "fox" would match "she was a f o\nxy lady" but not "Fo\r\n\txy
        ///     lady"
        /// </summary>
        IgnoreWhitespace,
    }

    /// <summary>
    ///     Extension methods for use with strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Creates a Comma Separated List string. Well anything separated list really.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="list">A list of the generic type</param>
        /// <param name="separator">The separator character(s)</param>
        /// <returns>A string with each element separated by, well whatever you pass.</returns>
        public static string CSL<T>(this IEnumerable<T> list, string separator) => list.CSL(separator, null, null);

        /// <summary>
        ///     Creates a Comma Separated List string. Well anything separated list really.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="list">A list of the generic type</param>
        /// <param name="separator">The separator character(s)</param>
        /// <param name="surroundLeft">The string to surround the individual element in on the left</param>
        /// <param name="surroundRight">The string to surround the individual element in on the right</param>
        /// <returns>A string with each element separated by, well whatever you pass.</returns>
        public static string CSL<T>(
            this IEnumerable<T> list,
            string separator,
            string surroundLeft,
            string surroundRight)
        {
            StringBuilder joined = new StringBuilder();

            // Create a comma seperated list out of the list of things
            foreach (T item in list)
            {
                if (joined.Length > 0)
                {
                    joined.Append(separator);
                }

                if (!string.IsNullOrEmpty(surroundLeft))
                {
                    joined.Append(surroundLeft);
                }

                joined.Append(item);

                if (!string.IsNullOrEmpty(surroundRight))
                {
                    joined.Append(surroundRight);
                }
            }

            return joined.ToString();
        }

        /// <summary>
        ///     Object extension method to get description of a type.
        ///     <para>
        ///         The description is as specified by the DescriptionAttribute, or the basic type name if no description.
        ///     </para>
        /// </summary>
        /// <param name="instance">The instance to get the type description of.</param>
        /// <returns>The description of the instance's type</returns>
        public static string Description(this object instance)
        {
            string retVal;
            Type objectType = instance.GetType();

            // Check for a special case where we are being asked for a description of a type, rather than an instance
            if (instance is Type)
            {
                objectType = instance as Type;
            }

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])objectType.GetTypeInfo().GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                retVal = attributes[0].Description;
            }
            else
            {
                retVal = objectType.Name;
            }

            return retVal;
        }

        /// <summary>
        ///     Extracts a string using a start marker and optional end marker.
        /// </summary>
        /// <param name="fromWhat">The string to look in</param>
        /// <param name="startMarker">The start to look for</param>
        /// <param name="endMarker">The end to look for</param>
        /// <param name="trim">if set to <c>true</c> DOES trim the returned string.</param>
        /// <returns>
        ///     The extracted, trimmed string.
        /// </returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static string Extract(
            this string fromWhat,
            string startMarker,
            string endMarker = null,
            bool trim = true)
        {
            int start = 0;

            return fromWhat.Extract(ref start,
                startMarker,
                1,
                endMarker,
                trim);
        }

        /// <summary>
        ///     Extracts a string using a start point, then a start marker and end marker.
        /// </summary>
        /// <param name="fromWhat">The string to look in</param>
        /// <param name="start">Where in the string to start looking. Updated with the point AFTER the end marker</param>
        /// <param name="startMarker">The start to look for</param>
        /// <param name="endMarker">The end to look for</param>
        /// <param name="trim">if set to <c>true</c> DOES trim the returned string.</param>
        /// <returns>
        ///     The extracted, trimmed string.
        /// </returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static string Extract(
            this string fromWhat,
            ref int start,
            string startMarker,
            string endMarker,
            bool trim = true) =>
            fromWhat.Extract(ref start,
                startMarker,
                1,
                endMarker,
                trim);

        /// <summary>
        ///     Extracts a string using a start point, then a number of start markers and end marker.
        /// </summary>
        /// <param name="fromWhat">The string to look in</param>
        /// <param name="start">Where in the string to start looking. Updated with the point AFTER the end marker</param>
        /// <param name="startMarker">The start to look for</param>
        /// <param name="howMany">How many markers to get past</param>
        /// <param name="endMarker">The end to look for</param>
        /// <param name="trim">if set to <c>true</c> DOES trim the returned string.</param>
        /// <returns>The extracted, trimmed string.</returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static string Extract(
            this string fromWhat,
            ref int start,
            string startMarker,
            int howMany,
            string endMarker,
            bool trim = true)
        {
            fromWhat.FindStart(ref start, startMarker, howMany);
            return fromWhat.Extract(ref start, endMarker);
        }

        /// <summary>
        ///     Extracts a string using a start point and end marker.
        /// </summary>
        /// <param name="fromWhat">The string to look in</param>
        /// <param name="start">Where in the string to start looking. Updated with the point AFTER the end marker</param>
        /// <param name="endMarker">What to look for</param>
        /// <param name="trim">if set to <c>true</c> DOES trim the returned string.</param>
        /// <returns>The extracted, trimmed string.</returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static string Extract(
            this string fromWhat,
            ref int start,
            string endMarker,
            bool trim = true)
        {
            string retVal;
            int end = fromWhat.FindEnd(start, endMarker);
            int length = end - start;
            retVal = fromWhat.Substring(start, length);

            if (trim)
            {
                retVal = retVal.Trim();
            }

            if (endMarker == null)
            {
                start = end;
            }
            else
            {
                start = end + endMarker.Length;
            }

            return retVal;
        }

        /// <summary>
        ///     Finds the end point of a string to extract.
        /// </summary>
        /// <param name="inWhat">The string to look for it in</param>
        /// <param name="start">Where in the string to start looking</param>
        /// <param name="endMarker">What to look for</param>
        /// <returns>The point at which the end marker is found</returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static int FindEnd(this string inWhat, int start, string endMarker)
        {
            int end = inWhat.Length - 1;

            if (endMarker != null)
            {
                end = AssertFound(inWhat.IndexOf(endMarker, start));
            }
            else
            {
                end++;
            }

            return end;
        }

        /// <summary>
        ///     Finds the start point of a string to extract based on skipping past a single start marker.
        /// </summary>
        /// <param name="inWhat">The string to look for it in</param>
        /// <param name="start">Where in the string to start looking. Updated with the point AFTER the start marker</param>
        /// <param name="startMarker">What to look for</param>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static void FindStart(this string inWhat, ref int start, string startMarker)
        {
            inWhat.FindStart(ref start, startMarker, 1);
        }

        /// <summary>
        ///     Finds the start point of a string to extract based on skipping a number of start markers
        /// </summary>
        /// <param name="inWhat">The string to look for it in</param>
        /// <param name="start">Where in the string to start looking. Updated with the point AFTER the last start marker</param>
        /// <param name="startMarker">What to look for</param>
        /// <param name="howMany">How many markers to get past</param>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        public static void FindStart(
            this string inWhat,
            ref int start,
            string startMarker,
            int howMany)
        {
            for (int i = 0; i < howMany; i++)
            {
                start = AssertFound(inWhat.IndexOf(startMarker, start));
                start += startMarker.Length;
            }
        }

        /// <summary>
        ///     Finds a string inside another but can ignore any whitespace in the target string. e.g. Look for "fox" will match
        ///     "fo x", "f   o x" or "f\r\no\tx"!
        /// </summary>
        /// <param name="inWhat">The string to look in</param>
        /// <param name="findThis">The string to look for</param>
        /// <param name="options">How you want to search</param>
        /// <returns>The index of the START of the (potentially) whitespace interrupted string</returns>
        public static int IndexOf(this string inWhat, string findThis, IndexOfOptions options)
        {
            int retVal = -1;

            if (options == IndexOfOptions.IgnoreCase)
            {
                return inWhat.IndexOf(findThis, StringComparison.CurrentCultureIgnoreCase);
            }

            int currentPos;
            int previousPos = 0;
            int endPos = previousPos;
            string potentialMatch = string.Empty;
            StringComparison compareOption = StringComparison.CurrentCulture;

            if (options == IndexOfOptions.IgnoreCaseAndWhitespace)
            {
                compareOption = StringComparison.CurrentCultureIgnoreCase;
            }

            bool startAgain = true;

            while (startAgain)
            {
                startAgain = false;

                foreach (char findChar in findThis)
                {
                    currentPos = inWhat.IndexOf(new string(findChar, 1), previousPos, compareOption);

                    if (currentPos == -1)
                    {
                        retVal = currentPos;
                        break;
                    }

                    if (retVal == -1)
                    {
                        retVal = currentPos;
                        endPos = currentPos + 1;
                        previousPos = currentPos;
                        potentialMatch = new string(findChar, 1);
                    }
                    else
                    {
                        string got = inWhat.Substring(previousPos + 1, currentPos - previousPos).Trim();

                        if (got.Length == 0 && char.IsWhiteSpace(findChar))
                        {
                            // Whitespace within the findThis is a special case!
                            got = new string(findChar, 1);
                        }

                        potentialMatch += got;

                        if (findThis.IndexOf(potentialMatch, compareOption) == -1)
                        {
                            previousPos = endPos;
                            retVal = -1;
                            startAgain = true;
                            break;
                        }

                        previousPos = currentPos;
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Determines if a string contains something that at least looks like an email address
        /// </summary>
        /// <param name="email">String to check</param>
        /// <returns>true or false</returns>
        public static bool IsValidEmail(this string email) =>
            new Regex(@"^\s*[\w\-\+_']+(\.[\w\-\+_']+)*\@[a-z0-9]([\w\.-]*[a-z0-9])?\.[a-z][a-z\.]*[a-z]$",
                RegexOptions.IgnoreCase).IsMatch(email);

        /// <summary>
        ///     Determines if a string contains something that at least looks like a phone number
        /// </summary>
        /// It should only have '0'-'9' ' ' '(' ')' '+' '-' and then be at least 11 chars long.
        /// The characters that allow "ext", "ddi", "direct" or "dial" are also allowed.
        /// <param name="phone">String to check</param>
        /// <returns>true or false</returns>
        public static bool IsValidPhone(this string phone)
        {
            Regex reg = new Regex(@"[0-9 \(\)\+\-extdirectal]+", RegexOptions.IgnoreCase);
            return reg.IsMatch(phone) && phone.Length > 10;
        }

        /// <summary>
        ///     Reverse finds a string inside another but can ignore any whitespace in the target string. e.g. Look for "fox" will
        ///     match "fo x", "f   o x" or "f\r\no\tx"!
        /// </summary>
        /// <param name="inWhat">The string to look in</param>
        /// <param name="findThis">The string to look for</param>
        /// <param name="options">How you want to search</param>
        /// <returns>The index of the START of the (potentially) whitespace interrupted string</returns>
        public static int LastIndexOf(this string inWhat, string findThis, IndexOfOptions options)
        {
            int retVal = -1;

            if (options == IndexOfOptions.IgnoreCase)
            {
                return inWhat.LastIndexOf(findThis, StringComparison.CurrentCultureIgnoreCase);
            }

            int currentPos;
            int previousPos = inWhat.Length - 1;
            int endPos = previousPos;
            string reversedPotentialMatch = string.Empty;
            StringComparison compareOption = StringComparison.CurrentCulture;

            if (options == IndexOfOptions.IgnoreCaseAndWhitespace)
            {
                compareOption = StringComparison.CurrentCultureIgnoreCase;
            }

            findThis = new string(findThis.Reverse().ToArray());

            bool startAgain = true;

            while (startAgain)
            {
                startAgain = false;

                foreach (char findChar in findThis)
                {
                    currentPos = inWhat.LastIndexOf(new string(findChar, 1), previousPos, compareOption);

                    if (currentPos == -1)
                    {
                        retVal = currentPos;
                        break;
                    }

                    if (retVal == -1)
                    {
                        retVal = currentPos;
                        endPos = currentPos - 1;
                        previousPos = currentPos;
                        reversedPotentialMatch = new string(findChar, 1);
                    }
                    else
                    {
                        string got = inWhat.Substring(currentPos, previousPos - currentPos).Trim();

                        if (got.Length == 0 && char.IsWhiteSpace(findChar))
                        {
                            // Whitespace within the findThis is a special case!
                            got = new string(findChar, 1);
                        }

                        reversedPotentialMatch += got;

                        if (findThis.IndexOf(reversedPotentialMatch, compareOption) == -1)
                        {
                            previousPos = endPos;
                            retVal = -1;
                            startAgain = true;
                            break;
                        }

                        retVal = currentPos;
                        previousPos = currentPos;
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Returns up to <paramref name="count" /> characters from the left of a string. Does NOT throw
        ///     <see cref="IndexOutOfRangeException" />!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The count of characters to return.</param>
        /// <returns>Left part of the string</returns>
        public static string Left(this string source, int count) => count > source.Length ? source : source.Substring(0, count);

        /// <summary>
        ///     Returns up to <paramref name="count" /> characters from the inner of a string. Does NOT throw
        ///     <see cref="IndexOutOfRangeException" />!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="start">The start of the "middle".</param>
        /// <param name="count">The count of characters to return.</param>
        /// <returns>Middle part of the string</returns>
        public static string Mid(this string source, int start, int count) =>
            count + start > source.Length ? source.Substring(start, source.Length - start) : source.Substring(start, count);

        /// <summary>
        ///     Pluralises word based on count
        /// </summary>
        /// <param name="item">The string to pluralise.</param>
        /// <param name="count">Number of items</param>
        /// <param name="includeCount">if set to <c>true</c> [include count].</param>
        /// <returns>
        ///     Pluralised word prefixed with count
        /// </returns>
        public static string Pluralise(this string item, int count, bool includeCount = true)
        {
            string plural = item + 's';

            if (Regex.IsMatch(item, "[^aeiou]y$", RegexOptions.IgnoreCase))
            {
                plural = item.Substring(0, item.Length - 1) + "ies";
            }

            return item.Pluralise(count, plural, includeCount);
        }

        /// <summary>
        ///     Pluralises word based on count
        /// </summary>
        /// <param name="item">The string to pluralise.</param>
        /// <param name="count">Number of items</param>
        /// <param name="plural">Plural form of word</param>
        /// <param name="includeCount">if set to <c>true</c> [include count].</param>
        /// <returns>
        ///     Pluralised word prefixed with count
        /// </returns>
        public static string Pluralise(
            this string item,
            int count,
            string plural,
            bool includeCount = true)
        {
            string pluralisedString = count == 1 ? item : plural;

            if (includeCount)
            {
                pluralisedString = count + " " + pluralisedString;
            }

            return pluralisedString;
        }

        /// <summary>
        ///     Replaces a string between two string markers with another.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startFrom">The string that marks the start of the replacement boundary.</param>
        /// <param name="endWith">The string that marks the end of the replacement boundary.</param>
        /// <param name="putThisInstead">The string to place BETWEEN the two boundaries.</param>
        /// <returns>
        ///     The replaced string
        /// </returns>
        public static string ReplaceBetween(
            this string source,
            string startFrom,
            string endWith,
            string putThisInstead)
        {
            int startPos = 0;

            return source.ReplaceBetween(ref startPos,
                startFrom,
                endWith,
                putThisInstead);
        }

        /// <summary>
        ///     Replaces a string between two string markers with another.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startPos">The start position -  Updated with the point AFTER the end marker</param>
        /// <param name="startFrom">The string that marks the start of the replacement boundary.</param>
        /// <param name="endWith">The string that marks the end of the replacement boundary.</param>
        /// <param name="putThisInstead">The string to place BETWEEN the two boundaries.</param>
        /// <returns>The replaced string</returns>
        public static string ReplaceBetween(
            this string source,
            ref int startPos,
            string startFrom,
            string endWith,
            string putThisInstead)
        {
            string retVal;

            // Extract the first part up to BUT NOT INCLUDING the startFrom marker
            string firstPart = source.Extract(ref startPos, startFrom, false);

            // Now locate the START of the end marker
            startPos = source.FindEnd(startPos, endWith);
            string lastPart = source.Substring(startPos);

            // Build up the replaced string.
            retVal = firstPart + startFrom + putThisInstead + lastPart;

            // Make sure that the startPos is updated to be the first char AFTER the endWith marker.
            startPos += endWith.Length;

            return retVal;
        }

        /// <summary>
        ///     Returns up to <paramref name="count" /> characters from the right of a string. Does NOT throw
        ///     <see cref="IndexOutOfRangeException" />!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="count">The count of characters to return.</param>
        /// <returns>Right part of the string</returns>
        public static string Right(this string source, int count) => count > source.Length ? source : source.Substring(source.Length - count, count);

        /// <summary>
        ///     Splits a string as though it were a row typically found in a CSV formatted file.
        ///     <para>
        ///         Taken from the answer by Sam Jazz @
        ///         https://stackoverflow.com/questions/3776458/split-a-comma-separated-string-with-both-quoted-and-unquoted-strings
        ///     </para>
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="delimiter">The delimiter, typically commas.</param>
        /// <param name="qualifier">
        ///     A qualifier, typically double quotes. i.e. Can be used to enclose whitespace, including the
        ///     delimiter, within the one value.
        /// </param>
        /// <param name="trimData">Should the qualifiers be trimmed?</param>
        /// <returns>The array of split strings</returns>
        public static string[] SplitRow(
            this string record,
            string delimiter = ",",
            string qualifier = "\"",
            bool trimData = false)
        {
            // In-Line for example, but I implemented as string extender in production code
            Func<string, int, int> IndexOfNextNonWhiteSpaceChar =
                delegate(string source, int startIndex)
                {
                    if (startIndex >= 0)
                    {
                        if (source != null)
                        {
                            for (int i = startIndex; i < source.Length; i++)
                            {
                                if (!char.IsWhiteSpace(source[i]))
                                {
                                    return i;
                                }
                            }
                        }
                    }

                    return -1;
                };

            List<string> results = new List<string>();
            StringBuilder result = new StringBuilder();
            bool inQualifier = false;
            bool inField = false;

            // We add new columns at the delimiter, so append one for the parser.
            string row = $"{record}{delimiter}";

            for (int idx = 0; idx < row.Length; idx++)
            {
                // A delimiter character...
                if (row[idx] == delimiter[0])
                {
                    // Are we inside qualifier? If not, we've hit the end of a column value.
                    if (!inQualifier)
                    {
                        results.Add(trimData ? result.ToString().Trim() : result.ToString());
                        result.Clear();
                        inField = false;
                    }
                    else
                    {
                        result.Append(row[idx]);
                    }
                }

                // NOT a delimiter character...
                else
                {
                    // ...Not a space character
                    if (row[idx] != ' ')
                    {
                        // A qualifier character...
                        if (row[idx] == qualifier[0])
                        {
                            // Qualifier is closing qualifier...
                            if (inQualifier && row[IndexOfNextNonWhiteSpaceChar(row, idx + 1)] == delimiter[0])
                            {
                                inQualifier = false;
                            }

                            else
                            {
                                // ...Qualifier is opening qualifier
                                if (!inQualifier)
                                {
                                    inQualifier = true;
                                }

                                // ...It's a qualifier inside a qualifier.
                                else
                                {
                                    inField = true;
                                    result.Append(row[idx]);
                                }
                            }
                        }

                        // Not a qualifier character...
                        else
                        {
                            result.Append(row[idx]);
                            inField = true;
                        }
                    }

                    // ...A space character
                    else
                    {
                        if (inQualifier || inField)
                        {
                            result.Append(row[idx]);
                        }
                    }
                }
            }

            return results.ToArray<string>();
        }

        /// <summary>
        ///     Creates a list of integers from a "Comma Separated List" string. A counterpart to CSL string extension method.
        /// </summary>
        /// <param name="csl">"this" string containing separated list</param>
        /// <param name="separator">the separator character</param>
        /// <returns>A list of integers. NOTE: If the string is null or empty you will get a list with 0 entries.</returns>
        /// <exception>Anything that <see cref="int.Parse(string)" /> will throw!</exception>
        public static List<int> ToIntList(this string csl, char separator)
        {
            List<int> retVal;

            if (string.IsNullOrEmpty(csl))
            {
                retVal = new List<int>(0);
            }
            else
            {
                string[] items = csl.Split(separator);
                retVal = new List<int>(items.Length);

                foreach (string item in items)
                {
                    retVal.Add(int.Parse(item));
                }
            }

            return retVal;
        }

        /// <summary>
        ///     "TurnsCasedText" into "Turns Cased Text".
        /// </summary>
        /// <param name="instance">The string being sentenced.</param>
        /// <returns></returns>
        public static string ToSentence(this string instance)
        {
            return new string(instance.SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new[] { ' ', c } : new[] { c }).ToArray());
        }

        /// <summary>
        ///     Creates a list of strings from a "Comma Separated List" string. A counterpart to CSL string
        ///     extension method.
        /// </summary>
        /// <param name="csl">"this" string containing separated list</param>
        /// <param name="separator">the separator character</param>
        /// <returns>A list of trimmed strings. NOTE: If the string is null or empty you will get a list with 0 entries.</returns>
        public static List<string> ToStringList(this string csl, char separator)
        {
            List<string> retVal;

            if (string.IsNullOrEmpty(csl))
            {
                retVal = new List<string>(0);
            }
            else
            {
                string[] items = csl.Split(separator);
                retVal = new List<string>(items.Length);

                foreach (string item in items)
                {
                    retVal.Add(item.Trim());
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Removes a single instance of a specified string from the start and end of a string.
        /// </summary>
        /// <param name="instance">The string being trimmed</param>
        /// <param name="what">What to trim off the start and end</param>
        /// <returns>The trimmed string</returns>
        public static string Trim(this string instance, string what) => instance.TrimStart(what).TrimEnd(what);

        /// <summary>
        ///     Removes a single instance of a specified string from the end of a string.
        /// </summary>
        /// <param name="instance">The string being trimmed</param>
        /// <param name="what">What to trim off the end</param>
        /// <returns>The trimmed string</returns>
        public static string TrimEnd(this string instance, string what)
        {
            string retVal = instance;

            if (instance.EndsWith(what))
            {
                retVal = instance.Substring(0, instance.Length - what.Length);
            }

            return retVal;
        }

        /// <summary>
        ///     Removes a single instance of a specified string from the start of a string.
        /// </summary>
        /// <param name="instance">The string being trimmed</param>
        /// <param name="what">What to trim off the start</param>
        /// <returns>The trimmed string</returns>
        public static string TrimStart(this string instance, string what)
        {
            string retVal = instance;

            if (instance.StartsWith(what))
            {
                retVal = instance.Substring(what.Length);
            }

            return retVal;
        }

        /// <summary>
        ///     Determines if the IndexOf operation found the required text or not. Saves multiple in-line found checking after
        ///     each IndexOf.
        /// </summary>
        /// Only used by the marker extraction extensions below
        /// <param name="indexOfResult">If text wasn't found (-1), throws an exception.</param>
        /// <returns>Otherwise returns the result.</returns>
        /// <exception cref="DataMisalignedException">Marker was not found</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse exceptions and returns!")]
        private static int AssertFound(int indexOfResult)
        {
            if (indexOfResult == -1)
            {
                throw new DataMisalignedException("Marker was not found");
            }

            return indexOfResult;
        }
    }
}
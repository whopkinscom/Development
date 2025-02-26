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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

[assembly: CLSCompliant(false)]

namespace Moonrise.Utils.Standard.Extensions
{
    /// <summary>
    ///     Contains extension methods for enums.
    /// </summary>
    /// <remarks>
    ///     Originally written by Will Hopkins 2010-2013
    /// </remarks>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Caches descriptions of enums - whether original or changed.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<Enum, string>> UpdatedDescriptions =
            new Dictionary<Type, Dictionary<Enum, string>>();

        /// <summary>
        ///     Enum extension method to get the description of an enum.
        ///     <para>
        ///         The description is as specified by the changed description, DescriptionAttribute, or the string value if no
        ///         description. PascalCase enum names will be sentenced to "Pascal Case".
        ///     </para>
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description of the enum value.</returns>
        public static string Description(this Enum enumValue)
        {
            string retVal = string.Empty;
            Type enumType = enumValue.GetType();
            Dictionary<Enum, string> desc;

            if (!(UpdatedDescriptions.TryGetValue(enumType, out desc) && desc.TryGetValue(enumValue, out retVal)))
            {
                retVal = OriginalDescription(enumValue);

                // OK, let's cache the Description by treating it as a modified description.
                ModifyDescription(enumValue, retVal);
            }

            return retVal;
        }

        /// <summary>
        ///     Allows an enum to be enumerated as follows;
        ///     <para>
        ///         foreach (EnumType enumLoopVar in EnumExtensions.Enumerable{EnumType}()) { ...code... }
        ///     </para>
        /// </summary>
        /// <typeparam name="T">This will always be an Enum!</typeparam>
        /// <returns>
        ///     The enumerable list of enums
        /// </returns>
        /// <exception cref="ArgumentException">Enumerable{T} must only be used on Enums</exception>
        [CLSCompliant(false)] // This is because IConvertible is non-CLSCompliant
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse exceptions and returns!")]
        public static IEnumerable<Enum> Enumerable<T>()
            where T : IConvertible
        {
            Type typeT = typeof(T);

            if (!typeT.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("Enumerable<T> must only be used on Enums");
            }

            return Enum.GetValues(typeT).Cast<Enum>();
        }

        /// <summary>
        ///     Returns an enum value from a string.
        ///     <para>
        ///         The string matches either the enum "name" or if there is a description attribute for a member then the
        ///         description.
        ///         The name takes precedence over the description - in case a description is given to a member and there is
        ///         another
        ///         member whose name is the same as that description!
        ///         Usage: EnumType eVar = EnumExtensions.FromString{EnumType}("string_representation");
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The actual enum type</typeparam>
        /// <param name="candidateValue">The candidate value.</param>
        /// <returns>The resultant enum</returns>
        [CLSCompliant(false)] // This is because IConvertible is non-CLSCompliant
        public static T FromString<T>(string candidateValue)
            where T : IConvertible =>
            FromString(candidateValue, (T)(object)0, false);

        /// <summary>
        ///     Returns an enum value from a string.
        ///     <para>
        ///         The string matches either the enum "name" or if there is a description attribute for a member then the
        ///         description.
        ///         The name takes precedence over the description - in case a description is given to a member and there is
        ///         another
        ///         member whose name is the same as that description!
        ///         Usage: EnumType eVar = EnumExtensions.FromString{EnumType}("string_representation");
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The actual enum type</typeparam>
        /// <param name="candidateValue">The candidate value.</param>
        /// <param name="unfoundValue">The value to use if the value cannot be found as an enum</param>
        /// <returns>The resultant enum</returns>
        [CLSCompliant(false)] // This is because IConvertible is non-CLSCompliant
        public static T FromString<T>(
            string candidateValue,
            T unfoundValue)
            where T : IConvertible =>
            FromString(candidateValue, unfoundValue, true);

        /// <summary>
        ///     Returns an enum value from a string.
        ///     <para>
        ///         The string matches either the enum "name" or if there is a description attribute for a member then the
        ///         description.
        ///         The name takes precedence over the description - in case a description is given to a member and there is
        ///         another
        ///         member whose name is the same as that description!
        ///         Usage: EnumType eVar = EnumExtensions.FromString{EnumType}("string_representation");
        ///     </para>
        /// </summary>
        /// <param name="candidateValue">The candidiate value</param>
        /// <param name="unfoundValue">The value to use if the value cannot be found as an enum</param>
        /// <param name="typeT">The Enum type</param>
        /// <param name="useUnfound">if set to <c>true</c> [use unfound].</param>
        /// <returns></returns>
        public static Enum FromString(
            string candidateValue,
            Enum unfoundValue,
            Type typeT,
            bool useUnfound)
        {
            bool found = false;
            object result = default(Enum);

            if (!typeT.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("FromString<T> must only be used on Enums");
            }

            try
            {
                result = Enum.Parse(typeT, candidateValue, true);
                found = true;
            }
            catch (ArgumentException)
            {
                // An argument exception means that the string was not found as an enum "value", so now we'll check if there are any DescriptionAttributes, 
                // then look for Display.Name & Display.Description
                FieldInfo[] fis = typeT.GetFields();

                foreach (FieldInfo fi in fis)
                {
                    DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute),
                        false);

                    if (attributes != null &&
                        attributes.Length > 0 &&
                        attributes[0].Description.Equals(candidateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        result = fi.GetValue(null);
                        found = true;

                        // OK value is set so we can now break.
                        break;
                    }
                }

                if (!found)
                {
                    foreach (FieldInfo fi in fis)
                    {
                        DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute),
                            false);

                        if (attributes != null && attributes.Length > 0)
                        {
                            if (attributes[0].Name.Equals(candidateValue, StringComparison.OrdinalIgnoreCase) ||
                                attributes[0].Description.Equals(candidateValue, StringComparison.OrdinalIgnoreCase))
                            {
                                result = fi.GetValue(null);
                                found = true;

                                // OK value is set so we can now break.
                                break;
                            }
                        }

                        if (!found)
                        {
                            // Ok, now check the modified descriptions
                            if (UpdatedDescriptions.ContainsKey(typeT))
                            {
                                foreach (KeyValuePair<Enum, string> descs in UpdatedDescriptions[typeT])
                                {
                                    if (descs.Value.Equals(candidateValue, StringComparison.OrdinalIgnoreCase))
                                    {
                                        result = descs.Key;
                                        found = true;

                                        // OK value is set so we can now break.
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // If we get here it means there was no attribute that matched
                if (!found)
                {
                    // Let's see if the string is an integer we can parse, if so we'll treat the enum as that value!
                    int value;

                    if (int.TryParse(candidateValue, out value))
                    {
                        result = Enum.Parse(typeT, candidateValue);
                    }
                }

                // OK, everything is exhausted, so do we rethrow the argument exception or return the unfound value?
                if (!found)
                {
                    if (useUnfound)
                    {
                        result = unfoundValue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return (Enum)result;
        }

        /// <summary>
        ///     Determines if an enum value is in a range of possible enum values.
        /// </summary>
        /// <typeparam name="T">This is the enum type</typeparam>
        /// <param name="val">The value that may or may not be in the list</param>
        /// <param name="values">
        ///     A comma separated list (i.e. a variable parameter list - NOT a comma separated string!) of the
        ///     possible enum values
        /// </param>
        /// <returns>true if in the list, or false if not!</returns>
        public static bool In<T>(
            this T val,
            params T[] values)
            where T : struct =>
            values.Contains(val);

        /// <summary>
        ///     Modifies the Description attributed to an enum value
        /// </summary>
        /// <param name="enumValue">Value who's description is to be changed</param>
        /// <param name="newDescription">New description</param>
        public static void ModifyDescription(
            this Enum enumValue,
            string newDescription)
        {
            Type enumType = enumValue.GetType();
            Dictionary<Enum, string> desc;

            if (!UpdatedDescriptions.TryGetValue(enumType, out desc))
            {
                desc = new Dictionary<Enum, string>();
                UpdatedDescriptions[enumType] = desc;
            }

            desc[enumValue] = newDescription;
        }

        /// <summary>
        ///     Enum extension method to get the original, attributed, description of an enum.
        ///     <para>
        ///         The description is as specified - in this order - by the DescriptionAttribute, or the DisplayAttribute.Name,
        ///         or the DisplayAttribute.Description, or the string value if no description.
        ///     </para>
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description of the enum value.</returns>
        public static string OriginalDescription(this Enum enumValue)
        {
            string retVal = string.Empty;
            Type enumType = enumValue.GetType();

            FieldInfo fi = enumType.GetField(enumValue.ToString());

            if (fi != null)
            {
                DescriptionAttribute[] descriptionAttributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (descriptionAttributes != null && descriptionAttributes.Length > 0)
                {
                    retVal = descriptionAttributes[0].Description;
                }
                else
                {
                    DisplayAttribute[] displayAttributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

                    if (displayAttributes != null && displayAttributes.Length > 0)
                    {
                        retVal = displayAttributes[0].Description;

                        if (string.IsNullOrEmpty(retVal))
                        {
                            retVal = displayAttributes[0].Name;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(retVal))
            {
                // Automatically apply spaces where there is mixed case going on within the enum name.
                retVal = enumValue.ToString().ToSentence();
            }

            return retVal;
        }

        /// <summary>
        ///     Converts an Enum type value to integer
        /// </summary>
        /// <param name="enumValue">Enum value to convert</param>
        /// <returns>Integer value</returns>
        public static int ToInt(this Enum enumValue)
        {
            int retVal = Convert.ToInt32(enumValue);
            return retVal;
        }

        /// <summary>
        ///     Private version to simply deal better with supplying unfound values since a default parameter value for a generic
        ///     enum is not possible to set to a known unique value to indicate that the parameter wasn't passed! (I tried
        ///     <see cref="Int32.MinValue" /> for example - no dice).
        /// </summary>
        /// <typeparam name="T">Will always be an enum</typeparam>
        /// <param name="candidateValue">The candidate value.</param>
        /// <param name="unfoundValue">The unfound value.</param>
        /// <param name="useUnfound">if set to <c>true</c> [use unfound].</param>
        /// <returns>The enum converted from the string</returns>
        /// <exception cref="ArgumentException">FromString{T} must only be used on Enums</exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse exceptions and returns!")]
        private static T FromString<T>(
            string candidateValue,
            T unfoundValue,
            bool useUnfound)
            where T : IConvertible
        {
            Type typeT = typeof(T);

            Enum result = FromString(candidateValue,
                (Enum)(object)unfoundValue,
                typeT,
                useUnfound);

            return (T)(object)result;
        }
    }
}
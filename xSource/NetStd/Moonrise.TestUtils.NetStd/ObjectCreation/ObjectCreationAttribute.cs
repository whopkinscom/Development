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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Moonrise.Utils.Test.ObjectCreation
{
    /// <summary>
    ///     Creation attributes to apply to the attributed property that override the <see cref="Creator" /> properties.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ObjectCreationAttribute : Attribute
    {
        /// <summary>
        ///     This "static" interface (which you can't have) is to illustrate the expected static method your ItemSource static
        ///     class should expose in order to act as in ItemSource OR it must expose the element name as a property !
        /// </summary>
        /// <typeparam name="T">
        ///     Just to illustrate that each of the <see cref="ElementName" /> properties need to return the
        ///     appropriate type for that element.
        /// </typeparam>
        [SuppressMessage("StyleCop.CSharp.NamingRules",
            "SA1302:InterfaceNamesMustBeginWithI",
            Justification = "This is to illustrate a STATIC interface (which can't exist)!")]
#pragma warning disable IDE1006 // Naming Styles
        public /*static*/ interface SIItemSource<T>
#pragma warning restore IDE1006 // Naming Styles
        {
            /// <summary>
            ///     Gets the list of objects to be used as the item source for <see cref="ElementName" />. IF the ItemsSource attribute
            ///     DOES supply a static <see cref="ItemSource" /> method then
            ///     it will be called in preference to the property, unless your <see cref="SIItemSource{T}" /> implements the static
            ///     property <see cref="PreferPropertyToItemSourceCall" />, in which case
            ///     it will use that to decide whether to use the property OR the static <see cref="ItemSource" /> method to supply
            ///     the source list.
            /// </summary>
            /*static*/
            IList<T> ElementName { get; }

            /// <summary>
            ///     Returns the list of objects to be used as the item source for elementName
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <param name="propInfo">The property information - Only if it's a property, null otherwise.</param>
            /// <param name="fieldInfo">The field information - Only if it's a field, null otherwise.</param>
            /// <param name="creator">The instance to use to help you create a random instance, if you need to.</param>
            /// <returns>
            ///     List of objects, to be used to randomly choose from to fill the property or field that the attribute has been
            ///     applied
            ///     to
            /// </returns>
            /*static*/
            IList ItemSource(string elementName, PropertyInfo propInfo, FieldInfo fieldInfo, Creator creator);

            /// <summary>
            ///     Determines if the <see cref="ElementName" /> or the <see cref="ItemSource" /> is to be used as the source for
            ///     elements for the elementName />.
            /// </summary>
            /// <param name="elementName">Name of the element.</param>
            /// <returns>
            ///     True means use the static <see cref="ElementName" /> property where it exists, False means use the static
            ///     <see cref="ItemSource" /> method.
            /// </returns>
            /*static*/ bool PreferPropertyToItemSourceCall(string elementName);
        }

        /// <summary>
        ///     This "static" interface (which you can't have) is to illustrate the expected static method your TypeCreation static
        ///     class should expose in order to act as a TypeCreation OR it must expose the element name as a property !
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the random instance this class will create.
        /// </typeparam>
        [SuppressMessage("StyleCop.CSharp.NamingRules",
            "SA1302:InterfaceNamesMustBeginWithI",
            Justification = "This is to illustrate a STATIC interface (which can't exist)!")]
#pragma warning disable IDE1006 // Naming Styles
        public /*static*/ interface SITypeCreation<T>
#pragma warning restore IDE1006 // Naming Styles
        {
            /// <summary>
            ///     Creates an instance of the generic type using the supplied <see cref="Random" />.
            /// </summary>
            /// <param name="creator">The instance to use to help you create a random instance.</param>
            /// <returns>A "random" instance</returns>
            /*static*/
            T CreateInstance(Creator creator);
        }

        /// <summary>
        ///     Determines if nulls are allowed to be inserted into enumerable types
        /// </summary>
        public bool AllowNullElementsInEnumerable { get; set; }

        /// <summary>
        ///     Determines if nulls are allowed for objects and strings
        /// </summary>
        public bool AllowNulls { get; set; }

        /// <summary>
        ///     Determines if an element will be filled by the creator or ignored
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        ///     The implementation to use for an interface
        /// </summary>
        public Type Implementation { get; set; }

        /// <summary>
        ///     The source to use for items to populate the property. The source HAS to be a static AND it needs to "implement",
        ///     i.e. respect, <see cref="SIItemSource{T}" />
        ///     Use of this attribute will ALWAYS override any <see cref="RespectValidation" />
        /// </summary>
        public Type ItemsSource { get; set; }

        /// <summary>
        ///     The maximum byte value to use in following creation/random calls
        /// </summary>
        public byte MaxByte { get; set; }

        /// <summary>
        ///     The maximum char value to use in following creation/random calls
        /// </summary>
        public char MaxChar { get; set; }

        /// <summary>
        ///     The maximum date time to use in following creation/random calls
        /// </summary>
        public DateTime MaxDateTime { get; set; }

        /// <summary>
        ///     The maximum date time offset to use in following creation/random calls
        /// </summary>
        public DateTimeOffset MaxDateTimeOffset { get; set; }

        /// <summary>
        ///     The maximum "decimal" value to use in following creation/random calls
        /// </summary>
        public double MaxDecimal { get; set; }

        /// <summary>
        ///     The maximum double value to use in following creation/random calls
        /// </summary>
        public double MaxDouble { get; set; }

        /// <summary>
        ///     The maximum float value to use in following creation/random calls
        /// </summary>
        public float MaxFloat { get; set; }

        /// <summary>
        ///     The maximum integer value to use in following creation/random calls
        /// </summary>
        public int MaxInt { get; set; }

        /// <summary>
        ///     Maximum number of items to add to an IEnumerable member
        /// </summary>
        public int MaxItems { get; set; }

        /// <summary>
        ///     The maximum sbyte value to use in following creation/random calls
        /// </summary>
        public sbyte MaxSByte { get; set; }

        /// <summary>
        ///     The maximum short value to use in following creation/random calls
        /// </summary>
        public short MaxShort { get; set; }

        /// <summary>
        ///     The maximum string length to use in following creation/random calls
        /// </summary>
        public int MaxStrLen { get; set; }

        /// <summary>
        ///     The maximum uint value to use in following creation/random calls
        /// </summary>
        public uint MaxUInt { get; set; }

        /// <summary>
        ///     The maximum ulong value to use in following creation/random calls
        /// </summary>
        public ulong MaxULong { get; set; }

        /// <summary>
        ///     The maximum ushort value to use in following creation/random calls
        /// </summary>
        public ushort MaxUShort { get; set; }

        /// <summary>
        ///     The minimum byte value to use in following creation/random calls
        /// </summary>
        public byte MinByte { get; set; }

        /// <summary>
        ///     The minimum char value to use in following creation/random calls
        /// </summary>
        public char MinChar { get; set; }

        /// <summary>
        ///     The minimum date time to use in following creation/random calls
        /// </summary>
        public DateTime MinDateTime { get; set; }

        /// <summary>
        ///     The minimum date time offset to use in following creation/random calls
        /// </summary>
        public DateTimeOffset MinDateTimeOffset { get; set; }

        /// <summary>
        ///     The minimum "decimal" value to use in following creation/random calls
        /// </summary>
        public double MinDecimal { get; set; }

        /// <summary>
        ///     The minimum double value to use in following creation/random calls
        /// </summary>
        public double MinDouble { get; set; }

        /// <summary>
        ///     The minimum float value to use in following creation/random calls
        /// </summary>
        public float MinFloat { get; set; }

        /// <summary>
        ///     The minimum integer value to use in following creation/random calls
        /// </summary>
        public int MinInt { get; set; }

        /// <summary>
        ///     Minimum number of items to add to an IEnumerable member
        /// </summary>
        public int MinItems { get; set; }

        /// <summary>
        ///     The minimum sbyte value to use in following creation/random calls
        /// </summary>
        public sbyte MinSByte { get; set; }

        /// <summary>
        ///     The minimum short value to use in following creation/random calls
        /// </summary>
        public short MinShort { get; set; }

        /// <summary>
        ///     The minimum string length to use in following creation/random calls
        /// </summary>
        public int MinStrLen { get; set; }

        /// <summary>
        ///     The minimum uint value to use in following creation/random calls
        /// </summary>
        public uint MinUInt { get; set; }

        /// <summary>
        ///     The minimum ulong value to use in following creation/random calls
        /// </summary>
        public ulong MinULong { get; set; }

        /// <summary>
        ///     The minimum ushort value to use in following creation/random calls
        /// </summary>
        public ushort MinUShort { get; set; }

        /// <summary>
        ///     The threshold for creating nulls. 0..1. Larger value = more nulls!
        /// </summary>
        public double NullThreshold { get; set; }

        /// <summary>
        ///     Determines if the filling should respect any validation attributes on the elements being filled and only fill with
        ///     data that meets the validation constraints.
        /// </summary>
        public bool RespectValidation { get; set; }

        /// <summary>
        ///     The source to use for characters with which to create random strings
        /// </summary>
        public string StringSource { get; set; }

        /// <summary>
        ///     The random creator to use to create an instance not supported by Creator. The creator HAS to be a static AND it
        ///     needs to "implement",
        ///     i.e. respect, <see cref="SITypeCreation{T}" />
        /// </summary>
        public Type TypeCreation { get; set; }
    }
}

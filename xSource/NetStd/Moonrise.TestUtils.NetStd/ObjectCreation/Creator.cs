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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Moonrise.Utils.Standard.Validation;

namespace Moonrise.Utils.Test.ObjectCreation
{
    /// <summary>
    ///     Creates objects filled with data and can also create random data of different types.
    /// </summary>
    /// NOTE: All the properties that are ...OneShot are properties that are set by an element having an
    /// <see cref="ObjectCreationAttribute" />
    /// . The pattern is that the
    /// <see cref="ObjectCreationAttribute" />
    /// property
    /// applied results in the associated [property]OneShot being set to the value of the attribute. This then controls
    /// whether and how that value is created.
    public class Creator
    {
        /// <summary>
        ///     Namespace for the available string sources
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "The intellisense will be more effective if the summaries show the string source!")]
        [SuppressMessage("StyleCop.CSharp.OrderingRules",
            "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification = "I like nested types to be first!")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
            "SA1630:DocumentationTextMustContainWhitespace",
            Justification = "The intellisense will be more effective if the summaries show the string source!")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
            "SA1631:DocumentationMustMeetCharacterPercentage",
            Justification = "The intellisense will be more effective if the summaries show the string source!")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
            "SA1603:DocumentationMustContainValidXml",
            Justification = "The intellisense will be more effective if the summaries show the string source!")]
#pragma warning disable CS1570 
        public class StringSources
        {
            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
            /// </summary>
            public const string AlphaNumericCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 "
            /// </summary>
            public const string AlphaNumericCharactersWithSpaces = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

            /// <summary>
            ///     ""
            /// </summary>
            public const string Empty = "";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ.0123456789:;!\"£$%^&amp;*()-_+={}[]'@#~/?\\€&lt;&gt;"
            /// </summary>
            public const string EverydayCharacters =
                "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ.0123456789:;!\"£$%^&*()-_+={}[]'@#~/?\\€<>";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyz"
            /// </summary>
            public const string LowercaseAlphaCharacters = "abcdefghijklmnopqrstuvwxyz";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyz "
            /// </summary>
            public const string LowercaseAlphaCharactersWithSpaces = "abcdefghijklmnopqrstuvwxyz ";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyz0123456789"
            /// </summary>
            public const string LowercaseAlphaNumericCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

            /// <summary>
            ///     "abcdefghijklmnopqrstuvwxyz0123456789 "
            /// </summary>
            public const string LowercaseAlphaNumericCharactersWithSpaces = "abcdefghijklmnopqrstuvwxyz0123456789 ";

            /// <summary>
            ///     "0123456789"
            /// </summary>
            public const string Numeric = "0123456789";

            /// <summary>
            ///     "0123456789."
            /// </summary>
            public const string NumericWithDecimal = "0123456789.";

            /// <summary>
            ///     "0123456789.+-*/^&lt;&gt;=%"
            /// </summary>
            public const string NumericWithSymbols = "0123456789.+-*/^<>=%";

            /// <summary>
            ///     ".:;!\"£$%^&*()-_+={}[]'@#~/?\\€<>"
            /// </summary>
            public const string OnlySymbols = ".:;!\"£$%^&*()-_+={}[]'@#~/?\\€<>";

            /// <summary>
            ///     "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            /// </summary>
            public const string UppercaseAlphaCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            /// <summary>
            ///     "ABCDEFGHIJKLMNOPQRSTUVWXYZ "
            /// </summary>
            public const string UppercaseAlphaCharactersWithSpaces = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";

            /// <summary>
            ///     "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
            /// </summary>
            public const string UppercaseAlphaNumericCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            /// <summary>
            ///     "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 "
            /// </summary>
            public const string UppercaseAlphaNumericCharactersWithSpaces = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
        }
#pragma warning restore CS1570

        /// <summary>
        ///     Constant representing the default datetime
        /// </summary>
        private static readonly DateTime DEFAULT_DATETIME = default(DateTime);

        /// <summary>
        ///     Constant representing the default datetime offset
        /// </summary>
        private static readonly DateTimeOffset DEFAULT_DATETIMEOFFSET = default(DateTimeOffset);

        /// <summary>
        ///     A map of types and the creators that can create them.
        /// </summary>
        private readonly Dictionary<Type, MethodInfo> _creatorMap = new Dictionary<Type, MethodInfo>();

        /// <summary>
        ///     Maps implementation types to interfaces that carry <see cref="ObjectCreationAttribute" />s
        /// </summary>
        private readonly Dictionary<Type, Type> _interfaceAttributesMap = new Dictionary<Type, Type>();

        /// <summary>
        ///     Maps interfaces to implementations to be used when creating elements that are interfaces
        /// </summary>
        private readonly Dictionary<Type, Type> _interfaceMap = new Dictionary<Type, Type>();

        /// <summary>
        /// A list of object types that have been encountered in a particular context. Used to avoid nesting certain objects. 
        /// </summary>
        protected List<int> encounteredObjects = new List<int>();

        /// <summary>
        ///     Backing field for MaxObjects
        /// </summary>
        private int _maxObjects = 100000;

        /// <summary>
        ///     Backing field for <see cref="OneShot" />
        /// </summary>
        private bool _oneShot;

        /// <summary>
        ///     The random that will be used by the instance to generate random stuff
        /// </summary>
        private Random _random;

        private List<Type> _ignoreTypes = new List<Type>();
        private List<string> _elementContext = new List<string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Creator" /> class. Elements created via this constructor will be
        ///     non-reliably-repeatable
        /// </summary>
        public Creator()
        {
            _random = new Random();
            Initialise();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Creator" /> class. Elements created via this constructor will be
        ///     reliably-repeatable
        /// </summary>
        /// <param name="seed">
        ///     The seed to pass to the random generator. Using the same seed and calling in the same creation order
        ///     will produce repeatable results!
        /// </param>
        public Creator(int seed)
        {
            _random = new Random(seed);
            Initialise();
        }

        /// <summary>
        ///     Determines if nulls are allowed to be inserted into enumerable types - Defaults to false
        /// </summary>
        public bool AllowNullElementsInEnumerable { get; set; }

        /// <summary>
        ///     Determines if nulls are allowed for objects and strings - Defaults to false
        /// </summary>
        public bool AllowNulls { get; set; }

        /// <summary>
        ///     The action to pass the name of the property or field being created - VERY useful for diagnosing recursive creation failures!
        /// </summary>
        public Action<string> CreationTracer { get; set; }

        /// <summary>
        ///     The action to pass the string of a problem that has been set to ignore - VERY useful for diagnosing expected creations with null elements!
        /// </summary>
        public Action<string> IgnoredIssueTracer { get; set; }

        /// <summary>
        ///     Any class requiring a non-default constructor will be ignored and returned as null - defaults to true.
        /// </summary>
        public bool IgnoreNonDefaultConstructors { get; set; }

        /// <summary>
        ///     Detects and ignores recursion - defaults to true.
        /// </summary>
        public bool IgnoreRecursion { get; set; }

        /// <summary>
        ///     Ignores exceptions that occur when a value is assigned to a property or field, leaving it as null - defaults to true.
        /// </summary>
        public bool IgnoreSetterExceptions { get; set; }

        /// <summary>
        ///     If an interface does not have a defined implemented indicated via <see cref="MapInterface" /> or
        ///     <see cref="ObjectCreationAttribute.Implementation" /> then setting this to true will allow that element to be null,
        ///     despite the setting of <see cref="AllowNulls" />  - defaults to true.
        /// </summary>
        public bool IgnoreUnimplementedInterfaces { get; set; }

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
        ///     The maximum decimal value to use in following creation/random calls
        /// </summary>
        public decimal MaxDecimal { get; set; }

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
        ///     Maximum number of items to add to an IEnumerable member. Defaults to 4.
        /// </summary>
        public int MaxItems { get; set; }

        /// <summary>
        ///     The maximum long value to use in following creation/random calls
        /// </summary>
        public long MaxLong { get; set; }

        /// <summary>
        ///     The maximum number of objects that will be created in one hit. Defaults to 100,000. This WILL overrule all other
        ///     properties so you MAY get null objects despite properties specified elsewhere! You cannot set this value higher
        ///     than
        ///     <see cref="AbsoluteMaximumObjects" />.
        /// </summary>
        /// <exception cref="System.ArgumentException">If you try to set MaxObjects too high, I won't let you!</exception>
        public int MaxObjects
        {
            get
            {
                return _maxObjects;
            }

            set
            {
                if (value > AbsoluteMaximumObjects())
                {
                    throw new ArgumentException(
                        string.Format(
                            "The absolute maximum objects is hard set to {0}. The only way to override this value is to literally override the AbsoluteMaximumObjects() method in a descendant class, i.e. REALLY mean it!",
                            AbsoluteMaximumObjects()));
                }

                _maxObjects = value;
            }
        }

        /// <summary>
        ///     The maximum sbyte value to use in following creation/random calls
        /// </summary>
        public sbyte MaxSByte { get; set; }

        /// <summary>
        ///     The maximum short value to use in following creation/random calls
        /// </summary>
        public short MaxShort { get; set; }

        /// <summary>
        ///     The maximum string length to use in following creation/random calls. Defauts to 32.
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
        ///     The minimum decimal value to use in following creation/random calls
        /// </summary>
        public decimal MinDecimal { get; set; }

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
        ///     Minimum number of items to add to an IEnumerable member. Defaults to 0.
        /// </summary>
        public int MinItems { get; set; }

        /// <summary>
        ///     The minimum long value to use in following creation/random calls
        /// </summary>
        public long MinLong { get; set; }

        /// <summary>
        ///     The minimum sbyte value to use in following creation/random calls
        /// </summary>
        public sbyte MinSByte { get; set; }

        /// <summary>
        ///     The minimum short value to use in following creation/random calls
        /// </summary>
        public short MinShort { get; set; }

        /// <summary>
        ///     The minimum string length to use in following creation/random calls. Defaults to 0.
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
        ///     The threshold for creating nulls. 0..1. Larger value = more nulls! 0 = None, 1 = All. Defaults to 0.1.
        /// </summary>
        public double NullThreshold { get; set; }

        /// <summary>
        ///     The number of objects that were created by the last <see cref="CreateFilled{T}" /> call.
        /// </summary>
        public int ObjectCount { get; protected set; }

        /// <summary>
        ///     Determines if the filling should respect any validation attributes on the elements being filled and only fill with
        ///     data that meets the validation constraints. At the moment, only the following are respected;
        ///     <para>
        ///         <see cref="RequiredAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="StringLengthAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="RangeAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="MinLengthAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="MaxLengthAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="ListContentValidationAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="GuidValidationAttribute" />
        ///     </para>
        ///     <para>
        ///         <see cref="FileValidationAttribute" />
        ///     </para>
        ///     NOTE: Validation requirements will overule any <see cref="ObjectCreationAttribute" />s that cover the same
        ///     territory!
        /// </summary>
        public bool RespectValidation { get; set; }

        /// <summary>
        ///     The source to use for characters with which to create random strings. See also
        ///     <seealso cref="Creator.StringSources" /> Defaults to <see cref="StringSources.EverydayCharacters" />.
        /// </summary>
        public string StringSource { get; set; }

        /// <summary>
        ///     Determines if we need to use the xxxOneShot properties or not - Will always get reset after each property or field
        ///     setting.
        /// </summary>
        protected bool OneShot
        {
            get
            {
                return _oneShot && !SkipOneShots;
            }

            private set
            {
                _oneShot = value;

                if (!_oneShot)
                {
                    ValidationAttributes.Clear();
                }
            }
        }

        /// <summary>
        ///     Indicates if OneShot is to be skipped.
        /// </summary>
        protected bool SkipOneShots { get; set; }

        /// <summary>
        ///     Determines if nulls are allowed to be inserted into enumerable types - Defaults to false
        /// </summary>
        private bool AllowNullElementsInEnumerableOneShot { get; set; }

        /// <summary>
        ///     Determines if nulls are allowed for objects and strings - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private bool AllowNullsOneShot { get; set; }

        /// <summary>
        ///     The current field information
        /// </summary>
        private FieldInfo CurrentFieldInfo { get; set; }

        /// <summary>
        ///     The current property information
        /// </summary>
        private PropertyInfo CurrentPropertyInfo { get; set; }

        /// <summary>
        ///     Determines if an element will be filled by the creator or ignored - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private bool IgnoreOneShot { get; set; }

        /// <summary>
        ///     The implementation to use on an interface element - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private Type ImplementationOneShot { get; set; }

        /// <summary>
        ///     The source to use for items to populate the property. The source HAS to be a static AND it needs to "implement",
        ///     i.e. respect, <see cref="ObjectCreationAttribute.SIItemSource{T}" />
        /// </summary>
        private Type ItemsSourceOneShot { get; set; }

        /// <summary>
        ///     The name of the property/field being set for the ItemsSource - passed to the
        ///     <see cref="ObjectCreationAttribute.SIItemSource{T}" /> "static
        ///     interface"
        /// </summary>
        private string ItemsSourceOneShotName { get; set; }

        /// <summary>
        ///     The maximum byte value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private byte MaxByteOneShot { get; set; }

        /// <summary>
        ///     The maximum char value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private char MaxCharOneShot { get; set; }

        /// <summary>
        ///     The maximum date time offset to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private DateTimeOffset MaxDateTimeOffsetOneShot { get; set; }

        /// <summary>
        ///     The maximum date time to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private DateTime MaxDateTimeOneShot { get; set; }

        /// <summary>
        ///     The maximum decimal value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private double MaxDecimalOneShot { get; set; }

        /// <summary>
        ///     The maximum double value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private double MaxDoubleOneShot { get; set; }

        /// <summary>
        ///     The maximum float value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private float MaxFloatOneShot { get; set; }

        /// <summary>
        ///     The maximum integer value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MaxIntOneShot { get; set; }

        /// <summary>
        ///     Maximum number of items to add to an IEnumerable member - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MaxItemsOneShot { get; set; }

        /// <summary>
        ///     The maximum long value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private long MaxLongOneShot { get; set; }

        /// <summary>
        ///     The maximum sbyte value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private sbyte MaxSByteOneShot { get; set; }

        /// <summary>
        ///     The maximum short value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private short MaxShortOneShot { get; set; }

        /// <summary>
        ///     The maximum string length to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MaxStrLenOneShot { get; set; }

        /// <summary>
        ///     The maximum uint value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private uint MaxUIntOneShot { get; set; }

        /// <summary>
        ///     The maximum ulong value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private ulong MaxULongOneShot { get; set; }

        /// <summary>
        ///     The maximum ushort value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private ushort MaxUShortOneShot { get; set; }

        /// <summary>
        ///     The minimum byte value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private byte MinByteOneShot { get; set; }

        /// <summary>
        ///     The minimum char value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private char MinCharOneShot { get; set; }

        /// <summary>
        ///     The minimum date time offset to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private DateTimeOffset MinDateTimeOffsetOneShot { get; set; }

        /// <summary>
        ///     The minimum date time to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private DateTime MinDateTimeOneShot { get; set; }

        /// <summary>
        ///     The minimum decimal value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private double MinDecimalOneShot { get; set; }

        /// <summary>
        ///     The minimum double value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private double MinDoubleOneShot { get; set; }

        /// <summary>
        ///     The minimum float value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private float MinFloatOneShot { get; set; }

        /// <summary>
        ///     The minimum integer value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MinIntOneShot { get; set; }

        /// <summary>
        ///     Minimum number of items to add to an IEnumerable member - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MinItemsOneShot { get; set; }

        /// <summary>
        ///     The minimum long value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private long MinLongOneShot { get; set; }

        /// <summary>
        ///     The minimum sbyte value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private sbyte MinSByteOneShot { get; set; }

        /// <summary>
        ///     The minimum short value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private short MinShortOneShot { get; set; }

        /// <summary>
        ///     The minimum string length to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private int MinStrLenOneShot { get; set; }

        /// <summary>
        ///     The minimum uint value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private uint MinUIntOneShot { get; set; }

        /// <summary>
        ///     The minimum ulong value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private ulong MinULongOneShot { get; set; }

        /// <summary>
        ///     The minimum ushort value to use in following creation/random calls - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private ushort MinUShortOneShot { get; set; }

        /// <summary>
        ///     The threshold for creating nulls. 0..1. Larger value = more nulls! - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private double NullThresholdOneShot { get; set; }

        /// <summary>
        ///     Determines if the filling should respect any validation attributes on the elements being filled and only fill with
        ///     data that meets the validation constraints  - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private bool RespectValidationOneShot { get; set; }

        /// <summary>
        ///     The source to use for characters with which to create random strings - One shot usage as determined by
        ///     <see cref="ObjectCreationAttribute" />.
        /// </summary>
        private string StringSourceOneShot { get; set; }

        /// <summary>
        ///     The random creator to use to create an instance not supported by Creator. The creator HAS to be a static AND it
        ///     needs to "implement",
        ///     i.e. respect, <see cref="ObjectCreationAttribute.SITypeCreation{T}" />
        /// </summary>
        private Type TypeCreationOneShot { get; set; }

        /// <summary>
        ///     The validation attributes to respect - only ever filled if we need to respect the validation attributes
        /// </summary>
        private List<ValidationAttribute> ValidationAttributes { get; set; }

        /// <summary>
        /// Gives the context of the property/field being currently created. i.e. The "breadcrumb" of the parental elements.
        /// </summary>
        public string ElementContext
        {
            get
            {
                return string.Join(".", _elementContext);
            }
        }

        /// <summary>
        ///     Creates an object of type T whose public fields and properties are filled with random(ish) values.
        /// </summary>
        /// <typeparam name="T">The type to fill.</typeparam>
        /// <param name="seed">
        ///     The seed to apply for this creation. If left at default of <see cref="int.MinValue" /> then the
        ///     random generator will not be re-seeded
        /// </param>
        /// <param name="recursive">Determines if nested classes will be filled as well. Usually want this.</param>
        /// <returns>
        ///     An object populated with guff
        /// </returns>
        public T CreateFilled<T>(int seed = int.MinValue, bool recursive = true)
        {
            if (seed != int.MinValue)
            {
                _random = new Random(seed);
            }

            InitialiseCreation();

            // Create a filled structure.
            T retval = (T)CreateFilled(typeof(T), recursive);

            // We need to reset any OneShot values in case straight GetRandomXXXX calls are made directly by anything
            InitialiseCreation();
            return retval;
        }

        /// <summary>
        ///     Creates an object of type T whose public fields and properties are filled with random(ish) values but can use
        ///     interface I to control the creation.
        /// </summary>
        /// <typeparam name="T">The type to fill.</typeparam>
        /// <typeparam name="I">
        ///     An interface that can be used to control object creation via <see cref="ObjectCreationAttribute" />
        ///     . The interface effectively implements the class, i.e. same property/field names are found in order to get the
        ///     attributes applied, if any.
        /// </typeparam>
        /// <param name="seed">
        ///     The seed to apply for this creation. If left at default of <see cref="int.MinValue" /> then the
        ///     random generator will not be re-seeded
        /// </param>
        /// <param name="recursive">Determines if nested classes will be filled as well. Usually want this.</param>
        /// <returns>
        ///     An object populated with guff
        /// </returns>
        public T CreateFilled<T, I>(int seed = int.MinValue, bool recursive = true)
        {
            if (seed != int.MinValue)
            {
                _random = new Random(seed);
            }

            InitialiseCreation();

            MapInterfaceAttributes(typeof(T), typeof(I));
            T retval = (T)CreateFilled(typeof(T), recursive);

            // We need to reset any OneShot values in case straight GetRandomXXXX calls are made directly by anything
            InitialiseCreation();
            return retval;
        }

        /// <summary>
        ///     Creates an object of type T whose public fields and properties are filled with random(ish) values but can use
        ///     interface I to control the creation.
        /// </summary>
        /// <param name="instanceType">The type to fill.</param>
        /// <param name="interfaceType">
        ///     An interface that can be used to control object creation via <see cref="ObjectCreationAttribute" />
        ///     . The interface effectively implements the class, i.e. same property/field names are found in order to get the
        ///     attributes applied, if any.
        /// </param>
        /// <param name="seed">
        ///     The seed to apply for this creation. If left at default of <see cref="int.MinValue" /> then the
        ///     random generator will not be re-seeded
        /// </param>
        /// <returns>
        ///     An object populated with guff
        /// </returns>
        public object CreateFilled(Type instanceType, Type interfaceType = null, int seed = int.MinValue)
        {
            if (seed != int.MinValue)
            {
                _random = new Random(seed);
            }

            InitialiseCreation();

            if (interfaceType != null)
            {
                MapInterfaceAttributes(instanceType, interfaceType);
            }

            object retval = CreateFilled(instanceType, true);

            // We need to reset any OneShot values in case straight GetRandomXXXX calls are made directly by anything
            InitialiseCreation();
            return retval;
        }

        /// <summary>
        ///     Gets a random boolean.
        /// </summary>
        /// <param name="threshold">The threshold above which to return true - set lower for more trues.</param>
        /// <returns>
        ///     true or false - guess which one!
        /// </returns>
        public bool GetRandomBool(double threshold = 0.5)
        {
            return _random.NextDouble() > threshold;
        }

        /// <summary>
        ///     Gets a random int in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinByte" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxByte" />)</param>
        /// <returns>A random byte value</returns>
        public byte GetRandomByte(int min = -1, int max = 256)
        {
            byte retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == -1)
                {
                    min = OneShot ? MinByteOneShot : MinByte;
                }

                if (max == 256)
                {
                    max = OneShot ? MaxByteOneShot : MaxByte;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, (int)(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, (int)(((RangeAttribute)validationAttribute).Maximum ?? max));

                            // As it's a byte, we can never go outside 0-255!
                            min = Math.Max(min, 0);
                            max = Math.Min(max, 255);
                        }
                    }
                }

                retVal = (byte)_random.Next(min, max);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random char in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinChar" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxChar" />)</param>
        /// <returns>A random char value</returns>
        public char GetRandomChar(ushort min = ushort.MaxValue, ushort max = ushort.MaxValue)
        {
            char retVal = char.MinValue;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if ((min == ushort.MaxValue) && (max == ushort.MinValue))
                {
                    // OK, this means we use the current string source
                    retVal = GetRandomString(null, 1, 1)[0];
                }
                else
                {
                    retVal = Convert.ToChar(GetRandomUShort(min, max));
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random date time.
        /// </summary>
        /// <param name="min">The minimum date time to use - overrides <see cref="MinDateTime" /> if used.</param>
        /// <param name="max">The maximum date time to use - overrides <see cref="MaxDateTime" /> if used.</param>
        /// <returns>A Date Time with random date AND time within the specified range</returns>
        public DateTime GetRandomDateTime(DateTime min = default(DateTime), DateTime max = default(DateTime))
        {
            DateTime retVal = default(DateTime);

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == DEFAULT_DATETIME)
                {
                    min = OneShot ? MinDateTimeOneShot : MinDateTime;
                }

                if (max == DEFAULT_DATETIME)
                {
                    max = OneShot ? MaxDateTimeOneShot : MaxDateTime;
                }

                // Work out the range of seconds we can play with between min & max
                double range = (max - min).TotalSeconds;

                // Add the random number of seconds to the minimum
                retVal = min.AddSeconds(GetRandomDouble(0D, range));
            }

            // It's a date!
            return retVal;
        }

        /// <summary>
        ///     Gets a random date time.
        /// </summary>
        /// <param name="min">The minimum date time to use - overrides <see cref="MinDateTime" /> if used.</param>
        /// <param name="max">The maximum date time to use - overrides <see cref="MaxDateTime" /> if used.</param>
        /// <returns>A Date Time with random date AND time within the specified range</returns>
        public DateTimeOffset GetRandomDateTimeOffset(DateTimeOffset min = default(DateTimeOffset), DateTimeOffset max = default(DateTimeOffset))
        {
            DateTimeOffset retVal = default(DateTimeOffset);

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == DEFAULT_DATETIMEOFFSET)
                {
                    min = OneShot ? MinDateTimeOffsetOneShot : MinDateTimeOffset;
                }

                if (max == DEFAULT_DATETIMEOFFSET)
                {
                    max = OneShot ? MaxDateTimeOffsetOneShot : MaxDateTimeOffset;
                }

                // Work out the range of seconds we can play with between min & max
                double range = (max - min).TotalSeconds;

                // Add the random number of seconds to the minimum
                retVal = min.AddSeconds(GetRandomDouble(0D, range));
            }

            // It's a date!
            return retVal;
        }

        /// <summary>
        ///     Gets a random decimal in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="double.MinValue" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="double.MaxValue" />)</param>
        /// <returns>A random decimal value</returns>
        [SuppressMessage("ReSharper",
            "CompareOfFloatsByEqualityOperator",
            Justification =
                "Comparisons against Min/Max as 'flags' are valid!")]
        public decimal GetRandomDecimal(decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
        {
            decimal retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == decimal.MinValue)
                {
                    min = OneShot ? (decimal)MinDecimalOneShot : MinDecimal;
                }

                if (max == decimal.MaxValue)
                {
                    max = OneShot ? (decimal)MaxDecimalOneShot : MaxDecimal;
                }

                double minDub = (double)min;
                double maxDub = (double)max;

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            minDub = Math.Max(minDub, (double)(((RangeAttribute)validationAttribute).Minimum ?? minDub));
                            maxDub = Math.Min(maxDub, (double)(((RangeAttribute)validationAttribute).Maximum ?? maxDub));
                        }
                    }
                }

                retVal = Convert.ToDecimal(GetRandomDouble(minDub, maxDub));
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random double in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinDouble" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxDouble" />)</param>
        /// <returns>A random double value</returns>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Comparisons against Min/Max as 'flags' are valid!")]
        public double GetRandomDouble(double min = double.MinValue, double max = double.MaxValue)
        {
            double retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == double.MinValue)
                {
                    min = OneShot ? MinDoubleOneShot : MinDouble;
                }

                if (max == double.MaxValue)
                {
                    max = OneShot ? MaxDoubleOneShot : MaxDouble;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, (double)(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, (double)(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = (_random.NextDouble() * (max - min)) + min;
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random enum value.
        /// </summary>
        /// <typeparam name="T">The enum</typeparam>
        /// <returns>A random value!</returns>
        public T GetRandomEnum<T>()
        {
            return (T)GetRandomEnum(typeof(T));
        }

        /// <summary>
        ///     Gets a random file path. The file will not exist, this is JUST a string that looks like a folder path!
        /// </summary>
        /// <param name="numFolders">The maximum number of folders.</param>
        /// <param name="folderNameLength">Maximum length of the folder name.</param>
        /// <param name="fileNameLength">Maximum length of the file name.</param>
        /// <param name="extensionLength">Maximum length of the extension.</param>
        /// <returns>A passable, but wierd, filepath</returns>
        public string GetRandomFilePath(int numFolders = 5, int folderNameLength = 10, int fileNameLength = 20, int extensionLength = 3)
        {
            // Start of with the drive letter
            string retVal = GetRandomString(StringSources.UppercaseAlphaCharacters, 1, 1, true) + ":\\";

            numFolders = GetRandomInt(0, numFolders);

            // Add some folders
            for (int i = 0; i < numFolders; i++)
            {
                retVal += GetRandomString(StringSources.AlphaNumericCharactersWithSpaces, 1, folderNameLength, true).Trim() + "\\";
            }

            // A filename
            retVal += GetRandomString(StringSources.AlphaNumericCharactersWithSpaces, 1, fileNameLength, true).Trim() + ".";

            // And an extension
            retVal += GetRandomString(StringSources.LowercaseAlphaCharacters, 1, extensionLength, true);

            return retVal;
        }

        /// <summary>
        ///     Gets a random float in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinFloat" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxFloat" />)</param>
        /// <returns>A random float value</returns>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Comparisons against Min/Max as 'flags' are valid!")]
        public float GetRandomFloat(float min = float.MinValue, float max = float.MaxValue)
        {
            float retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == float.MinValue)
                {
                    min = OneShot ? MinFloatOneShot : MinFloat;
                }

                if (max == float.MaxValue)
                {
                    max = OneShot ? MaxFloatOneShot : MaxFloat;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToSingle(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, Convert.ToSingle(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = (float)((_random.NextDouble() * (max - (double)min)) + min);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random, but repeatable unique identifier.
        /// </summary>
        /// <param name="seed">The optional seed. If passed you will get the same random Guid back for the same seed.</param>
        /// <returns>
        ///     A Guid
        /// </returns>
        public Guid GetRandomGuid(int seed = int.MinValue)
        {
            if (seed != int.MinValue)
            {
                _random = new Random(seed);
            }

            byte[] initialiser = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                initialiser[i] = GetRandomByte();
            }

            return new Guid(initialiser);
        }

        /// <summary>
        ///     Gets a random int in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinInt" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxInt" />)</param>
        /// <returns>A random int value</returns>
        public int GetRandomInt(int min = int.MinValue, int max = int.MaxValue)
        {
            int retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == int.MinValue)
                {
                    min = OneShot ? MinIntOneShot : MinInt;
                }

                if (max == int.MaxValue)
                {
                    max = OneShot ? MaxIntOneShot : MaxInt;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, (int)(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, (int)(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = _random.Next(min, max);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random long in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinLong" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxLong" />)</param>
        /// <returns>A random int value</returns>
        public long GetRandomLong(long min = long.MinValue, long max = long.MaxValue)
        {
            long retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == long.MinValue)
                {
                    min = OneShot ? MinLongOneShot : MinLong;
                }

                if (max == long.MaxValue)
                {
                    max = OneShot ? MaxLongOneShot : MaxLong;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToInt64(((RangeAttribute)validationAttribute).Minimum ?? min));

                            // [Range] with a ulong.MaxInt actually uses the double version of [Range] and for some reason
                            // there is an overflow exception when assigning the double version on ulong.MaxValue to ulong
                            // so we do it in a more convoluted way!
                            // There isn't the same issue with .MinValue, go figure!
                            object omax = ((RangeAttribute)validationAttribute).Maximum;
                            long lmax = max;

                            if (omax != null)
                            {
                                if (omax is double && ((double)omax >= long.MaxValue - 1))
                                {
                                    lmax = long.MaxValue;
                                }
                                else
                                {
                                    lmax = Convert.ToInt64(omax);
                                }
                            }

                            max = Math.Min(max, lmax);
                        }
                    }
                }

                retVal = min + (long)(_random.NextDouble() * (max - min));
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random sbyte in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinSByte" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxSByte" />)</param>
        /// <returns>A random sbyte value</returns>
        public sbyte GetRandomSByte(sbyte min = sbyte.MinValue, sbyte max = sbyte.MaxValue)
        {
            sbyte retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == sbyte.MinValue)
                {
                    min = OneShot ? MinSByteOneShot : MinSByte;
                }

                if (max == sbyte.MaxValue)
                {
                    max = OneShot ? MaxSByteOneShot : MaxSByte;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToSByte(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, Convert.ToSByte(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = (sbyte)_random.Next(min, max);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random short in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinShort" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxShort" />)</param>
        /// <returns>A random short value</returns>
        public short GetRandomShort(short min = short.MinValue, short max = short.MaxValue)
        {
            short retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == short.MinValue)
                {
                    min = OneShot ? MinShortOneShot : MinShort;
                }

                if (max == short.MaxValue)
                {
                    max = OneShot ? MaxShortOneShot : MaxShort;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToInt16(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, Convert.ToInt16(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = (short)_random.Next(min, max);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random string of whatever length using the current <paramref name="stringSource" /> as the source for
        ///     characters.
        ///     <seealso cref="StringSources" />
        /// </summary>
        /// <param name="stringSource">The string source to fetch chars from. Defaults to <see cref="StringSource" /></param>
        /// <param name="minStrLen">Minimum length of the string. Defaults to <see cref="MinStrLen" /></param>
        /// <param name="maxStrLen">Maximum length of the string. Defaults to <see cref="MaxStrLen" /></param>
        /// <returns>
        ///     The random string
        /// </returns>
        public string GetRandomString(string stringSource = null, int minStrLen = -1, int maxStrLen = -1)
        {
            return GetRandomString(stringSource, minStrLen, maxStrLen, false);
        }

        /// <summary>
        ///     Gets a random uint in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinUInt" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxUInt" />)</param>
        /// <returns>A random uint value</returns>
        public uint GetRandomUInt(uint min = uint.MinValue, uint max = uint.MaxValue)
        {
            uint retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == uint.MinValue)
                {
                    min = OneShot ? MinUIntOneShot : MinUInt;
                }

                if (max == uint.MaxValue)
                {
                    max = OneShot ? MaxUIntOneShot : MaxUInt;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToUInt32(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, Convert.ToUInt32(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = min + (uint)(_random.NextDouble() * (max - min));
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random ulong in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinULong" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxULong" />)</param>
        /// <returns>A random ulong value</returns>
        public ulong GetRandomULong(ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
        {
            ulong retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == ulong.MinValue)
                {
                    min = OneShot ? MinULongOneShot : MinULong;
                }

                if (max == ulong.MaxValue)
                {
                    max = OneShot ? MaxULongOneShot : MaxULong;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToUInt64(((RangeAttribute)validationAttribute).Minimum ?? min));

                            // [Range] with a ulong.MaxInt actually uses the double version of [Range] and for some reason
                            // there is an overflow exception when assigning the double version on ulong.MaxValue to ulong
                            // so we do it in a more convoluted way!
                            // There isn't the same issue with .MinValue, go figure!
                            object omax = ((RangeAttribute)validationAttribute).Maximum;

                            ulong lmax = max;

                            if (omax != null)
                            {
                                if (omax is double && ((double)omax >= long.MaxValue - 1))
                                {
                                    lmax = long.MaxValue;
                                }
                                else
                                {
                                    lmax = Convert.ToUInt64(omax);
                                }
                            }

                            max = Math.Min(max, lmax);
                        }
                    }
                }

                retVal = min + (ulong)(_random.NextDouble() * (max - min));
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random ushort in a specified range.
        /// </summary>
        /// <param name="min">The minimum. (If default, defaults to <see cref="MinUShort" />)</param>
        /// <param name="max">The maximum. (If default, defaults to <see cref="MaxUShort" />)</param>
        /// <returns>A random ushort value</returns>
        public ushort GetRandomUShort(ushort min = ushort.MinValue, ushort max = ushort.MaxValue)
        {
            ushort retVal = 0;

            if (!GetOneShotItemsSource(ref retVal))
            {
                if (min == ushort.MinValue)
                {
                    min = OneShot ? MinUShortOneShot : MinUShort;
                }

                if (max == ushort.MaxValue)
                {
                    max = OneShot ? MaxUShortOneShot : MaxUShort;
                }

                if (OneShot ? RespectValidationOneShot : RespectValidation)
                {
                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RangeAttribute))
                        {
                            min = Math.Max(min, Convert.ToUInt16(((RangeAttribute)validationAttribute).Minimum ?? min));
                            max = Math.Min(max, Convert.ToUInt16(((RangeAttribute)validationAttribute).Maximum ?? max));
                        }
                    }
                }

                retVal = (ushort)_random.Next(min, max);
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random value of the appropriate type. Currently supports;
        ///     int, double, string, DateTime,
        /// </summary>
        /// <typeparam name="T">A basic type</typeparam>
        /// <param name="recursive">Determines if nested classes will also be created</param>
        /// <returns>A random(ish) value (i.e. non default - but COULD be!)</returns>
        public T GetRnd<T>(bool recursive = true)
        {
            return (T)GetRnd(typeof(T), recursive);
        }

        /// <summary>
        ///     Gets a random value of the appropriate type. Currently supports;
        ///     int, double, string, DateTime,
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="recursive">Determines if nested classes will also be created</param>
        /// <returns>
        ///     An object of the required type filled with a random(ish) value
        /// </returns>
        [
            SuppressMessage("StyleCop.CSharp.LayoutRules",
                "SA1503:CurlyBracketsMustNotBeOmitted",
                Justification = "I allow myself this in these circumstances!")]
        public object GetRnd(Type fieldType, bool recursive)
        {
            if (_ignoreTypes.Contains(fieldType))
            {
                IgnoredIssueTracer?.Invoke($"Ignoring creation of {fieldType.FullName}. Instance created as null.");
                return null;
            }

            // First check if we have a Nullable type
            if (fieldType.GetTypeInfo().IsGenericType &&
                (fieldType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                fieldType = fieldType.GetTypeInfo().DeclaredProperties.Skip(1).First().PropertyType;

                // OK, so we have a nullble type, let's check the null threshold
                bool allowNulls = OneShot ? AllowNullsOneShot : AllowNulls;

                if (allowNulls && GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                {
                    return null;
                }
            }

            if (fieldType == typeof(bool))
            {
                return GetRandomBool();
            }

            if (fieldType == typeof(byte))
            {
                return GetRandomByte();
            }

            if (fieldType == typeof(char))
            {
                return GetRandomChar();
            }

            if (fieldType == typeof(sbyte))
            {
                return GetRandomSByte();
            }

            if (fieldType == typeof(short))
            {
                return GetRandomShort();
            }

            if (fieldType == typeof(ushort))
            {
                return GetRandomUShort();
            }

            if (fieldType == typeof(int))
            {
                return GetRandomInt();
            }

            if (fieldType == typeof(uint))
            {
                return GetRandomUInt();
            }

            if (fieldType == typeof(long))
            {
                return GetRandomLong();
            }

            if (fieldType == typeof(ulong))
            {
                return GetRandomULong();
            }

            if (fieldType == typeof(double))
            {
                return GetRandomDouble();
            }

            if (fieldType == typeof(decimal))
            {
                return GetRandomDecimal();
            }

            if (fieldType == typeof(float))
            {
                return GetRandomFloat();
            }

            if (fieldType == typeof(string))
            {
                return GetRandomString();
            }

            if (fieldType == typeof(DateTime))
            {
                return GetRandomDateTime();
            }

            if (fieldType.GetTypeInfo().IsEnum)
            {
                return GetRandomEnum(fieldType);
            }

            if (fieldType == typeof(Guid))
            {
                return GetRandomGuid();
            }

            object retVal = TryFallingBackOnASuppliedCreator(fieldType);

            if (retVal != null)
            {
                return retVal;
            }

            if (recursive && fieldType.GetTypeInfo().IsClass)
            {
                return CreateFilled(fieldType, recursive);
            }

            if (recursive && fieldType.GetTypeInfo().IsInterface)
            {
                return CreateMappedInterface(fieldType, recursive);
            }

            return null;
        }

        /// <summary>
        ///     Maps a type to its creator. If you need a non-normal type, essentially one that needs a non-default constructor,
        ///     then map a creator that must implement <see cref="ObjectCreationAttribute.SITypeCreation{T}" /> as a static!
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <typeparam name="TCreator">The type of the creator.</typeparam>
        public void MapCreator<T, TCreator>()
        {
            MethodInfo creatorMethod = typeof(TCreator).GetTypeInfo().GetDeclaredMethod("CreateInstance");

            if (creatorMethod == null)
            {
                throw new ArgumentException($"The Creator needs to implement ObjectCreationAttribute.SITypeCreation<{typeof(T).Name}>");
            }

            if (!_creatorMap.ContainsKey(typeof(T)))
            {
                _creatorMap.Add(typeof(T), creatorMethod);
            }
            else
            {
                _creatorMap[typeof(T)] = creatorMethod;
            }
        }

        /// <summary>
        ///     Maps an interface to an implementation to use when creating members that are interfaces
        /// </summary>
        /// <typeparam name="I">Interface type to map</typeparam>
        /// <typeparam name="T">Type to map the interface to</typeparam>
        public void MapInterface<I, T>()
        {
            MapInterface(typeof(I), typeof(T));
        }

        /// <summary>
        /// Instructs the creator that any type of {T} should not be created and replaced with null
        /// </summary>
        /// <typeparam name="T">The type not to create</typeparam>
        public void DoNotCreate<T>()
        {
            _ignoreTypes.Add(typeof(T));
        }

        /// <summary>
        ///     Maps an interface to an implementation to use when creating members that are interfaces
        /// </summary>
        /// <param name="I">Interface type to map</param>
        /// <param name="T">Type to map the interface to</param>
        public void MapInterface(Type I, Type T)
        {
            if (!I.IsAssignableFrom(T))
            {
                throw new ArgumentException(string.Format("{0} does not implement {1}!", T.Name, I.Name));
            }

            if (!_interfaceMap.ContainsKey(I))
            {
                _interfaceMap.Add(I, T);
            }
            else
            {
                _interfaceMap[I] = T;
            }
        }

        /// <summary>
        ///     Maps a type that might be created to an interface that carries same named elements that have creation attributes -
        ///     NOT all members are required in the interface, ONLY those you wish to have specific
        ///     <see cref="ObjectCreationAttribute" />s.
        /// </summary>
        /// <typeparam name="T">Type to map the interface to</typeparam>
        /// <typeparam name="I">Interface type to map</typeparam>
        public void MapInterfaceAttributes<T, I>()
        {
            MapInterfaceAttributes(typeof(T), typeof(I));
        }

        /// <summary>
        ///     Maps a type that might be created to an interface that carries same named elements that have creation attributes -
        ///     NOT all members are required in the interface, ONLY those you wish to have specific
        ///     <see cref="ObjectCreationAttribute" />s.
        /// </summary>
        /// <param name="T">Type to map the interface to</param>
        /// <param name="I">Interface type to map</param>
        public void MapInterfaceAttributes(Type T, Type I)
        {
            if (!_interfaceAttributesMap.ContainsKey(T))
            {
                _interfaceAttributesMap.Add(T, I);
            }
            else
            {
                _interfaceAttributesMap[T] = I;
            }
        }

        /// <summary>
        ///     Absolute maximum number of created objects. Override this if you REALLY want the capability to create more than
        ///     this!
        /// </summary>
        /// <returns>10,000,000</returns>
        protected virtual int AbsoluteMaximumObjects()
        {
            return 10000000;
        }

        /// <summary>
        ///     Creates an object whose public fields and properties are filled with random(ish) values.
        /// </summary>
        /// <param name="instanceType">The type of the instance being created.</param>
        /// <param name="recursive">Determines if nested classes will be filled as well. Usually want this.</param>
        /// <returns>
        ///     An object populated with guff
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     If the class is an enumerable and is not at least IList
        /// </exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I allow this for exceptions!")]
        protected object CreateFilled(Type instanceType, bool recursive)
        {
            if (IgnoreRecursion)
            {
                int hashCode = instanceType.GetHashCode();

                if (encounteredObjects.Contains(hashCode))
                {
                    IgnoredIssueTracer?.Invoke($"Detected a recursive creation for {instanceType.FullName}. Instance created as null.");

                    return null;
                }

                encounteredObjects.Add(hashCode);
            }

            object retVal = null;

            if (ObjectCount < MaxObjects)
            {
                if (!GetOneShotItemsSource(ref retVal))
                {
                    bool allowNulls = OneShot ? AllowNullsOneShot : AllowNulls;
                    bool allowNullElementsInEnumerable = OneShot ? AllowNullElementsInEnumerableOneShot : AllowNullElementsInEnumerable;
                    bool respectValidation = OneShot ? RespectValidationOneShot : RespectValidation;

                    if (respectValidation)
                    {
                        foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                        {
                            if (validationAttribute.GetType() == typeof(RequiredAttribute))
                            {
                                allowNulls = false;
                            }
                            else if (validationAttribute.GetType() == typeof(ListContentValidationAttribute))
                            {
                                if (!((ListContentValidationAttribute)validationAttribute).AllowNulls)
                                {
                                    allowNullElementsInEnumerable = false;
                                }

                                if (((ListContentValidationAttribute)validationAttribute).MinElements < int.MaxValue)
                                {
                                    MinItemsOneShot = ((ListContentValidationAttribute)validationAttribute).MinElements;
                                }

                                if (((ListContentValidationAttribute)validationAttribute).MaxElements > int.MinValue)
                                {
                                    MaxItemsOneShot = ((ListContentValidationAttribute)validationAttribute).MaxElements;
                                }
                            }
                        }
                    }

                    if (!allowNulls || GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(instanceType))
                        {
                            if (!typeof(IList).IsAssignableFrom(instanceType))
                            {
                                throw new ArgumentException(string.Format("{0} needs to implement IList!", instanceType.Name));
                            }

                            Type listContentType;

                            if (instanceType.GenericTypeArguments.Length == 0)
                            {
                                if (!instanceType.IsArray)
                                {
                                    throw new ArgumentException(string.Format("As {0} is non-generic, this version can only handle arrays, sorry!",
                                                                              instanceType.Name));
                                }

                                listContentType = instanceType.GetElementType();
                            }
                            else
                            {
                                if (instanceType.GenericTypeArguments.Length > 1)
                                {
                                    throw new ArgumentException(string.Format("{0} can only have a single generic with this version, sorry!",
                                                                              instanceType.Name));
                                }

                                listContentType = instanceType.GenericTypeArguments[0];
                            }

                            int numItems = _random.Next(OneShot ? MinItemsOneShot : MinItems, OneShot ? MaxItemsOneShot : MaxItems);

                            retVal = InstantiateType(instanceType, numItems);
                            ObjectCount++;

                            for (int i = 0; i < numItems; i++)
                            {
                                object item = null;

                                if (!allowNullElementsInEnumerable || GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                                {
                                    _elementContext.Add($"[{i}]");
                                    item = GetRnd(listContentType, recursive);
                                    _elementContext.RemoveAt(_elementContext.Count - 1);
                                }

                                if (instanceType.IsArray)
                                {
                                    ((Array)retVal).SetValue(item, i);
                                }
                                else
                                {
                                    ((IList)retVal).Add(item);
                                }
                            }
                        }
                        else
                        {
                            retVal = InstantiateType(instanceType);
                            FieldInfo[] fields = instanceType.GetFields();

                            foreach (FieldInfo field in fields)
                            {
                                if (!field.IsLiteral)
                                {
                                    PopulateField(retVal, field, recursive);
                                }
                            }

                            PropertyInfo[] props = instanceType.GetProperties();

                            foreach (PropertyInfo prop in props)
                            {
                                PopulateProperty(retVal, prop, recursive);
                            }
                        }
                    }
                }
            }

            if (IgnoreRecursion)
            {
                encounteredObjects.RemoveAt(encounteredObjects.Count - 1);
            }

            return retVal;
        }

        /// <summary>
        ///     Adds the field attributes from the associated interface.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="attributeInterface">The attribute interface.</param>
        /// <param name="memberAttributes">The member attributes.</param>
        /// <param name="isProperty">Determines if we're looking for a property or a field.</param>
        private void AddMemberAttributesFromInterface(string memberName,
                                                      Type attributeInterface,
                                                      List<CustomAttributeData> memberAttributes,
                                                      bool isProperty)
        {
            // Now look for a corresponding member in this interface and see if it has any custom attributes to add to the list
            MemberInfo interfaceMember;

            if (isProperty)
            {
                interfaceMember = attributeInterface.GetTypeInfo().GetDeclaredProperty(memberName);
            }
            else
            {
                interfaceMember = attributeInterface.GetTypeInfo().GetDeclaredField(memberName);
            }

            if (interfaceMember != null)
            {
                List<CustomAttributeData> interfaceMemberAttributes = interfaceMember.CustomAttributes.ToList();

                // We use Insert range so that parent interface field attributes will appear (and be treated) first, allowing children to override parents
                memberAttributes.InsertRange(0, interfaceMemberAttributes);
            }
        }

        /// <summary>
        ///     Creates an interface member using the mapped implementation type
        /// </summary>
        /// <param name="fieldType">Type of the interface field.</param>
        /// <param name="recursive">Determines if nested classes will also be created</param>
        /// <returns>
        ///     A filled implementation
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     If there is a problem with the interface
        /// </exception>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "I excuse throwing exceptions!")]
        private object CreateMappedInterface(Type fieldType, bool recursive)
        {
            object retVal = null;

            if (ObjectCount < MaxObjects)
            {
                if (!GetOneShotItemsSource(ref retVal))
                {
                    bool allowNulls = OneShot ? AllowNullsOneShot : AllowNulls;
                    bool allowNullElementsInEnumerable = OneShot ? AllowNullElementsInEnumerableOneShot : AllowNullElementsInEnumerable;

                    foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                    {
                        if (validationAttribute.GetType() == typeof(RequiredAttribute))
                        {
                            allowNulls = false;
                        }
                        else if (validationAttribute.GetType() == typeof(ListContentValidationAttribute))
                        {
                            if (!((ListContentValidationAttribute)validationAttribute).AllowNulls)
                            {
                                allowNullElementsInEnumerable = false;
                            }

                            if (((ListContentValidationAttribute)validationAttribute).MinElements < int.MaxValue)
                            {
                                MinItemsOneShot = ((ListContentValidationAttribute)validationAttribute).MinElements;
                            }

                            if (((ListContentValidationAttribute)validationAttribute).MaxElements > int.MinValue)
                            {
                                MaxItemsOneShot = ((ListContentValidationAttribute)validationAttribute).MaxElements;
                            }
                        }
                    }

                    if (!allowNulls || GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                    {
                        Type implementationType = null;

                        if (OneShot && (ImplementationOneShot != null))
                        {
                            implementationType = ImplementationOneShot;
                        }
                        else if (_interfaceMap.ContainsKey(fieldType))
                        {
                            implementationType = _interfaceMap[fieldType];
                        }

                        if (implementationType == null)
                        {
                            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(fieldType))
                            {
                                if (fieldType.GetTypeInfo().IsGenericType)
                                {
                                    if (fieldType.GenericTypeArguments.Length > 1)
                                    {
                                        throw new ArgumentException(string.Format("{0} can only have a single generic with this version, sorry!",
                                                                                  fieldType.Name));
                                    }

                                    Type listContentType = fieldType.GenericTypeArguments[0];

                                    implementationType = typeof(List<>).GetTypeInfo().MakeGenericType(listContentType);
                                }
                            }
                        }

                        if (implementationType == null)
                        {
                            if (IgnoreUnimplementedInterfaces)
                            {
                                IgnoredIssueTracer?.Invoke($"There is no implemention defined for {fieldType.FullName}, the created instance will be null");
                            }
                            else
                            {
                                throw new ArgumentException(string.Format("There is no implementation defined for {0}!", fieldType.FullName));
                            }
                        }

                        if (implementationType == null)
                        {
                            retVal = null;
                        }
                        else
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(fieldType))
                            {
                                if (!typeof(IList).IsAssignableFrom(implementationType))
                                {
                                    throw new ArgumentException(string.Format("{0} needs to implement IList!", implementationType.Name));
                                }

                                if (implementationType.GenericTypeArguments.Length > 1)
                                {
                                    throw new ArgumentException(string.Format("{0} can only have a single generic with this version, sorry!",
                                                                              implementationType.Name));
                                }

                                Type listContentType = implementationType.GenericTypeArguments[0];
                                retVal = InstantiateType(implementationType);
                                ObjectCount++;

                                int numItems = _random.Next(OneShot ? MinItemsOneShot : MinItems, OneShot ? MaxItemsOneShot : MaxItems);

                                for (int i = 0; i < numItems; i++)
                                {
                                    object item = null;

                                    if (!allowNullElementsInEnumerable || GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                                    {
                                        _elementContext.Add($"[{i}]");
                                        item = GetRnd(listContentType, recursive);
                                        _elementContext.RemoveAt(_elementContext.Count - 1);
                                    }

                                    ((IList)retVal).Add(item);
                                }
                            }
                            else
                            {
                                retVal = CreateFilled(implementationType, recursive);
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Gets the one shot items source. If there is one.
        /// </summary>
        /// <typeparam name="T">The type of the element to fetch</typeparam>
        /// <param name="value">The value retrieved from the ItemsSource.</param>
        /// <returns>True if there was an ItemsSource for the element.</returns>
        private bool GetOneShotItemsSource<T>(ref T value)
        {
            bool retVal = false;

            if (OneShot && (ItemsSourceOneShot != null) && !string.IsNullOrWhiteSpace(ItemsSourceOneShotName))
            {
                // First we collate our static property and methods
                PropertyInfo sourceProperty = ItemsSourceOneShot.GetTypeInfo().GetDeclaredProperty(ItemsSourceOneShotName);
                MethodInfo itemsSourceMethod = ItemsSourceOneShot.GetTypeInfo().GetDeclaredMethod("ItemSource");
                MethodInfo preferPropertyMethod = ItemsSourceOneShot.GetTypeInfo().GetDeclaredMethod("PreferPropertyToItemSourceCall");

                bool preferProperty = false;

                // If we have a property we COULD call AND we have an alternative items source method AND we have a method to determine if the property SHOULD still be used
                if ((sourceProperty != null) && (itemsSourceMethod != null) && (preferPropertyMethod != null))
                {
                    // Then we'll ask to see if we should prefer the property
                    object[] param =
                    {
                        ItemsSourceOneShotName
                    };
                    preferProperty = (bool)preferPropertyMethod.Invoke(null, param);
                }

                if ((itemsSourceMethod != null) && !preferProperty)
                {
                    try
                    {
                        object[] param =
                        {
                            ItemsSourceOneShotName,
                            CurrentPropertyInfo,
                            CurrentFieldInfo,
                            this
                        };

                        // Just in case the ItemSource makes use of US, The Creator, makes sure we don't go to recursive hell!
                        OneShot = false;
                        IList<T> source = (IList<T>)itemsSourceMethod.Invoke(null, param);
                        OneShot = true;

                        if (source != null)
                        {
                            int rndIndex = _random.Next(0, source.Count);
                            value = source[rndIndex];
                            retVal = true;
                        }
                    }
                    catch (Exception excep)
                    {
                        throw new ObjectCreationException(excep,
                                                          ObjectCreationExceptionReason.IssueWithItemsSourceMethod,
                                                          ItemsSourceOneShot.Name,
                                                          ItemsSourceOneShotName);
                    }
                }
                else if (sourceProperty != null)
                {
                    try
                    {
                        IList<T> source = (IList<T>)sourceProperty.GetValue(null);

                        if (source != null)
                        {
                            int rndIndex = _random.Next(0, source.Count);
                            value = source[rndIndex];
                            retVal = true;
                        }
                    }
                    catch (Exception excep)
                    {
                        throw new ObjectCreationException(excep,
                                                          ObjectCreationExceptionReason.IssueWithItemsSourceProperty,
                                                          ItemsSourceOneShot.Name,
                                                          ItemsSourceOneShotName);
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Gets a random enum.
        /// </summary>
        /// <param name="fieldType">Type of the enum.</param>
        /// <returns>A random value for the specified enum type</returns>
        private object GetRandomEnum(Type fieldType)
        {
            object retVal;
            Array enumValues = Enum.GetValues(fieldType);
            int index = GetRandomInt(0, enumValues.GetUpperBound(0));
            retVal = enumValues.GetValue(index);
            return retVal;
        }

        /// <summary>
        ///     Gets a random string of whatever length using the current <paramref name="stringSource" /> as the source for
        ///     characters.
        ///     <seealso cref="StringSources" />
        /// </summary>
        /// <param name="stringSource">The string source to fetch chars from.</param>
        /// <param name="minStrLen">Minimum length of the string.</param>
        /// <param name="maxStrLen">Maximum length of the string.</param>
        /// <param name="internalOnly">Only true when called, possibly recursively, from within some other GetRandomXXX method.</param>
        /// <returns>
        ///     The random string
        /// </returns>
        private string GetRandomString(string stringSource, int minStrLen, int maxStrLen, bool internalOnly)
        {
            string retVal = null;

            if ((ObjectCount < MaxObjects) || internalOnly)
            {
                // Try to get a string from the appropriate items source if possible
                if (internalOnly || !GetOneShotItemsSource(ref retVal))
                {
                    // That wasn't possible so we need to generate a string
                    if (stringSource == null)
                    {
                        stringSource = OneShot ? StringSourceOneShot : StringSource;
                    }

                    if (!string.IsNullOrEmpty(stringSource)) // Just a sanity check.
                    {
                        if (minStrLen == -1)
                        {
                            minStrLen = OneShot ? MinStrLenOneShot : MinStrLen;
                        }

                        if (maxStrLen == -1)
                        {
                            maxStrLen = OneShot ? MaxStrLenOneShot : MaxStrLen;
                        }

                        bool allowNulls = OneShot ? AllowNullsOneShot : AllowNulls;

                        if (internalOnly || !allowNulls || GetRandomBool(OneShot ? NullThresholdOneShot : NullThreshold))
                        {
                            if (!internalOnly && (OneShot ? RespectValidationOneShot : RespectValidation))
                            {
                                int minValidationLength = minStrLen;

                                foreach (ValidationAttribute validationAttribute in ValidationAttributes)
                                {
                                    // First deal with the special attributes
                                    if (validationAttribute.GetType() == typeof(GuidValidationAttribute))
                                    {
                                        // The string needs to be a valid, random, REPEATABLE Guid!
                                        retVal = GetRandomGuid().ToString();

                                        // That's it, nothing more. A RARE early exit return!
                                        return retVal;
                                    }

                                    if (validationAttribute.GetType() == typeof(FileValidationAttribute))
                                    {
                                        // The string needs to represent a filepath - we won't check the file exists!
                                        retVal = GetRandomFilePath(5, 10, 10, 3);

                                        // That's it, nothing more. A RARE early exit return!
                                        return retVal;
                                    }

                                    // Now the less than special attributes - sorry guys, that's just how it is!
                                    if (validationAttribute.GetType() == typeof(RequiredAttribute))
                                    {
                                        minStrLen = Math.Max(minStrLen, 1);
                                        allowNulls = false;
                                    }
                                    else if (validationAttribute.GetType() == typeof(StringLengthAttribute))
                                    {
                                        minValidationLength = ((StringLengthAttribute)validationAttribute).MinimumLength;
                                        maxStrLen = Math.Min(maxStrLen, ((StringLengthAttribute)validationAttribute).MaximumLength);
                                    }
                                }

                                minStrLen = Math.Max(minStrLen, minValidationLength);
                            }

                            // Get the string length we're going to use
                            int strLen = _random.Next(minStrLen, maxStrLen);

                            if (strLen == 0)
                            {
                                retVal = string.Empty;
                            }
                            else
                            {
                                char[] randomChars = new char[strLen];

                                for (int i = 0; i < strLen; i++)
                                {
                                    randomChars[i] = stringSource[_random.Next(stringSource.Length)];
                                }

                                retVal = new string(randomChars);

                                if (!internalOnly)
                                {
                                    ObjectCount++;
                                }
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Initialises the properties of this instance.
        /// </summary>
        private void Initialise()
        {
            IgnoreRecursion = true;
            IgnoreNonDefaultConstructors = true;
            IgnoreSetterExceptions = true;
            IgnoreUnimplementedInterfaces = true;

            MinByte = 0;
            MaxByte = 255;
            MinSByte = sbyte.MinValue;
            MaxSByte = sbyte.MaxValue;
            MinChar = char.MinValue;
            MaxChar = char.MaxValue;
            MinShort = short.MinValue;
            MaxShort = short.MaxValue;
            MinUShort = ushort.MinValue;
            MaxUShort = ushort.MaxValue;
            MinInt = int.MinValue;
            MaxInt = int.MaxValue;
            MinUInt = uint.MinValue;
            MaxUInt = uint.MaxValue;
            MinLong = 0;
            MaxLong = long.MaxValue;
            MinULong = ulong.MinValue;
            MaxULong = ulong.MaxValue;
            MinDouble = double.MinValue;
            MaxDouble = double.MaxValue;
            MinDecimal = decimal.MinValue;
            MaxDecimal = decimal.MaxValue;
            MinFloat = float.MinValue;
            MaxFloat = float.MaxValue;
            MinStrLen = 0;
            MaxStrLen = 32;
            MinDateTime = DateTime.MinValue;
            MaxDateTime = DateTime.MaxValue;
            MinDateTimeOffset = DateTimeOffset.MinValue;
            MaxDateTimeOffset = DateTimeOffset.MaxValue;
            MinItems = 0;
            MaxItems = 4;
            StringSource = StringSources.EverydayCharacters;
            ValidationAttributes = new List<ValidationAttribute>();
            AllowNulls = false;
            AllowNullElementsInEnumerable = false;
            NullThreshold = 0.1;
        }

        /// <summary>
        ///     Initialises the creation state.
        /// </summary>
        private void InitialiseCreation()
        {
            ObjectCount = 0;

            // Ensure we always get an initial object
            InitialiseOneShotValues();
            OneShot = true;
            AllowNullsOneShot = false;
        }

        /// <summary>
        ///     Initialises the one shot values.
        /// </summary>
        private void InitialiseOneShotValues()
        {
            ImplementationOneShot = null;
            AllowNullsOneShot = AllowNulls;
            AllowNullElementsInEnumerableOneShot = AllowNullElementsInEnumerable;
            IgnoreOneShot = false;
            MinDateTimeOneShot = MinDateTime;
            MaxDateTimeOneShot = MaxDateTime;
            MinDateTimeOffsetOneShot = MinDateTimeOffset;
            MaxDateTimeOffsetOneShot = MaxDateTimeOffset;
            MinItemsOneShot = MinItems;
            MaxItemsOneShot = MaxItems;
            MinDecimalOneShot = (double)MinDecimal;
            MaxDecimalOneShot = (double)MaxDecimal;
            MinDoubleOneShot = MinDouble;
            MaxDoubleOneShot = MaxDouble;
            MinFloatOneShot = MinFloat;
            MaxFloatOneShot = MaxFloat;
            MinByteOneShot = MinByte;
            MaxByteOneShot = MaxByte;
            MinSByteOneShot = MinSByte;
            MaxSByteOneShot = MaxSByte;
            MinShortOneShot = MinShort;
            MaxShortOneShot = MaxShort;
            MinUShortOneShot = MinUShort;
            MaxUShortOneShot = MaxUShort;
            MinIntOneShot = MinInt;
            MaxIntOneShot = MaxInt;
            MinUIntOneShot = MinUInt;
            MaxUIntOneShot = MaxUInt;
            MinLongOneShot = MinLong;
            MaxLongOneShot = MaxLong;
            MinULongOneShot = MinULong;
            MaxULongOneShot = MaxULong;
            MinCharOneShot = MinChar;
            MaxCharOneShot = MaxChar;
            MinStrLenOneShot = MinStrLen;
            MaxStrLenOneShot = MaxStrLen;
            StringSourceOneShot = StringSource;
            RespectValidationOneShot = RespectValidation;
            NullThresholdOneShot = NullThreshold;
            TypeCreationOneShot = null;
            ItemsSourceOneShot = null;
            ItemsSourceOneShotName = string.Empty;
        }

        private object InstantiateType(Type instanceType, int numItems = 1)
        {
            object retVal;
            try
            {
                if (_interfaceMap.ContainsKey(instanceType))
                {
                    instanceType = _interfaceMap[instanceType];
                }

                if (instanceType.IsArray)
                {
                    retVal = Activator.CreateInstance(instanceType, numItems);
                }
                else
                {
                    retVal = Activator.CreateInstance(instanceType);
                }
            }
            catch (MissingMethodException excep)
            {
                retVal = TryFallingBackOnASuppliedCreator(instanceType);

                if (retVal == null)
                {
                    if (IgnoreNonDefaultConstructors)
                    {
                        IgnoredIssueTracer?.Invoke($"No default constructor for {instanceType.FullName}. Created instance is null.");
                    }
                    else
                        throw new ArgumentException("Creator currently only works with classes that have a default constructor",
                                                    instanceType.FullName,
                                                    excep);
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Populates the field, taking into account any custom attribute properties or validation requirements.
        /// </summary>
        /// <param name="instance">The instance to set the field on.</param>
        /// <param name="field">The field.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        private void PopulateField(object instance, FieldInfo field, bool recursive)
        {
            PrepareMemberAttributes(instance, field, recursive);
            CurrentFieldInfo = field;

            // Have we been asked to ignore setting this value?
            if (IgnoreOneShot)
            {
                IgnoredIssueTracer?.Invoke($"Ignoring {ElementContext}.{field.Name}");
            }
            else
            {
                try
                {
                    CreationTracer?.Invoke($"{ElementContext}.{field.Name} - {field.FieldType.Name}");
                    _elementContext.Add(field.Name);
                    field.SetValue(instance, GetRnd(field.FieldType, recursive));
                }
                catch (Exception e)
                {
                    if (IgnoreSetterExceptions)
                    {
                        IgnoredIssueTracer?.Invoke(
                            $"Could not call the setter for {field.DeclaringType.Name}.{field.Name}. Instance left at default value.");
                    }
                    else
                    {
                        throw new ObjectCreationException(e, ObjectCreationExceptionReason.CouldNotCallSetter, field.DeclaringType.Name, field.Name);
                    }
                }
                finally
                {
                    _elementContext.RemoveAt(_elementContext.Count-1);
                }
            }

            CurrentFieldInfo = null;
        }

        /// <summary>
        ///     Populates the property, taking into account any custom attribute properties or validation requirements.
        /// </summary>
        /// <param name="instance">The instance to set the property on.</param>
        /// <param name="prop">The property.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        private void PopulateProperty(object instance, PropertyInfo prop, bool recursive)
        {
            PrepareMemberAttributes(instance, prop, recursive);
            CurrentPropertyInfo = prop;

            // Have we been asked to ignore setting this value?
            if (IgnoreOneShot)
            {
                IgnoredIssueTracer?.Invoke($"Ignoring {ElementContext}.{prop.Name}");                
            }
            else
            {
                try
                {
                    CreationTracer?.Invoke($"{ElementContext}.{prop.Name} - {prop.PropertyType.Name}");
                    _elementContext.Add(prop.Name);
                    prop.SetValue(instance, GetRnd(prop.PropertyType, recursive));
                }
                catch (Exception e)
                {
                    if (IgnoreSetterExceptions)
                    {
                        IgnoredIssueTracer?.Invoke($"Could not call the setter for {prop.DeclaringType.Name}.{prop.Name}. Instance left at default value.");
                    }
                    else
                    {
                        throw new ObjectCreationException(e, ObjectCreationExceptionReason.CouldNotCallSetter, prop.DeclaringType.Name, prop.Name);
                    }
                }
                finally
                {
                    _elementContext.RemoveAt(_elementContext.Count - 1);
                }
            }

            CurrentPropertyInfo = null;
        }

        /// <summary>
        ///     Prepares any custom attribute properties or validation requirements for the member (Property or Field).
        /// </summary>
        /// <param name="instance">The instance to set the member on.</param>
        /// <param name="member">The member.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        private void PrepareMemberAttributes<TInfo>(object instance, TInfo member, bool recursive)
            where TInfo : MemberInfo
        {
            OneShot = false;
            InitialiseOneShotValues();

            bool isProperty = member is PropertyInfo;

            // Search to see if this member has an ObjectCreationAttribute on it.
            List<CustomAttributeData> memberAttributes = member.CustomAttributes.ToList();

            // Now see if this member's owner type has an attribute interface mapped to it
            if (_interfaceAttributesMap.ContainsKey(member.DeclaringType))
            {
                Type attributeInterface = _interfaceAttributesMap[member.DeclaringType];

                // Look for a corresponding property in this interface and see if it has any custom attributes to add to the list
                AddMemberAttributesFromInterface(member.Name, attributeInterface, memberAttributes, isProperty);

                // Now check any inherited interfaces
                foreach (Type inheritedInterface in attributeInterface.GetTypeInfo().GetInterfaces())
                {
                    AddMemberAttributesFromInterface(member.Name, inheritedInterface, memberAttributes, isProperty);
                }
            }

            ValidationAttributes.Clear();

            foreach (CustomAttributeData customAttributeData in memberAttributes)
            {
                if (customAttributeData.AttributeType == typeof(ObjectCreationAttribute))
                {
                    SetOneShotProperties(member.Name, customAttributeData);
                }
                else if (typeof(ValidationAttribute).IsAssignableFrom(customAttributeData.AttributeType))
                {
                    // We now need to create an instance of the attribute that is on the interface and add it to the list of attributes.
                    ValidationAttribute validAttr = (ValidationAttribute)customAttributeData.CreateAttribute();

                    if (validAttr != null)
                    {
                        ValidationAttributes.Add(validAttr);
                    }
                }
            }

            // Check if we need to account for validation properties
            if (OneShot ? RespectValidationOneShot : RespectValidation)
            {
                // See if there is any validation properties on the item and add that to any we found on it's mapped interface
                ValidationAttributes.AddRange(((ValidationAttribute[])member.GetCustomAttributes(typeof(ValidationAttribute), true)).ToList());

                // Now add any that are on its declared interfaces
                foreach (Type implementedInterface in member.DeclaringType.GetTypeInfo().ImplementedInterfaces)
                {
                    PropertyInfo interfaceProperty = implementedInterface.GetTypeInfo().GetDeclaredProperty(member.Name);

                    List<ValidationAttribute> interfaceAttributes = null;

                    if (interfaceProperty != null)
                    {
                        interfaceAttributes =
                            ((ValidationAttribute[])interfaceProperty.GetCustomAttributes(typeof(ValidationAttribute), true)).ToList();
                    }

                    if (interfaceAttributes != null)
                    {
                        ValidationAttributes.AddRange(interfaceAttributes);
                    }
                }
            }
        }

        /// <summary>
        ///     Sets the one shot properties, on this Creator instance, identified in the custom attribute.
        /// </summary>
        /// <param name="elementName">
        ///     Name of the field/property that the attribute is for - used for the
        ///     <see cref="ItemsSourceOneShot" />.
        /// </param>
        /// <param name="customAttributeData">The custom attribute data.</param>
        /// <exception cref="System.ArgumentException">If this class and <see cref="ObjectCreationAttribute" /> get out of synch</exception>
        [
            SuppressMessage("StyleCop.CSharp.LayoutRules",
                "SA1503:CurlyBracketsMustNotBeOmitted",
                Justification = "I allow this for returns & exceptions!")]
        private void SetOneShotProperties(string elementName, CustomAttributeData customAttributeData)
        {
            if (customAttributeData.NamedArguments == null)
            {
                return;
            }

            OneShot = true;

            // Set the individual on shot properties as required
            foreach (CustomAttributeNamedArgument customAttributeNamedArgument in customAttributeData.NamedArguments)
            {
                string oneShotPropertyName = customAttributeNamedArgument.MemberName + "OneShot";

                // Now invoke this method on, well me!
                PropertyInfo oneShotProperty = GetType().GetTypeInfo().GetDeclaredProperty(oneShotPropertyName);

                if (oneShotProperty == null)
                {
                    throw new ArgumentException(
                        string.Format("Couldn't find the Creator.{0} property, the ObjectCreationAttribute must be out of synch!",
                                      oneShotPropertyName));
                }

                oneShotProperty.SetValue(this, customAttributeNamedArgument.TypedValue.Value);
            }

            ItemsSourceOneShotName = elementName;
        }

        /// <summary>
        ///     Tries falling back on a supplied creator to create an instance of the particular type.
        /// </summary>
        /// <returns></returns>
        private object TryFallingBackOnASuppliedCreator(Type typeToCreate)
        {
            object retVal = null;

            MethodInfo creatorMethod = null;

            if (OneShot && (TypeCreationOneShot != null))
            {
                creatorMethod = TypeCreationOneShot.GetTypeInfo().GetDeclaredMethod("CreateInstance");
            }
            else if (_creatorMap.ContainsKey(typeToCreate))
            {
                creatorMethod = _creatorMap[typeToCreate];
            }

            if (creatorMethod != null)
            {
                object[] param =
                {
                    this
                };

                retVal = creatorMethod.Invoke(null, param);
            }

            return retVal;
        }
    }

    /// <summary>
    ///     An extension class that extends <see cref="CustomAttributeData" />. Modified from
    ///     http://haacked.com/archive/2010/08/05/copying-attributes.aspx/ for DotNetCore
    /// </summary>
    internal static class CustomAttributeDataExtension
    {
        /// <summary>
        ///     Creates an instance of an attribute from the <see cref="CustomAttributeData" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>An attribute that has the same value.</returns>
        public static Attribute CreateAttribute(this CustomAttributeData data)
        {
            IEnumerable<object> arguments = from arg in data.ConstructorArguments
                                            select arg.Value;

            Attribute attribute = data.Constructor.Invoke(arguments.ToArray()) as Attribute;

            if (attribute != null)
            {
                // Look for any properties or fields that need to be set.
                foreach (CustomAttributeNamedArgument namedArgument in data.NamedArguments)
                {
                    if (namedArgument.IsField)
                    {
                        FieldInfo fieldInfo = attribute.GetType().GetField(namedArgument.MemberName);

                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(attribute, namedArgument.TypedValue.Value);
                        }
                    }
                    else
                    {
                        PropertyInfo propertyInfo = attribute.GetType().GetProperty(namedArgument.MemberName);

                        if (propertyInfo != null)
                        {
                            propertyInfo.SetValue(attribute, namedArgument.TypedValue.Value, null);
                        }
                    }
                }
            }

            return attribute;
        }
    }
}

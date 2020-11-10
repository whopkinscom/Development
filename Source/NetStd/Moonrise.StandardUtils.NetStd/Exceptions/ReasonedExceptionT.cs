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
using Moonrise.Utils.Standard.Extensions;

namespace Moonrise.Utils.Standard.Exceptions
{
    /// <summary>
    ///     Reasoned exceptions require an enum to identify the reason for the exception.
    ///     The enum Description attribute carries the message for the exception. This gives you a concise list of all of the
    ///     reasons you expect an exception to be thrown and collates all of the messages into one place. The reason
    ///     descriptions
    ///     can contain parameter placeholders that will be passed to string.Format. If the Description attribute is repeated
    ///     as
    ///     the enum value XML summary documentation then you will also get intellisense on the message associated with the
    ///     reason.
    ///     <example>
    ///         <para>
    ///             Define the reason as follows;
    ///             public enum YourExceptionReason {
    ///             &lt;summary&gt;
    ///             An unknown exception occurred inside the method: {0}
    ///             &lt;/summary&gt;
    ///             [Description("An unknown exception occurred inside the method: {0}")]
    ///             UnknownException,
    ///             &lt;summary&gt;
    ///             The widget diameter of {0} was out of range. At this point it should be between {1} &amp; {2}.
    ///             &lt;/summary&gt;
    ///             [Description("The widget diameter of {0} was out of range. At this point it should be between {1} &amp;
    ///             {2}.")]
    ///             WidgetDiameterOutOfRange
    ///             }
    ///         </para>
    ///         <para>
    ///             public class YourException : ReasonedException&lt;YourExceptionReason&gt; ....
    ///         </para>
    ///         <para>
    ///             The exception would then be thrown as;
    ///             throw new YourException(YourExceptionReason.WidgetDiameterOutOfRange, widget.Diameter, MinWidgetDiameter,
    ///             MaxWidgetDiameter);
    ///         </para>
    ///     </example>
    /// </summary>
    /// <typeparam name="TReason">The type of the reason enum</typeparam>
    /// <seealso cref="System.Exception" />
    [Serializable]
    [SuppressMessage("Microsoft.Design",
        "CA1032:ImplementStandardExceptionConstructors",
        Justification = "The constructors have been designed for effective use of this exception pattern.")]
    public class ReasonedException<TReason> : Exception
        where TReason : struct, IConvertible // i.e. what you need is an Enum!
    {
        /// <summary>
        ///     Initialises a new instance of the ReasonedException class.
        /// </summary>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public ReasonedException(TReason reason, params object[] args)
            : base(string.Format(((Enum)(object)reason).Description(), args))
        {
            ReasonCode = reason;
        }

        /// <summary>
        ///     Initialises a new instance of the ReasonedException class but can take an inner exception.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public ReasonedException(Exception innerException, TReason reason, params object[] args)
            : base(string.Format(((Enum)(object)reason).Description(), args), innerException)
        {
            ReasonCode = reason;
        }

        /// <summary>
        ///     Gets the numeric error code for the reason of the exception.
        /// </summary>
        /// <value>The code.</value>
        public int ErrorCode
        {
            get
            {
                return (int)(object)ReasonCode;
            }
        }

        /// <summary>
        ///     Gets or sets the reason for the exception as one of the defined enum values.
        /// </summary>
        public TReason ReasonCode { get; protected set; }
    }
}

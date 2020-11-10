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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Moonrise.Utils.Standard.Exceptions;

namespace Moonrise.Utils.Test.ObjectCreation
{
    /// <summary>
    /// Reasons why an <see cref="ObjectCreationException"/> has been thrown
    /// </summary>
    public enum ObjectCreationExceptionReason
    {
        // Note that the summaries are the same as the descriptions. This means intellisense will indicate the message
        // and hence the vars required to complete the message. Please continue this pattern for additional messages.
        // IMPORTANT: If you change the description, PLEASE change the summary.

        /// <summary>
        /// There was an error whilst accessing the ItemsSource property {0}.{1}.
        /// </summary>
        [Description("There was an error whilst accessing the ItemsSource property {0}.{1}.")]
        IssueWithItemsSourceProperty,

        /// <summary>
        /// There was an error whilst accessing the ItemsSource method {0}.ItemsSource() for the {1} property.
        /// </summary>
        [Description("There was an error whilst accessing the ItemsSource method {0}.ItemsSource() for the {1} property.")]
        IssueWithItemsSourceMethod,

        /// <summary>
        ///     An unknown exception occurred within one of the ObjectCreation modules inside the method: {0} \nException
        ///     message; \n{1}
        /// </summary>
        [Description("An unknown exception occurred within one of the ObjectCreation modules inside the method: {0} \nException message; \n{1}")]
        UnknownException,

        /// <summary>
        /// Could not call the setter for {0}.{1}
        /// </summary>
        [Description("Could not call the setter for {0}.{1}")]
        CouldNotCallSetter
    }

    /// <summary>
    ///     Represents exceptional problems that arise whilst processing ObjectCreation functions
    /// </summary>
    [SuppressMessage("Microsoft.Design",
        "CA1032:ImplementStandardExceptionConstructors",
        Justification = "The constructors have been designed for effective use of this exception pattern.")]
    public class ObjectCreationException : ReasonedException<ObjectCreationExceptionReason>
    {
        /// <summary>
        ///     Initialises a new instance of the ObjectCreationException class.
        /// </summary>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public ObjectCreationException(ObjectCreationExceptionReason reason, params object[] args)
            : base(reason, args) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectCreationException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public ObjectCreationException(Exception innerException, ObjectCreationExceptionReason reason, params object[] args)
            : base(innerException, reason, args) { }
    }
}

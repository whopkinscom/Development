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

namespace Moonrise.Utils.Standard.Config
{
    /// <summary>
    ///     The reason why a <see cref="SettingsException" /> has been thrown
    /// </summary>
    public enum SettingsExceptionReason
    {
        // Note that the summaries are the same as the descriptions. This means intellisense will indicate the message
        // and hence the vars required to complete the message. Please continue this pattern for additional messages.
        // IMPORTANT: If you change the description, PLEASE change the summary.

        /// <summary>
        ///     An unknown exception occurred within one of the Settings modules inside the method: {0} \nException
        ///     message; \n{1}
        /// </summary>
        [Description("An unknown exception occurred within one of the Settings modules inside the method: {0} \nException message; \n{1}")]
        UnknownException,

        /// <summary>
        ///     An encrypted setting was found but no SettingsEncryptor has been specified.
        /// </summary>
        [Description("An encrypted setting was found but no SettingsEncryptor has been specified.")]
        NoEncryptionProvider,

        /// <summary>
        ///     The key syntax for [{0}] is not valid.
        /// </summary>
        [Description("The key syntax for [{0}] is not valid.")]
        InvalidKey,

        /// <summary>
        ///     The setting [{0}] contained invalid data.
        /// </summary>
        [Description("The setting [{0}] contained invalid data.")]
        InvalidData,

        /// <summary>
        /// When writing an encrypted value for key [{0}] to a re-encrypted settings file, you MUST pass the additional entropy.
        /// </summary>
        [Description("When writing an encrypted value for key [{0}] to a re-encrypted settings file, you MUST pass the additional entropy.")]
        AddtionalEntropyRequired,

        /// <summary>
        /// The additional entropy data for reading a re-encrypted setting was invalid.
        /// </summary>
        [Description("The additional entropy data for reading a re-encrypted setting was invalid.")]
        InvalidAdditionalEntropy
    }

    /// <summary>
    ///     Represents exceptional problems that arise whilst processing Settings functions
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design",
        "CA1032:ImplementStandardExceptionConstructors",
        Justification = "The constructors have been designed for effective use of this exception pattern.")]
    public class SettingsException : ReasonedException<SettingsExceptionReason>
    {
        /// <summary>
        ///     Initialises a new instance of the SettingsException class.
        /// </summary>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public SettingsException(SettingsExceptionReason reason, params object[] args)
            : base(reason, args) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingsException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public SettingsException(Exception innerException, SettingsExceptionReason reason, params object[] args)
            : base(innerException, reason, args) { }
    }
}

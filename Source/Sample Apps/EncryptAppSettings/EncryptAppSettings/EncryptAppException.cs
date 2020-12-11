using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Moonrise.Utils.Standard.Exceptions;

namespace EncryptAppSettings
{
    /// <summary>
    ///     The reason why a <see cref="EncryptAppException" /> has been thrown
    /// </summary>
    public enum EncryptAppExceptionReason
    {
        // Note that the summaries are the same as the descriptions. This means intellisense will indicate the message
        // and hence the vars required to complete the message. Please continue this pattern for additional messages.
        // IMPORTANT: If you change the description, PLEASE change the summary.

        /// <summary>
        ///     An unknown exception occurred within one of the EncryptApp modules inside the method: {0} \nException
        ///     message; \n{1}
        /// </summary>
        [Description("An unknown exception occurred within one of the EncryptApp modules inside the method: {0} \nException message; \n{1}")]
        UnknownException,

        /// <summary>
        ///     The setting [{0}] was not found in {1}
        /// </summary>
        [Description("The setting [{0}] was not found in {1}")]
        SettingNotFound,

        /// <summary>
        ///     You are not authorised to decrypt this file. Only {0} can decrypt this file.
        /// </summary>
        [Description("You are not authorised to decrypt this file. Only {0} can decrypt this file.")]
        UnauthorisedUser,

        /// <summary>
        ///     This machine is not authorised to decrypt this file. Only the encrypting user on {0} can decrypt this file.
        /// </summary>
        [Description("This machine is not authorised to decrypt this file. Only the encrypting user on {0} can decrypt this file.")]
        UnauthorisedMachine,

        /// <summary>
        ///    The file was not previously encrypted by this tool, or has had the encryption metadata removed!
        /// </summary>
        [Description("The file was not previously encrypted by this tool, or has had the encryption metadata removed!")]
        FileNotPreviouslyEncrypted,

        /// <summary>
        ///     No settings were specified. You must specify an option, try -? for a list of options.
        /// </summary>
        [Description("No settings were specified. You must specify an option, try -? for a list of options.")]
        NoSettingsSpecified,

        /// <summary>
        ///     The specified settings file [{0}] was not found.
        /// </summary>
        [Description("The specified settings file [{0}] was not found.")]
        SettingsInputFileNotFound,

        /// <summary>
        ///     {0}
        /// </summary>
        [Description("{0}")]
        CommandLineOptionException
    }

    /// <summary>
    ///     Represents exceptional problems that arise whilst processing EncryptApp functions
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design",
        "CA1032:ImplementStandardExceptionConstructors",
        Justification = "The constructors have been designed for effective use of this exception pattern.")]
    public class EncryptAppException : ReasonedException<EncryptAppExceptionReason>
    {
        /// <summary>
        ///     Initialises a new instance of the EncryptAppException class.
        /// </summary>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public EncryptAppException(EncryptAppExceptionReason reason, params object[] args)
            : base(reason, args)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptAppException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="reason">The reason for the exception.</param>
        /// <param name="args">
        ///     Each reason has an associated description string. Most of these take one or more arguments which
        ///     should be passed here.
        /// </param>
        public EncryptAppException(Exception innerException, EncryptAppExceptionReason reason, params object[] args)
            : base(innerException, reason, args)
        {
        }
    }
}

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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;

#if !DotNetCore
using System.Diagnostics;

#endif

namespace Moonrise.Logging
{
    /// <summary>
    ///     Logging class which logs messages through the ILoggingProvider interface.
    ///     <para>
    ///         By default it will use itself to log via the console and so you can simply call Logger.Log(...) and it will do
    ///         its stuff. To change the logger you must call
    ///         Logger.Logger = new some_implementation_of_ILoggingProvider()
    ///         somewhere in your required application!
    ///     </para>
    ///     <para>
    ///         I suggest sticking this in your using!;
    ///         using Logger = Moonrise.Utils.Standard.Logging.Logger;
    ///     </para>
    /// </summary>
    public class Logger : ILoggingProvider, IAuditProvider
    {
        /// <summary>
        ///     The reporting level to use. Log messages not covered by the reporting level will not get output. Note that Audit
        ///     messages will ALWAYS be output, even if logging has been disabled!
        /// </summary>
        public enum ReportingLevel
        {
            /// <summary>
            ///     All messages will be output (effectively the same as Trace)
            /// </summary>
            All = LoggingLevel.Trace - 1,

            /// <summary>
            ///     All messages of Trace and higher (<see cref="Debug" />, <see cref="Information" />, <see cref="Warning" />,
            ///     <see cref="Error" />,
            ///     <see cref="Fatal" />) will be output
            /// </summary>
            Trace = LoggingLevel.Trace,

            /// <summary>
            ///     All messages of Debug and higher (<see cref="Information" />, <see cref="Warning" />, <see cref="Error" />,
            ///     <see cref="Fatal" />) will be output
            /// </summary>
            Debug = LoggingLevel.Debug,

            /// <summary>
            ///     All messages of Information and higher (<see cref="Warning" />, <see cref="Error" />, <see cref="Fatal" />) will be
            ///     output. Can also be read as an enum of "Info".
            /// </summary>
            [Description("Info")] Information = LoggingLevel.Information,

            /// <summary>
            ///     All messages of Warning and higher (<see cref="Error" />, <see cref="Fatal" />) will be output. Can also be read as
            ///     an enum of "Warn".
            /// </summary>
            [Description("Warn")] Warning = LoggingLevel.Warning,

            /// <summary>
            ///     All messages of Error and higher (<see cref="Fatal" />) will be output
            /// </summary>
            Error = LoggingLevel.Error,

            /// <summary>
            ///     Only messages of Critical will be output
            /// </summary>
            Critical = LoggingLevel.Critical,

            /// <summary>
            ///     Only messages of Fatal will be output.
            /// </summary>
            Fatal = LoggingLevel.Fatal,

            /// <summary>
            ///     No messages will be output, except Audit messages
            /// </summary>
            [Description("None")] Off = LoggingLevel.Audit
        }

        /// <summary>
        ///     Keeps track of the indent in use where we start to log entry and exit points
        /// </summary>
        internal static readonly ThreadLocal<int> _Indent = new ThreadLocal<int>();

        /// <summary>
        ///     Indicates if calls are to be logged within a particular thread
        /// </summary>
        private static readonly ThreadLocal<bool> _TraceCalls = new ThreadLocal<bool>();

        /// <summary>
        ///     The lock object to prevent cross-thread access once we get to the actual logging
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        ///     The per thread auditor provider
        /// </summary>
        private static ThreadLocal<IAuditProvider> _Auditor;

        /// <summary>
        ///     Backing field for <see cref="Disabled" />
        /// </summary>
        private static bool _Disabled = false;

        /// <summary>
        ///     The per thread logger provider
        /// </summary>
        private static ThreadLocal<ILoggingProvider> _Logger;

        /// <summary>
        ///     The maximum number of loggers that can be chained together
        /// </summary>
        private static int _MaxChainedLoggers = 3;

        /// <summary>
        ///     Maintains the actual auditing provider
        /// </summary>
        private static IAuditProvider _OriginalAuditor;

        /// <summary>
        ///     Maintains the actual logging provider
        /// </summary>
        private static ILoggingProvider _OriginalLogger;

        /// <summary>
        ///     The root <see cref="IAuditProvider" />. Only the first auditor to be assigned will be accepted. Additional auditors
        ///     can be managed via the <see cref="NextAuditor" />property of your <see cref="IAuditProvider"/>. If you need to
        ///     replace the original auditor you will need to use <see cref="ReplaceAuditProvider" /> as subsequent settings via
        ///     <see cref="AuditProvider" /> will only affect the current thread.
        /// </summary>
        /// <remarks>
        ///     If you set to null, then currently the auditor on ALL threads will be nulled out. Basically this shouldn't be done,
        ///     but you MIGHT in a testing environment where you're using mocked loggers!
        /// </remarks>
        public static IAuditProvider AuditProvider
        {
            get
            {
                if (_OriginalAuditor == null)
                {
                    // No audit provider has been set, so by default we use ourself.
                    _Auditor = new ThreadLocal<IAuditProvider>();
                    _OriginalAuditor = new Logger();
                    _Auditor.Value = _OriginalAuditor;
                }
                else if (_Auditor.Value == null)
                {
                    // There is no audit provider set up for THIS thread, so get the original one to clone itself!
                    _Auditor.Value = (IAuditProvider) _OriginalAuditor.Clone();

                    var originalNext = _OriginalAuditor.NextAuditor;
                    var threadedNext = _Auditor.Value;
                    var numClones = MaxChainedLoggers;

                    // We also need to clone any NextAuditors!
                    while (originalNext != null && numClones-- > 0)
                    {
                        threadedNext.NextAuditor = (IAuditProvider) originalNext.Clone();
                        originalNext = originalNext.NextAuditor;
                    }
                }

                return _Auditor.Value;
            }

            set
            {
                if (value == null && _Auditor != null && _Auditor.Value != null)
                {
                    // We need to allow for the provider to be nulled out. Essentially only for use in unit tests
                    // where we might be expecting instantiated loggers, generally mocked, to record certain behaviours.
                    _Auditor.Value = null;
                }
                else if (_Auditor == null && value != null)
                {
                    // Otherwise, only the first provider to get in determines the log provider to use.
                    _Auditor = new ThreadLocal<IAuditProvider>();
                    _OriginalAuditor = value;
                    _Auditor.Value = value;
                }
                else if (_Auditor != null && (_Auditor.Value == null) & (value != null))
                {
                    // A specific auditor is being set for this thread. This one WON'T be cloned, but the setter WILL be respected
                    _Auditor.Value = value;
                }
            }
        }

        /// <summary>
        ///     Determines if logging is enabled or not. A way to remove MOST of the small overhead of logging without
        ///     removing/commenting out logging statements.
        /// </summary>
        /// If Disabled, each log statement will exit ASAP.
        /// If not Disabled, logging will behave as normal.
        public static bool Disabled
        {
            get => _Disabled;

            set => _Disabled = value;
        }

        /// <summary>
        ///     Disabled works better as a property internally, Enabled works better externally. It's all the same thing!
        /// </summary>
        public static bool Enabled
        {
            get => !Disabled;

            set => Disabled = !value;
        }

        /// <summary>
        ///     Indicates if the name of the method that calls one of the logging methods (Debug, Info, Warning, Error or Fatal)
        ///     will be displayed as a prefix in the log message.
        ///     <para>
        ///         NOTE: This will only be the method name, and will not include the class or namespace!
        ///     </para>
        /// </summary>
        public static bool LogMethodName { get; set; }

        /// <summary>
        ///     The root <see cref="ILoggingProvider" />. Only the first logger to be assigned will be accepted. Additional loggers
        ///     can be managed via the <see cref="NextLogger" /> property of your <see cref="ILoggingProvider"/>. If you need to replace the original logger you will need to use
        ///     <see cref="ReplaceLoggingProvider" /> as subsequent settings via <see cref="LogProvider" /> will only affect the
        ///     current thread.
        /// </summary>
        /// <remarks>
        ///     If you set to null, then currently the logger on ALL threads will be nulled out. Basically this shouldn't be done,
        ///     but you MIGHT in a testing environment where you're using mocked auditors!
        /// </remarks>
        public static ILoggingProvider LogProvider
        {
            get
            {
                if (_OriginalLogger == null)
                {
                    // No log provider has been set, so by default we use ourself.
                    _Logger = new ThreadLocal<ILoggingProvider>();
                    _OriginalLogger = new Logger();
                    _Logger.Value = _OriginalLogger;

                    // Warning is the initial default setting
                    OutputLevel = ReportingLevel.Warning;
                }
                else if (_Logger.Value == null)
                {
                    // There is no log provider set up for THIS thread, so get the original one to clone itself!
                    _Logger.Value = (ILoggingProvider) _OriginalLogger.Clone();

                    var originalNext = _OriginalLogger.NextLogger;
                    var threadedNext = _Logger.Value;
                    var numClones = MaxChainedLoggers;

                    // We also need to clone any NextLoggers!
                    while (originalNext != null && numClones-- > 0)
                    {
                        threadedNext.NextLogger = (ILoggingProvider) originalNext.Clone();
                        originalNext = originalNext.NextLogger;
                    }
                }

                return _Logger.Value;
            }

            set
            {
                if (value == null && _Logger != null && _Logger.Value != null)
                {
                    // We need to allow for the provider to be nulled out. Essentially only for use in unit tests
                    // where we might be expecting instantiated loggers, generally mocked, to record certain behaviours.
                    _Logger.Value = null;

                    // And just for completeness and normal expectations, we will disable logging. If you want it enabled AFTER 
                    // you've nulled this out you'll need to be specific about enabling it again. It'll get enabled anyway if you 
                    // set a new LogProvider.
                    Disabled = true;
                }
                else if (_Logger == null && value != null)
                {
                    // Otherwise, only the first provider to get in determines the log provider to use.
                    _Logger = new ThreadLocal<ILoggingProvider>();
                    _OriginalLogger = value;
                    _Logger.Value = value;

                    // Warning is the initial default setting
                    OutputLevel = ReportingLevel.Warning;

                    // Since the log provider has been set, let's put a seperator line in!
                    Seperate('=');

                    // We default to disabled until given a log provider.
                    Enabled = true;
                }
                else if (_Logger != null && (_Logger.Value == null) & (value != null))
                {
                    // A specific logger is being set for this thread. This one WON'T be cloned, but the setter WILL be respected
                    _Logger.Value = value;
                }
            }
        }

        /// <summary>
        ///     Let's you access the main thread's original log provider. Use this sparingly. It doesn't offer you the static
        ///     convenience functions but IF you need to get this, then you can!
        /// </summary>
        public static ILoggingProvider MainLogProvider => _OriginalLogger;

        /// <summary>
        ///     The maximum number of loggers in the NextLogger logging chain - Defaults to 3. You need a seriously good reason to
        ///     change this! Also stops NextLogger loops.
        /// </summary>
        public static int MaxChainedLoggers
        {
            get => _MaxChainedLoggers;

            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(MaxChainedLoggers), "MaxChainedLoggers must be > 0");

                _MaxChainedLoggers = value;
            }
        }

        /// <summary>
        ///     Gets or sets the current logging output level
        /// </summary>
        public static ReportingLevel OutputLevel { get; set; }

        /// <summary>
        ///     Allows stack tracing to be switched on and off - as in supressing or allowing the output of
        ///     <see cref="Logger.TraceStack" />
        /// </summary>
        public static bool StackTracingEnabled { get; set; }

        /// <summary>
        ///     Determines if we should use Console output in addition to the LogProvider. Needs to be set AFTER the LogProvider
        ///     has been determined.
        /// </summary>
        public static bool UseConsoleOutput { get; set; }

        /// <summary>
        ///     Determines if any context is going to be printed in front of messages. If False, any scope WILL be logged at the
        ///     point of the scope and indentation will be used instead.
        /// </summary>
        public static bool UseContext { get; set; } = true;

        /// <summary>
        ///     Determines if the log message is to be prefixed with the thread id.
        /// </summary>
        public static bool UseThreadId { get; set; }

        /// <summary>
        ///     The next auditor to pass the audit message on to. Allows additional auditors to be used. Don't create circular
        ///     links though eh!
        /// </summary>
        public IAuditProvider NextAuditor { get; set; }

        /// <summary>
        ///     Audits the message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId"/> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        public void AuditThis(string msg, string context, string threadId, LogTag logTag)
        {
            if (UseConsoleOutput) Console.Out.WriteLine("AUDIT: " + msg);
        }

        /// <summary>
        ///     Audits an object. Can be used IF a specific object is to be audited by an implementation rather than simply a
        ///     string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="auditObject">The object to audit - it will be JSONd</param>
        /// <param name="auditLevel">The level of audit</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId"/> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AuditThisObject(string message, object auditObject, LoggingLevel auditLevel, string context,
            string threadId, LogTag logTag)
        {
            // The basic implementation simply audits the Json version
            var auditMsg = auditLevel == LoggingLevel.Audit
                ? message
                : auditLevel + $" {message}\r\n{JsonIt(auditObject)}";
            AuditThis(auditMsg, context, threadId, logTag);
        }

        /// <summary>
        ///     The next logger to pass the log message on to. Allows additional loggers to be used. Don't create circular links
        ///     though eh!
        /// </summary>
        public ILoggingProvider NextLogger { get; set; }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <remarks>
        ///     As a user of the library you don't need to worry about this method. It is used when creating a logger per
        ///     thread which means you can ALWAYS use Logger.Xxx and be sure that you have a thread-aware-safe logger for your
        ///     current thread.
        /// </remarks>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            // We can happily manage all of the different threads!
            return this;
        }

        /// <summary>
        /// Logs an error message to the console.
        /// <para>
        /// As a default ILoggingProvider, this is the default console logging if no other logger is provided.
        /// </para>
        /// </summary>
        /// <param name="level">The level for this message</param>
        /// <param name="context">The context.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId" /> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        /// <param name="msg">The message to log</param>
        public void LogThis(LoggingLevel level, string context, string threadId, LogTag logTag, string msg)
        {
            UseConsoleOutput = true;
        }

        /// <summary>A one-shot initialisation using the specified configuration.</summary>
        /// <param name="config">The configuration.</param>
        /// <param name="loggingProvider">The logging provider.</param>
        /// <param name="auditProvider">
        ///     The audit provider, defaults to NULL - which means an calls to <see cref="Audit" /> will be
        ///     logged as an Audit message.
        /// </param>
        /// <remarks>
        ///     Each of the properties in the config can be set individually, this just makes that initialisation a bit more
        ///     compact, especially if you've read the config as an object from a settings file.
        /// </remarks>
        public static void Initialise(Config config, ILoggingProvider logProvider,
            IAuditProvider auditProvider = null)
        {
            if (logProvider != null)
            {
                // Otherwise setting it to null means the logprovider won't get default to the Logger itself -
                // which we might want for simple console logging
                LogProvider = logProvider;
            }

            AuditProvider = auditProvider;
            LogMethodName = config.LogMethodName;
            OutputLevel = config.OutputLevel;
            StackTracingEnabled = config.StackTracingEnabled;
            UseConsoleOutput = config.UseConsoleOutput;
            UseContext = config.UseContext;
            UseThreadId = config.UseThreadId;

            if (config.LogTags != null && config.LogTags.Count > 0)
            {
                ActivateLogTags(null);
                ActivateLogTags(config.LogTags);
            }
        }

        /// <summary>
        ///     Activates the log tag by adding it to the list of those already active.
        /// </summary>
        /// <param name="tag">The tag - null not allowed!.</param>
        /// <exception cref="System.ArgumentNullException">tagName</exception>
        public static void ActivateLogTag(LogTag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            LogTag.ActivateLogTag(tag);
        }

        /// <summary>
        ///     Activates the log tags by adding the supplied tags to those already active. If you want to reset the list, pass a
        ///     null so you can start again.
        /// </summary>
        /// <param name="tagNames">The tag names or null to reset the list.</param>
        public static void ActivateLogTags(IEnumerable<string> tagNames)
        {
            LogTag.ActivateLogTags(tagNames);
        }

        /// <summary>
        ///     Deactivates the log tag by removing it from the list of those already active.
        /// </summary>
        /// <param name="tag">The tag - null not allowed!.</param>
        /// <exception cref="System.ArgumentNullException">tagName</exception>
        public static void DeactivateLogTag(LogTag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));

            LogTag.DeactivateLogTag(tag);
        }

        /// <summary>
        ///     Deactivates the log tags by removing the supplied tags from those already active.
        /// </summary>
        /// <param name="tagNames">The tag names</param>
        public static void DeactivateLogTags(IEnumerable<string> tagNames)
        {
            LogTag.DeactivateLogTags(tagNames);
        }

        /// <summary>
        ///     Converts any number of args into an array of objects. Useful from time to time!
        /// </summary>
        /// <param name="args">As many whatevers as you want</param>
        /// <returns>The args as an object array</returns>
        public static object[] Args(params object[] args)
        {
            return args;
        }

        /// <summary>
        ///     Logs an object in JSON format as a <see cref="LoggingLevel.Audit" />. It also passes the object to the
        ///     <see cref="IAuditProvider" /> which may wish to audit the actual object.
        ///     <remarks>
        ///         Note, this override of Audit will be used unless a more specific override is used. i.e. It will not JSON a
        ///         String!
        ///         Audit messages are log messages with a level of Audit. Audit is the highest level log message and will ALWAYS
        ///         be logged, even if Logging has been disabled. Different <see cref="ILoggingProvider" />s MAY decide to also
        ///         treat Audit messages differently, however in addition you CAN also supply an <see cref="IAuditProvider" /> (or
        ///         chain of them) which will only be passed Audit messages via <see cref="IAuditProvider.AuditThis" />.
        ///     </remarks>
        ///     <para>
        ///         Also you don't need to log AND audit as the audit message will be logged and if you supply an auditLevel
        ///         other than <see cref="LoggingLevel.Audit" /> then the message will be prefixed with that level. So, you can
        ///         audit errors and info. Unlikely you'd want to audit Debug but you could!
        ///     </para>
        /// </summary>
        /// <param name="message">The message to accompany the audit</param>
        /// <param name="anything">The object to be logged using JSON</param>
        /// <param name="logTag">
        ///     The log tag. Unlike for Logging, auditing will always audit so the logTag can be used for extra
        ///     information by the <see cref="IAuditProvider" /> where required.
        /// </param>
        /// <param name="auditLevel">The audit level.</param>
        /// <param name="caller">The name of the caller of this Audit.</param>
        public static void Audit(string message, object anything, LogTag logTag = null,
            LoggingLevel auditLevel = LoggingLevel.Audit, [CallerMemberName] string caller = null)
        {
            try
            {
                var auditMsg = auditLevel == LoggingLevel.Audit
                    ? message
                    : auditLevel + $" {message}\r\n{JsonIt(anything)}";

                LogMsgCommon(LoggingLevel.Audit, auditMsg, logTag, caller);

                var nextAuditor = AuditProvider;

                var maxAuditors = MaxChainedLoggers;

                while (nextAuditor != null && maxAuditors-- > 0)
                {
                    nextAuditor.AuditThisObject(message, anything, auditLevel, ScopeContext.CurrentValue, $"{Thread.CurrentThread.ManagedThreadId}", logTag);
                    nextAuditor = nextAuditor.NextAuditor;
                }
            }
            catch (Exception excep)
            {
                Error(excep, $"Error auditing the following object [{JsonIt(anything)}]");

                // Since this was an audit we MUST propogate the exception so that the caller is aware!
                throw;
            }
        }

        /// <summary>
        ///     Starts a new nested scoped threaded context.
        ///     <para>
        ///         Usage:
        ///     </para>
        ///     <para>
        ///         using (Logger.Context()){code} or Logger.Context("whatever")
        ///     </para>
        ///     <para>
        ///         Logger.UseContext = true will then prepend all messages within context with "My Method : ". Context is scoped
        ///         and nested. Logger.UseContext = false (default) will log the context and indent messages, but not include the
        ///         context before every message!
        ///     </para>
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logTagScope">The log tag scope - let's you also incorporate a log tag scope.</param>
        /// <param name="callerContext">The caller context - defaults to the name of the caller</param>
        /// <returns>A <see cref="ScopeContext" /> that SHOULD be used in a using statement</returns>
        public static ScopeContext Context(string context = null, LogTag logTagScope = null,
            [CallerMemberName] string callerContext = "")
        {
            if (context == null) context = callerContext;

            return new ScopeContext(context, logTagScope);
        }

        /// <summary>
        ///     Starts a new nested scoped threaded context.
        ///     <para>
        ///         Usage:
        ///     </para>
        ///     <para>
        ///         using (Logger.Context()){code} or Logger.Context("whatever")
        ///     </para>
        ///     <para>
        ///         Logger.UseContext = true will then prepend all messages within context with "My Method : ". Context is scoped
        ///         and nested. Logger.UseContext = false (default) will log the context and indent messages, but not include the
        ///         context before every message!
        ///     </para>
        /// </summary>
        /// <param name="logTagScope">The log tag scope - let's you also incorporate a log tag scope.</param>
        /// <param name="callerContext">The caller context - defaults to the name of the caller</param>
        /// <returns>A <see cref="ScopeContext" /> that SHOULD be used in a using statement</returns>
        public static ScopeContext Context(LogTag logTagScope, [CallerMemberName] string callerContext = "")
        {
            return new ScopeContext(callerContext, logTagScope);
        }

        /// <summary>
        ///     Starts a new nested scoped threaded context.
        ///     <para>
        ///         Usage:
        ///     </para>
        ///     <para>
        ///         using (Logger.Context()){code} or Logger.Context("whatever")
        ///     </para>
        ///     <para>
        ///         Logger.UseContext = true will then prepend all messages within context with "My Method : ". Context is scoped
        ///         and nested. Logger.UseContext = false (default) will log the context and indent messages, but not include the
        ///         context before every message!
        ///     </para>
        ///     Utilise Logger.<seealso cref="Args" />(args,..) to pass arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="logTagScope">The log tag scope - let's you also incorporate a log tag scope.</param>
        /// <param name="context">The context - defaults to the name of the caller.</param>
        /// <returns>
        ///     A <see cref="ScopeContext" /> that SHOULD be used in a using statement
        /// </returns>
        public static ScopeContext Context(object[] arguments, LogTag logTagScope = null,
            [CallerMemberName] string context = "")
        {
            ScopeContext retVal;
            retVal = new ScopeContext(context, arguments, logTagScope);

            return retVal;
        }


        /// <summary>
        ///     Starts a new nested scoped log tag.
        ///     <para>
        ///         Usage:
        ///     </para>
        ///     <para>
        ///         using (Logger.ScopedLogTag()){code} or Logger.ScopedLogTag("whatever"){code}
        ///     </para>
        /// </summary>
        /// <param name="logTagScope">
        ///     The log tag to scope - if null, a LogTag will be created of the same name as the caller's
        ///     name - Not really recommended though.
        /// </param>
        /// <param name="callerContext">The caller context - defaults to the name of the caller</param>
        /// <returns>A <see cref="LogTag.Scoped" /> that SHOULD be used in a using statement</returns>
        public static LogTag.Scoped ScopedLogTag(LogTag logTagScope = null,
            [CallerMemberName] string callerContext = "")
        {
            if (logTagScope == null) logTagScope = new LogTag(callerContext);

            return new LogTag.Scoped(logTagScope);
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Debug" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Debug(string msg, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Debug, msg, logTag, caller);
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Trace" />.
        ///     <para>
        ///         Try not to use this too often for DETAIL, use Debug in preference UNLESS YOU NEED LOTS OF DETAIL
        ///     </para>
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Trace(string msg, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Trace, msg, logTag, caller);
        }

        /// <summary>
        ///     Logs any object using JSON as a <see cref="LoggingLevel.Debug" />. This will log the field name and value of the
        ///     object. IF YOU CALL THIS MAKE SURE YOU
        ///     HAVE A REFERENCE TO NewtonSoft.Json
        /// </summary>
        /// Note, this override of Log will be used unless a more specific override is used. i.e. It will not JSON an Exception or a String!
        /// <param name="anything">The object to be logged using JSON</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Debug(object anything, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Debug, JsonIt(anything), logTag, caller);
        }

        /// <summary>
        ///     Logs any object using JSON as a <see cref="LoggingLevel.Trace" />. This will log the field name and value of the
        ///     object.
        ///     <para>
        ///         Try not to use this too often for DETAIL, use Debug in preference UNLESS YOU NEED LOTS OF DETAIL
        ///     </para>
        ///     <para>
        ///         IF YOU CALL THIS MAKE SURE YOU HAVE A REFERENCE TO NewtonSoft.Json
        ///     </para>
        /// </summary>
        /// Note, this override of Log will be used unless a more specific override is used. i.e. It will not JSON an Exception or a String!
        /// <param name="anything">The object to be logged using JSON</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Trace(object anything, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Trace, JsonIt(anything), logTag, caller);
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Error" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        public static void Error(string msg, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Error, msg, null, caller);
        }

        /// <summary>
        ///     Logs the specified object as a JSON string, as a <see cref="LoggingLevel.Error" />.
        /// </summary>
        /// <param name="thing">The thing to log.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        public static void Error(object thing, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Error, JsonIt(thing), null, caller);
        }

        /// <summary>
        ///     Logs an exception as a <see cref="LoggingLevel.Error" />.
        /// </summary>
        /// <param name="excep">The exception whose message and stack trace will be logged.</param>
        /// <param name="msg">The message to put in front of the exception message.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        /// Note however that the stack trace will be printed the first time this exception is logged.
        /// If it is logged subsequently, simply the message will be logged, not the stack trace.
        /// The HelpLink field (often unused) is used to flag this. If it is used, then you'll always get
        /// the stack trace.
        public static void Error(Exception excep, string msg = "", [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            if (string.IsNullOrEmpty(excep.HelpLink))
            {
                LogMsgCommon(LoggingLevel.Error,
                    msg + "\r\n\t" + GetFullExceptionMessage(excep) + "\r\nStack Trace:\r\n\t" + excep.StackTrace,
                    null, caller);

                excep.HelpLink = "Handled";
            }
            else if (excep.HelpLink == "Handled")
            {
                LogMsgCommon(LoggingLevel.Error, msg + "\r\n" + "Exception propogated;\r\n\t" + excep.Message, null,
                    caller);
            }
            else
            {
                LogMsgCommon(LoggingLevel.Error, msg + "\r\n\t" + excep.Message + "\r\n\t" + excep.StackTrace, null,
                    caller);
            }
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Fatal" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        public static void Fatal(string msg, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Fatal, msg, null, caller);
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Critical" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        public static void Critical(string msg, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Critical, msg, null, caller);
        }

        /// <summary>
        ///     Gets the argument values as a string
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>The string representing the argument values</returns>
        public static string GetArgVals(object[] arguments)
        {
            var retVal = string.Empty;

            // Check if any argument values were passed or not.
            if (arguments != null && arguments.Length > 0)
                for (var i = 0;
                    i <
                    arguments.Length;
                    i++)
                {
                    string value;

                    if (arguments[i]?.GetType() == typeof(string))
                        value = string.Format("\"{0}\"", arguments[i]);
                    else if (arguments[i] != null && arguments[i].GetType().GetTypeInfo().IsPrimitive)
                        value = arguments[i].ToString();
                    else
                        value = JsonIt(arguments[i]);

                    retVal += $"{value}{(i < arguments.Length - 1 ? ", " : string.Empty)}";
                }

            return retVal;
        }

        /// <summary>
        ///     Recursively gets the exception message with any inner exceptions tacked on.
        /// </summary>
        /// <param name="excep">An exception object</param>
        /// <returns>A full exception message, including inners</returns>
        public static string GetFullExceptionMessage(Exception excep)
        {
            var retVal = excep.Message;

            if (excep.InnerException != null)
                retVal += "\nInner Exception:\n\t" + GetFullExceptionMessage(excep.InnerException);

            return retVal;
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Information" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Info(string msg, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Information, msg, logTag, caller);
        }

        /// <summary>
        ///     Logs any object using JSON as a <see cref="LoggingLevel.Information" />. This will log the field name and value of
        ///     the object. IF YOU CALL THIS MAKE SURE YOU
        ///     HAVE A REFERENCE TO NewtonSoft.Json
        /// </summary>
        /// Note, this override of Log will be used unless a more specific override is used. i.e. It will not JSON an Exception or a String!
        /// <param name="anything">The object to be logged using JSON</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Info(object anything, LogTag logTag = null, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Information, JsonIt(anything), logTag, caller);
        }

        /// <summary>
        ///     Gets an object in JSON format, prefixed by the name of the type of the object. IF YOU CALL THIS MAKE SURE YOU HAVE
        ///     A REFERENCE TO NewtonSoft.Json. Do NOT use this to Json serialise things!
        /// </summary>
        /// <param name="anything">Some object</param>
        /// <returns>
        ///     It's JSON equivalent - Deeply nested/recursive objects are not always good candidates, nor those with certain
        ///     attributes, but generally this works quite well.
        /// </returns>
        public static string JsonIt(object anything)
        {
            var retVal = string.Empty;

            try
            {
                if (anything != null)
                {
                    var settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling =
                            PreserveReferencesHandling.None,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include
                    };
                    retVal = string.Format("{0}:{1}", anything.GetType().FullName,
                        JsonConvert.SerializeObject(anything, settings));
                }
                else
                {
                    retVal = "[Null object]";
                }
            }
            catch (Exception excep)
            {
                Error(excep);
            }

            return retVal;
        }

        /// <summary>
        ///     Allows use of a different log provider within a using scope
        /// </summary>
        /// <param name="localProvider">The local provider.</param>
        /// <returns>A new <see cref="LocalLogger" /></returns>
        public static LocalLogger Local(ILoggingProvider localProvider)
        {
            return new LocalLogger(localProvider);
        }

        /// <summary>
        ///     Logs the active log tags, will always get logged out.
        /// </summary>
        public static void LogActiveLogTags()
        {
            if (Disabled) return;

            Title("Active Log Tags:");
            LogMsgCommon((LoggingLevel) OutputLevel, string.Join(", ", LogTag.ActiveLogTags));
        }

        /// <summary>
        ///     Logs the encountered log tags, will always get logged out. Use this to get a handle on what log tags are available
        ///     - up to this point in your program execution.
        /// </summary>
        public static void LogEncounteredLogTags()
        {
            if (Disabled) return;

            Title("Encountered Log Tags:");
            var encountered = LogTag.EncounteredLogTags.ToList();
            encountered.Sort();
            LogMsgCommon((LoggingLevel) OutputLevel, JsonIt(encountered));
        }

        /// <summary>
        ///     Logs (at Debug level) a method's details with parameter and return types. If you want to log parameter values as
        ///     well then pass your params in the same order as declared.
        /// </summary>
        /// <param name="arguments">
        ///     The arguments in the same order as declared in your method. Strings and other primitives are
        ///     'stringed', other types are JSON'd
        /// </param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="methodName">Defaults to caller name</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void MethodInfo(object[] arguments = null, LogTag logTag = null,
            [CallerMemberName] string methodName = "")
        {
            if (Disabled || OutputLevel > ReportingLevel.Debug) return;

            var argVals = GetArgVals(arguments);
            var msg = $"{methodName}({argVals})";

            Debug(msg, logTag);
        }

        /// <summary>
        ///     Prints a message to the console output and also logs it as an Info level message. Great for keeping track of
        ///     messages that have been printed to the console!
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Print(string msg)
        {
            // We don't want to double up any console output so temporarily switch this off
            var currentValue = UseConsoleOutput;
            UseConsoleOutput = false;
            Console.Out.WriteLine(msg);
            Info($"Console: {msg}");

            // then back to what it was!
            UseConsoleOutput = currentValue;
        }

        /// <summary>
        ///     Replaces the audit provider - across the board, not just for the calling thread.
        /// </summary>
        /// <param name="newOne">The new one.</param>
        public static void ReplaceAuditProvider(IAuditProvider newOne)
        {
            _Auditor = null;
            AuditProvider = newOne;
        }

        /// <summary>
        ///     Replaces the logging provider - across the board, not just for the calling thread.
        /// </summary>
        /// <param name="newOne">The new one.</param>
        public static void ReplaceLoggingProvider(ILoggingProvider newOne)
        {
            _Logger = null;
            LogProvider = newOne;
        }

        /// <summary>
        ///     Puts a separating line into the log output
        /// </summary>
        /// <param name="use">The separator character to use.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Seperate(char use = '-')
        {
            if (Disabled) return;

            var line = string.Empty;
            line = line.PadLeft(80, use);

            // Use whatever output level is in place as we want this going out
            LogMsgCommon((LoggingLevel) OutputLevel, string.Format("{0}{1}", line, Environment.NewLine));
        }

        /// <summary>
        ///     Outputs a "----------- side lined ----------------------" string to the logger. This will always come out!
        /// </summary>
        /// <param name="msg">The message to appear "titled" - Can have argument placeholders!</param>
        /// <param name="args">Any arguments.</param>
        /// The whole line will be 79/80 chars total with the message centered in the ----- lines ------
        public static void Title(string msg, params object[] args)
        {
            if (Disabled) return;

            msg = string.Format(msg, args);
            var numDashes = (80 - msg.Length - 2) / 2;
            var title = string.Empty;
            var dashes = title.PadLeft(numDashes, '-');
            title = string.Format("{0}{1} {2} {1}", Environment.NewLine, dashes, msg);

            // Use whatever output level is in place as we want this going out
            LogMsgCommon((LoggingLevel) OutputLevel, title);
        }

        /// <summary>
        ///     Spits a stack trace out to the log.
        /// </summary>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void TraceStack()
        {
            if (Disabled) return;

            if (StackTracingEnabled)
            {
                StackTrace stackTrace = new StackTrace(); // get call stack
                StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

                // write call stack method names
                string stackList = "Stack Trace:\n  ";

                foreach (StackFrame stackFrame in stackFrames)
                    stackList += stackFrame.GetMethod().DeclaringType + "." + stackFrame.GetMethod().Name + "-->";

                // Stack traces will only ever go out at Debug level
                LogMsgCommon(LoggingLevel.Debug, stackList);
            }
        }

        /// <summary>
        ///     Logs the specified message as a <see cref="LoggingLevel.Warning" />.
        /// </summary>
        /// <param name="msg">The string to log.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Warning(string msg, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Warning, msg, null, caller);
        }

        /// <summary>
        ///     Logs any object using JSON as a <see cref="LoggingLevel.Warning" />. This will log the field name and value of the
        ///     object. IF YOU CALL THIS MAKE SURE YOU
        ///     HAVE A REFERENCE TO NewtonSoft.Json
        /// </summary>
        /// Note, this override of Log will be used unless a more specific override is used. i.e. It will not JSON an Exception or a String!
        /// <param name="anything">The object to be logged using JSON</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        [SuppressMessage(
            "StyleCop.CSharp.LayoutRules",
            "SA1503:CurlyBracketsMustNotBeOmitted",
            Justification = "I excuse exceptions and returns!")]
        public static void Warning(object anything, [CallerMemberName] string caller = null)
        {
            if (Disabled) return;

            LogMsgCommon(LoggingLevel.Warning, JsonIt(anything), null, caller);
        }

        /// <summary>
        /// Flush any buffers currently in use.
        /// </summary>
        public void FlushBuffers()
        {
            // Nothing to do here!
        }

        /// <summary>
        /// Ensures all loggers get flushed.
        /// </summary>
        public static void Flush()
        {
            ILoggingProvider nextLogger = LogProvider;

            int maxLoggers = MaxChainedLoggers;

            while (nextLogger != null && maxLoggers-- > 0)
            {
                // Ensure that there is a lock on the log provider!
                lock (LockObject)
                {
                    nextLogger.FlushBuffers();
                }

                nextLogger = nextLogger.NextLogger;
            }

            IAuditProvider nextAuditProvider = AuditProvider;

            maxLoggers = MaxChainedLoggers;

            while (nextAuditProvider != null && maxLoggers-- > 0)
            {
                // Ensure that there is a lock on the log provider!
                lock (LockObject)
                {
                    nextAuditProvider.FlushBuffers();
                }

                nextAuditProvider = nextAuditProvider.NextAuditor;
            }

        }

        /// <summary>
        ///     Another single point to call the log provider
        /// </summary>
        /// <param name="level">The level for this message</param>
        /// <param name="msg">Message to log.</param>
        /// <param name="logTag">An optional log tag.</param>
        /// <param name="caller">The method name of the caller. Will be displayed if <seealso cref="LogMethodName" /> is true.</param>
        private static void LogMsgCommon(LoggingLevel level, string msg, LogTag logTag = null, string caller = null)
        {
            if (logTag == null && level <= LoggingLevel.Information)
                // If there isn't a log tag, check if a scope one has been used.
                logTag = LogTag.Scoped.CurrentValue;

            if (level > LoggingLevel.Information)
                // We ignore log tags on anything above Information!
                logTag = null;

            if (OutputLevel <= (ReportingLevel) level && (logTag == null || logTag.IsActive()))
            {
                if (LogMethodName && !string.IsNullOrEmpty(caller)) msg = $"{caller}(): {msg}";

                if (!UseContext && (_TraceCalls.Value || ScopeContext.CurrentValue != null))
                    msg = msg.PadLeft(msg.Length + _Indent.Value);

                ILoggingProvider nextLogger = LogProvider;

                int maxLoggers = MaxChainedLoggers;

                while (nextLogger != null && maxLoggers-- > 0)
                {
                    // Ensure that there is a lock on the log provider!
                    lock (LockObject)
                    {
                        nextLogger.LogThis(level, UseContext ? ScopeContext.CurrentValue : string.Empty, UseThreadId ? $"{Thread.CurrentThread.ManagedThreadId}" : string.Empty, logTag, msg);
                    }

                    nextLogger = nextLogger.NextLogger;
                }

                if (UseConsoleOutput) Console.Out.WriteLine(msg);
            }
        }

        /// <summary>
        ///     Configuration data so that the Logger's static properties can be set in one hit, typically from a settings file.
        /// </summary>
        public class Config
        {
            /// <summary>
            ///     Indicates if the name of the method that calls one of the logging methods (Debug, Info, Warning, Error or Fatal)
            ///     will be displayed as a prefix in the log message.
            ///     <para>
            ///         NOTE: This will only be the method name, and will not include the class or namespace!
            ///     </para>
            /// </summary>
            public bool LogMethodName { get; set; }

            /// <summary>The set of log tags to initially activate. See also <seealso cref="Logger.ActivateLogTags"/></summary>
            public List<String> LogTags { get; set; }

            /// <summary>
            ///     Gets or sets the current logging output level
            /// </summary>
            public ReportingLevel OutputLevel { get; set; } = ReportingLevel.Information;

            /// <summary>
            ///     Allows stack tracing to be switched on and off - as in supressing or allowing the output of
            ///     <see cref="Logger.TraceStack" />
            /// </summary>
            public bool StackTracingEnabled { get; set; }

            /// <summary>
            ///     Determines if we should use Console output in addition to the LogProvider. Needs to be set AFTER the LogProvider
            ///     has been determined.
            /// </summary>
            public bool UseConsoleOutput { get; set; }

            /// <summary>
            ///     Determines if any context is going to be printed in front of messages. If False, any scope WILL be logged at the
            ///     point of the scope and indentation will be used instead.
            /// </summary>
            public bool UseContext { get; set; }

            /// <summary>
            ///     Determines if the log message is to be prefixed with the thread id.
            /// </summary>
            public bool UseThreadId { get; set; }
        }

        /// <summary>
        ///     Allows a different <see cref="ILoggingProvider" /> to be used within a "local" using scope
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        public class LocalLogger : IDisposable
        {
            /// <summary>
            ///     The previous logging provider as we need to restore this thread's <see cref="ILoggingProvider" /> when we drop out
            ///     of the using.
            /// </summary>
            private readonly ILoggingProvider _previousLoggingProvider = LogProvider;

            /// <summary>
            ///     Initializes a new instance of the <see cref="LocalLogger" /> class.
            /// </summary>
            /// <param name="localLoggingProvider">The logging provider that is to be temporarily used within the using scope.</param>
            public LocalLogger(ILoggingProvider localLoggingProvider)
            {
                LogProvider = localLoggingProvider;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. In this
            ///     case, restoring the <see cref="ILoggingProvider" /> that WAS in use.
            /// </summary>
            public void Dispose()
            {
                LogProvider = _previousLoggingProvider;
            }
        }
    }
}
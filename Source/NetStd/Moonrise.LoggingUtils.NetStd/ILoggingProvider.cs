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
namespace Moonrise.Logging
{
    /// <summary>
    ///     Indicates the level of logging
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        ///     Use debug messages for intricate detail that you only really need when digging deep into a problem.
        /// </summary>
        Debug = 0,

        /// <summary>
        ///     Use information messages for mundane but USEFUL information.
        /// </summary>
        Information,

        /// <summary>
        ///     Warnings are for when things aren't quite right and someone should pay attention - but not an actual error
        /// </summary>
        Warning,

        /// <summary>
        ///     Errors are for things that are wrong, unexpected exceptions and checked for, i.e. anticipated errors
        /// </summary>
        Error,

        /// <summary>
        ///     Reserve fatal errors for when you're dumping out of an application!
        /// </summary>
        Fatal,

        /// <summary>
        ///     A special type of logging, audits can be extracted separately.
        /// </summary>
        Audit = 10
    }

    /// <summary>
    ///     Logging provider interface. Any given log provider needs to support these operations.
    /// </summary>
    public interface ILoggingProvider : ICloneable
    {
        /// <summary>
        ///     The next logger to pass the log message on to. Allows additional loggers to be used. Don't create circular links
        ///     though eh!
        /// </summary>
        ILoggingProvider NextLogger { get; set; }

        /// <summary>
        /// Logs the appropriate level of message.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="context">The context.</param>
        /// <param name="logTag">The log tag.</param>
        /// <param name="msg">The message.</param>
        void LogThis(LoggingLevel level, string context, LogTag logTag, string msg);
    }
}

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
    ///     Audit provider interface. Any given audit provider needs to support these operations.
    /// </summary>
    public interface IAuditProvider : ICloneable
    {
        /// <summary>
        ///     The next auditor to pass the audit message on to. Allows additional auditors to be used. Don't create circular
        ///     links though eh!
        /// </summary>
        IAuditProvider NextAuditor { get; set; }

        /// <summary>
        /// Audits the message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId"/> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        void AuditThis(string msg, string context, string threadId, LogTag logTag);

        /// <summary>
        /// Audits an object. Can be used IF a specific object is to be audited by an implementation rather than simply a
        /// string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="auditObject">The audit object.</param>
        /// <param name="auditLevel">The audit level.</param>
        /// <param name="context">The context - if <see cref="Logger.UseContext" /> is false, this will be empty.</param>
        /// <param name="threadId">The thread identifier - if <see cref="Logger.UseThreadId"/> is false, this will be empty.</param>
        /// <param name="logTag">The log tag.</param>
        void AuditThisObject(string message, object auditObject, LoggingLevel auditLevel, string context, string threadId, LogTag logTag);

        /// <summary>
        /// Flush any buffers currently in use.
        /// </summary>
        void FlushBuffers();
    }
}

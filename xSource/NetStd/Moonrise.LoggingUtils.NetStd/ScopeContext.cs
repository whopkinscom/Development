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
using Moonrise.Logging.Util;

namespace Moonrise.Logging
{
    /// <summary>
    ///     Allows a context scope to be put into place for log messages.
    ///     <para>
    ///         Usage:
    ///     </para>
    ///     <para>
    ///         using (new Context("My Method")){code}
    ///     </para>
    ///     <para>
    ///         Logger.UseContext = true will then prepend all messages within context with "My Method : ". Context is scoped
    ///         and nested
    ///     </para>
    /// </summary>
    /// <seealso cref="string" />
    public class ScopeContext : ScopedNestableThreadGlobalSingleton<string>
    {
        private static readonly string spacer = "->";
        private readonly LogTag.Scoped logTagScope;
        private readonly string scopeName;

        /// <summary>
        ///  Constructs a <see cref="ScopeContext"/> for logging. Messages logged within this scope will either be indented or have the scope name prefixed. See also <seealso cref="Logger.UseContext"/>
        /// </summary>
        /// <param name="_scopeName">A name for the scope we are entering. It might be a method name, it might be something else to identify the scope your logging</param>
        /// <param name="logTag">The log tag to apply to this scope. Defaults to null, i.e. no log tag applied</param>
        public ScopeContext(string _scopeName, LogTag logTag = null) : base(
            $"{(string.IsNullOrEmpty(CurrentValue) ? string.Empty : CurrentValue)}{_scopeName}{spacer}")
        {
            scopeName = _scopeName ?? throw new ArgumentNullException(nameof(_scopeName), "There must be a value for context!");

            if (logTag != null)
            {
                logTagScope = new LogTag.Scoped(logTag);
            }

            // If we aren't putting the context in front of each message then we log the entry and exit points and increment and decrement the indent every time we enter and leave scope
            if (!Logger.UseContext)
            {
                Logger.Debug($"{scopeName}() - Entry");
                Logger._Indent.Value++;
            }
        }

        /// <summary>
        ///  Constructs a <see cref="ScopeContext"/> for logging. Messages logged within this scope will either be indented or have the scope name prefixed. See also <seealso cref="Logger.UseContext"/>
        /// </summary>
        /// <param name="_scopeName">A name for the scope we are entering. It might be a method name, it might be something else to identify the scope your logging</param>
        /// <param name="arguments">A list of values you want to be logged as the arguments - typically to your method</param>
        /// <param name="logTag">The log tag to apply to this scope. Defaults to null, i.e. no log tag applied</param>
        public ScopeContext(string _scopeName, object[] arguments, LogTag logTag = null) : base(
            $"{(string.IsNullOrEmpty(CurrentValue) ? string.Empty : CurrentValue)}{_scopeName}{spacer}")
        {
            scopeName = _scopeName ?? throw new ArgumentNullException(nameof(_scopeName), "There must be a value for context!");

            if (logTag != null)
            {
                logTagScope = new LogTag.Scoped(logTag);
            }

            // If we aren't putting the context in front of each message then we log the entry and exit points and increment and decrement the indent every time we enter and leave scope
            if (!Logger.UseContext)
            {
                string argVals = Logger.GetArgVals(arguments);
                Logger.Debug($"{scopeName}({argVals}) - Entry");
                Logger._Indent.Value++;
            }
        }

        /// <summary>
        /// Called when exiting a using scope. In this case, unwunds the scope context and the logging indent.
        /// </summary>
        protected override void Disposing()
        {
            // If we aren't putting the context in front of each message then we log the entry and exit points and increment and decrement the indent every time we enter and leave scope
            if (!Logger.UseContext)
            {
                Logger._Indent.Value--;

                if (Logger._Indent.Value < 0)
                {
                    Logger._Indent.Value = 0;
                }

                Logger.Debug($"{scopeName} - Exit");
            }

            if (logTagScope != null)
            {
                logTagScope.Dispose();
            }
        }
    }
}

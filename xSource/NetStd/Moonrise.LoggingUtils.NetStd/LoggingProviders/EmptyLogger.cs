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

// Disable the warning that there is no XML comments for this class as this is a simple empty logger!
#pragma warning disable 1591

namespace Moonrise.Logging.LoggingProviders
{
    /// <summary>
    ///     Can be used in combination with the technique below to "disable" logging without commenting out or removing
    ///     Logger.XXX calls
    ///     Put the next line as the "first" line (after file header).
    ///     <para>
    ///         #define _NoLogging // Comment this out if you want to restore the logging
    ///     </para>
    ///     <para>
    ///         Then inside the namespace of where you want to "remove" logging put these lines;
    ///     </para>
    ///     <para>
    ///         #if _NoLogging
    ///     </para>
    ///     <para>
    ///         using Logger = Moonrise.Utils.Standard.Loggingging.EmptyLogger;
    ///     </para>
    ///     <para>
    ///         #else
    ///     </para>
    ///     <para>
    ///         using Logger = Moonrise.Utils.Standard.Loggingging.Logger;
    ///     </para>
    ///     <para>
    ///         #endif
    ///     </para>
    ///     <para>
    ///         Then Logger.XXX will either resolve to the proper <see cref="Logger" /> or the <see cref="EmptyLogger" />.
    ///     </para>
    ///     <para>
    ///         However, do bear in mind that although <see cref="EmptyLogger" />'s methods are ... well ... empty, you can use
    ///         <see cref="Logger.Disabled" /> as that does a very efficient check at the start of each <see cref="Logger" />
    ///         method anyway!
    ///     </para>
    /// </summary>
    public class EmptyLogger
    {
        public static bool StackTracingEnabled { get; set; }

        public static bool UseConsoleOutput { get; set; }

        public static bool UseTraceOutput { get; set; }

        public static void Error(string msg, params object[] args) { }

        public static void Error(string msg) { }

        public static string JsonIt(object anything)
        {
            return Logger.JsonIt(anything);
        }

        public static void Log(string msg, params object[] args) { }

        public static void Log(string msg) { }

        public static void Log(Exception excep) { }

        public static void Log(object anything) { }

        public static void Seperate() { }

        public static void TraceStack() { }

        public static void Warning(string msg, params object[] args) { }

        public static void Warning(string msg) { }
    }
}

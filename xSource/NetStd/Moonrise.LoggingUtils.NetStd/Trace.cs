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

namespace Moonrise.Logging
{
    /// <summary>
    ///     .Net core doesn't support the Trace output - there is a new logging wrapper framework
    /// </summary>
    public static class Trace
    {
        /// <summary>
        ///     Writes the specified message simply to the console.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void Write(string msg)
        {
            Console.Write(msg);
        }

        /// <summary>
        ///     Writes the specified message simply to the console.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

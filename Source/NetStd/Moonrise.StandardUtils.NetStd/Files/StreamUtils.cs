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

using System.IO;

namespace Moonrise.Utils.Standard.Files
{
    /// <summary>
    ///     Methods to make using streams a little smoother!
    /// </summary>
    public class StreamUtils
    {
        /// <summary>
        ///     Generates a memory stream from string. Remember to use;
        ///     <para>
        ///         using (Stream strStream = StreamUtils.StringStream(string s)){...}
        ///     </para>
        /// </summary>
        /// <param name="s">The feed string.</param>
        /// <returns>A memory stream ready to rock.</returns>
        public static Stream StringStream(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
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
using System.Diagnostics;
using System.IO;
using Moonrise.Utils.Standard.Extensions;
#if DotNetCore
using Microsoft.Extensions.PlatformAbstractions;

#else
using System.Reflection;
#endif

namespace Moonrise.Utils.Standard.Files
{
    /// <summary>
    ///     Contains utility methods for working with files.
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        ///     Gets the application path.
        /// </summary>
        /// <returns>As above!</returns>
        public static string ApplicationPath()
        {
#if DotNetCore
            string retVal = PlatformServices.Default.Application.ApplicationBasePath;
#else
            string retVal = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
#endif
            return retVal;
        }

        /// <summary>
        ///     Gets the application name.
        /// </summary>
        /// <returns>As above!</returns>
        public static string ApplicationName()
        {
#if DotNetCore
            string retVal = PlatformServices.Default.Application.ApplicationName;
#else
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string retVal = Path.GetFileName(codeBase);
#endif
            return retVal;
        }

        /// <summary>
        /// Gets the parent directory of a specified filepath. To get the parent of a FOLDER path, the path needs to end with a separator!
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <returns>The parent folder, or null if already at the root of a drive!</returns>
        public static string GetParentDirectory(string filepath)
        {
            string retVal = null;
            string folder = Path.GetDirectoryName(filepath);
            string root = Path.GetPathRoot(filepath);

            if (!folder.Equals(root))
            {
                retVal = Path.GetDirectoryName(folder);
            }

            return retVal;
        }

        /// <summary>
        ///     Reads a complete file as a string.
        /// </summary>
        /// <param name="dir">The directory for the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Null if the directory/file could not be found OR the contents of the file</returns>
        public static string ReadFile(string dir, string fileName)
        {
            string retVal = null;
            string filePath = Path.Combine(dir, fileName);
            retVal = ReadFile(filePath);
            return retVal;
        }

        /// <summary>
        ///     Reads a complete file as a string.
        /// </summary>
        /// <param name="filePath">The full file path.</param>
        /// <returns>Null if the directory/file could not be found OR the contents of the file</returns>
        public static string ReadFile(string filePath)
        {
            string retVal = null;

            try
            {
                retVal = File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                // Swallow any reading errors and allow a null string to return
            }

            return retVal;
        }

        /// <summary>
        ///     Gets the user application path.
        ///     Note: Automatically strips any trailing ".vshost"!
        /// </summary>
        /// <param name="defaultCompanyName">If the company name cannot be worked out</param>
        /// <returns>As above!</returns>
        public static string RoamingUserApplicationPath(string defaultCompanyName = "Ghosted")
        {
            string retVal = string.Empty;
            string appName = Process.GetCurrentProcess().ProcessName;
            string companyName = string.Empty;

#if !DotNetCore
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly != null)
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(entryAssembly.Location);
                companyName = versionInfo.CompanyName;
            }
#endif

            if (string.IsNullOrWhiteSpace(companyName))
            {
                companyName = defaultCompanyName;
            }

            // Look for the special case where the app is running under VisualStudio debug and stip off any trailing ".vshost"
            if (appName.EndsWith(".vshost"))
            {
                int pos = 0;
                appName = appName.Extract(ref pos, ".vshost");
            }

            retVal = $"{Environment.GetEnvironmentVariable("AppData")}/{companyName}/{appName}";
            return retVal;
        }

        /// <summary>
        ///     Overwrites a complete string into a file.
        /// </summary>
        /// <param name="dir">The directory for the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="content">The content to write.</param>
        public static void WriteFile(string dir, string fileName, string content)
        {
            // If the directory doesn't exist, try to create it
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string filePath = Path.Combine(dir, fileName);
            File.WriteAllText(filePath, content);
        }
    }
}

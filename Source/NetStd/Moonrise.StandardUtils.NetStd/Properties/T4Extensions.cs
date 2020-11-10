using System;
using System.Collections.Generic;
using System.Reflection;

// Taken from https://blog.rsuter.com/use-t4-texttemplatingfilepreprocessor-in-net-standard-or-pcl-libraries/
// to allow for T4 templating with .Net Core+

namespace Moonrise
{
    /// <summary>
    /// Allows some of the generated code that occurs within T4 templating to work with .Net Core +
    /// </summary>
    internal static class T4Extensions
    {
        public static MethodInfo GetMethod(this Type type, string method, params Type[] parameters)
        {
            return type.GetRuntimeMethod(method, parameters);
        }
    }
}

namespace System.CodeDom.Compiler
{
//    public class CompilerErrorCollection : List<CompilerErrorCollection>
    public class CompilerErrorCollection : List<CompilerError>
    {
    }

    public class CompilerError
    {
        public string ErrorText { get; set; }
        public bool IsWarning { get; set; }
    }
}
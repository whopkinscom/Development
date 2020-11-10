using System;

namespace Moonrise.Utils.Standard.CSV
{
    /// <summary>
    ///     Indicates that a condition has been detected whereby the entire line is to be skipped/discarded
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CsvParseSkipThisLineException : Exception { }
}

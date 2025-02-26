using System;

namespace Moonrise.Utils.Standard.CSV
{
    /// <summary>
    ///     Defines metadata about how to convert from CSV data into the attributed property.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        /// <summary>
        ///     The header name for a CSV column
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        ///     A custom converter.
        /// </summary>
        public Type Converter { get; set; } = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvColumnAttribute" /> class. i.e. default takes the header name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public CsvColumnAttribute(string columnName) => ColumnName = columnName;
    }
}
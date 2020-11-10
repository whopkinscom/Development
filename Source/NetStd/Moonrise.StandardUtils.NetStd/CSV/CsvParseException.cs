using System;
using System.Collections.Generic;

namespace Moonrise.Utils.Standard.CSV
{
    /// <summary>
    ///     Indicates an exception whilst parsing a CSV file
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CsvParseException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvParseException" /> class.
        /// </summary>
        public CsvParseException() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvParseException" /> class.
        /// </summary>
        /// <param name="rowNo">The row no.</param>
        /// <param name="rowContent">Content of the row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="exception">The exception.</param>
        public CsvParseException(int rowNo, string rowContent, string columnName, Exception exception)
        {
            RowNo = rowNo;
            RowContent = rowContent;
            ColumnName = columnName;
            Exception = exception;
        }

        /// <summary>
        ///     Collated exceptions. See <seealso cref="CsvParser{TTarget,TOverlay}.CollateExceptions" />
        /// </summary>
        public List<CsvParseException> CollatedExceptions { get; private set; }

        /// <summary>
        ///     The name of the column the exception occurred processing.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        ///     The exception that occurred.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     The content of the column the exception occurred processing.
        /// </summary>
        public string RowContent { get; }

        /// <summary>
        ///     The row number the exception occurred processing.
        /// </summary>
        public int RowNo { get; }

        /// <summary>
        ///     Adds the specified parse exception.
        /// </summary>
        /// <param name="parseException">The parse exception.</param>
        internal void Add(CsvParseException parseException)
        {
            if (CollatedExceptions == null)
            {
                CollatedExceptions = new List<CsvParseException>();
            }

            CollatedExceptions.Add(parseException);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Moonrise.Utils.Standard.CSV
{
    public class CsvParseException : Exception
    {
        public CsvParseException() { }

        public CsvParseException(int row, string columnName, Exception exception)
        {
            Row = row;
            ColumnName = columnName;
            Exception = exception;
        }

        public List<CsvParseException> CollatedExceptions { get; private set; }

        public string ColumnName { get; }

        public Exception Exception { get; }

        public int Row { get; }

        public void Add(CsvParseException parseException)
        {
            if (CollatedExceptions == null)
            {
                CollatedExceptions = new List<CsvParseException>();
            }

            CollatedExceptions.Add(parseException);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moonrise.Utils.Standard.Extensions;

namespace Moonrise.Utils.Standard.CSV
{
    /// <summary>
    ///     Parses text in CSV format that includes a header that is mapped via a <see cref="CsvColumnAttribute" />
    ///     defined against the properties of the class.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <seealso cref="Moonrise.Utils.Standard.CSV.CsvParser{TTarget, TTarget}" />
    public class CsvParser<TTarget> : CsvParser<TTarget, TTarget>
        where TTarget : class, new() { }

    /// <summary>
    ///     Parses text in CSV format that includes a header that is mapped via a <see cref="CsvColumnAttribute" />
    ///     defined in a seperate overlay, usually an interface.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <typeparam name="TOverlay">The type of the overlay.</typeparam>
    /// <seealso cref="Moonrise.Utils.Standard.CSV.CsvParser{TTarget, TTarget}" />
    public class CsvParser<TTarget, TOverlay>
        where TTarget : class, new()
        where TOverlay : class
    {
        private class ColumnDefinition
        {
            public ICsvConverter ConvertWith { get; set; }

            public int Index { get; set; }

            public string Name { get; set; }

            public PropertyInfo PropertyInfo { get; set; }

            public Type Type { get; set; }

            public TypeConverter TypeConverter { get; set; }
        }

        private readonly Dictionary<string, ColumnDefinition> _columns = new Dictionary<string, ColumnDefinition>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvParser{TTarget, TOverlay}" /> class.
        /// </summary>
        public CsvParser()
        {
            BuildColumnDefinitions();
        }

        /// <summary>
        ///     Determines if exceptions should be collated and thrown out as one with all errors for all rows, or just the first
        ///     error encountered.
        /// </summary>
        public bool CollateExceptions { get; set; }

        private CsvParseException CollatedExceptions { get; set; }

        /// <summary>
        ///     Parses the specified filepath as a CSV and returns a list of imported instantied classes.
        /// </summary>
        /// <param name="filepath">The filepath of the CSV formatted file.</param>
        /// <param name="delimiter">The delimiter, typically commas.</param>
        /// <param name="qualifier">
        ///     A qualifier, typically double quotes. i.e. Can be used to enclose whitespace, including the
        ///     delimiter, within the one value.
        /// </param>
        /// <param name="trimData">Should the qualifiers be trimmed?</param>
        /// <returns>An enumerable of imported rows of the Type</returns>
        public IEnumerable<TTarget> Parse(string filepath, string delimiter = ",", string qualifier = "\"", bool trimData = true)
        {
            string[] lines = File.ReadAllLines(filepath);

            return Parse(lines, delimiter, qualifier, trimData);
        }

        /// <summary>
        ///     Parses the array of strings as a CSV and returns a list of imported instantied classes.
        /// </summary>
        /// <param name="lines">The array of strings containing the CSV formatted data.</param>
        /// <param name="delimiter">The delimiter, typically commas.</param>
        /// <param name="qualifier">
        ///     A qualifier, typically double quotes. i.e. Can be used to enclose whitespace, including the
        ///     delimiter, within the one value.
        /// </param>
        /// <param name="trimData">Should the qualifiers be trimmed?</param>
        /// <returns>An enumerable of imported rows of the Type</returns>
        public IEnumerable<TTarget> Parse(IList<string> lines, string delimiter = ",", string qualifier = "\"", bool trimData = true)
        {
            TTarget[] targets = new TTarget[lines.Count - 1];

            // First line contains the headers
            string[] headers = lines[0].SplitRow(delimiter, qualifier, true);

            // Match the position of the headers to the column definitions
            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i];

                if (_columns.ContainsKey(header))
                {
                    _columns[header].Index = i;
                }
            }

            Parallel.For(1, lines.Count, i => { ProcessRow(lines[i], targets, i - 1, delimiter, qualifier, trimData); });

            if (CollatedExceptions != null)
            {
                throw CollatedExceptions;
            }

            return targets;
        }

        /// <summary>
        /// Parses a collection of string object dictionaries.
        /// </summary>
        /// <param name="dictionaries">The dictionaries.</param>
        /// <returns>A collection of parsed target instances</returns>
        public IEnumerable<TTarget> ParseDictionaries(IEnumerable<Dictionary<string, object>> dictionaries)
        {
            List<string> rows = null;

            foreach (Dictionary<string, object> dictionary in dictionaries)
            {
                if (rows == null)
                {
                    rows = new List<string>();
                    List<string> headerList = new List<string>(dictionary.Keys);
                    string headerString = headerList.CSL(", ", "\"", "\"");
                    rows.Add(headerString);
                }

                string rowString = dictionary.Values.CSL(",");
                rows.Add(rowString);
            }

            return Parse(rows);
        }

        /// <summary>
        ///     Builds the column definitions from the specified <see cref="TOverlay" />.
        /// </summary>
        private void BuildColumnDefinitions()
        {
            // Get all public properties defined in the overlay
            PropertyInfo[] overlayProperties = typeof(TOverlay).GetProperties();

            foreach (PropertyInfo overlayProperty in overlayProperties)
            {
                IEnumerable<Attribute> memberAttributes = overlayProperty.GetCustomAttributes(typeof(CsvColumnAttribute));

                // Only interested in this property IF it has a CsvColumnAttribute
                CsvColumnAttribute csvAttribute = (CsvColumnAttribute)memberAttributes.FirstOrDefault();

                if (csvAttribute != null)
                {
                    // Only interested in this property IF it does actually map to the type
                    PropertyInfo targetProperty = typeof(TTarget).GetProperty(overlayProperty.Name);
                    ICsvConverter converter = null;

                    if (csvAttribute.Converter != null)
                    {
                        try
                        {
                            converter = (ICsvConverter)Activator.CreateInstance(csvAttribute.Converter);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException($"Converter must implement {nameof(ICsvConverter)}", overlayProperty.Name);
                        }
                    }

                    if ((targetProperty != null) && (targetProperty.PropertyType == overlayProperty.PropertyType))
                    {
                        // And its consequent TypeConverter
                        ColumnDefinition colDef = new ColumnDefinition
                                                  {
                                                      PropertyInfo = targetProperty,
                                                      TypeConverter = TypeDescriptor.GetConverter(targetProperty.PropertyType),
                                                      ConvertWith = converter,
                                                      Name = csvAttribute.ColumnName
                                                  };
                        _columns.Add(csvAttribute.ColumnName, colDef);
                    }
                }
            }
        }

        /// <summary>
        ///     Processes a row.
        /// </summary>
        /// <param name="row">The full row as a string.</param>
        /// <param name="targets">The array of targets.</param>
        /// <param name="i">The index of the row being processed.</param>
        /// <param name="delimiter">The delimiter, typically commas.</param>
        /// <param name="qualifier">
        ///     A qualifier, typically double quotes. i.e. Can be used to enclose whitespace, including the
        ///     delimiter, within the one value.
        /// </param>
        /// <param name="trimData">Should the qualifiers be trimmed?</param>
        private void ProcessRow(string row, TTarget[] targets, int i, string delimiter, string qualifier, bool trimData)
        {
            TTarget target = new TTarget();
            targets[i] = target;
            string[] columns = row.SplitRow(delimiter, qualifier, trimData);

            foreach (ColumnDefinition defn in _columns.Values)
            {
                try
                {
                    object value;

                    if (defn.ConvertWith != null)
                    {
                        value = defn.ConvertWith.Convert(columns[defn.Index]);
                    }
                    else
                    {
                        value = defn.TypeConverter.ConvertFromString(columns[defn.Index]);
                    }

                    defn.PropertyInfo.SetValue(target, value);
                }
                catch (CsvParseSkipThisLineException)
                {
                    // In this case we simply ignore this row!
                    return;
                }
                catch (Exception e)
                {
                    if (CollateExceptions)
                    {
                        if (CollatedExceptions == null)
                        {
                            CollatedExceptions = new CsvParseException();
                        }

                        CollatedExceptions.Add(new CsvParseException(i, row, defn.Name, e));
                    }
                    else
                    {
                        throw new CsvParseException(i, row, defn.Name, e);
                    }
                }
            }
        }
    }
}

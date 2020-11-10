﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.CSV;

namespace Moonrise.StandardUtils.Tests.CSV
{
    [TestClass]
    public class CsvParserTests
    {
        public class SampleClass
        {
            public int Number { get; set; }

            public string Name { get; set; }
        }

        public interface ICsvDefn
        {
            [CsvColumn("No.")]
            int Number { get; set; }

            [CsvColumn("Nom")]
            string Name { get; set; }
        }

        public class CsvDefn
        {
            public class ConvertDate : ICsvConverter
            {
                public object Convert(string input)
                {
                    return DateTimeOffset.Parse(input);
                }
            }

            [CsvColumn("First Name", Converter = typeof(ConvertDate))]
            public DateTimeOffset Date { get; set; }

            [CsvColumn("Nom")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void BasicParseWithOverlay()
        {
            CsvParser<SampleClass, ICsvDefn> parser = new CsvParser<SampleClass, ICsvDefn>();
            IEnumerable<SampleClass> results = parser.Parse("..\\..\\CSV\\sample1.csv");
            Assert.AreEqual(187, results.First().Number);
            Assert.AreEqual("Frederick", results.First().Name);
        }

        [TestMethod]
        public void BasicParse()
        {
            CsvParser<CsvDefn> parser = new CsvParser<CsvDefn>();
            IEnumerable<CsvDefn> results = parser.Parse("..\\..\\CSV\\sample1.csv");
            Assert.AreEqual("Frederick", results.First().Name);
        }

        [TestMethod]
        public void BasicDictionaryParse()
        {
            CsvParser<CsvDefn> parser = new CsvParser<CsvDefn>();

            List<Dictionary<string, object>> dictionaries = new List<Dictionary<string, object>>();

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("No.", 187);
            dictionary.Add("First Name", "12/12/1212");
            dictionary.Add("Nom", "Frederick");
            dictionaries.Add(dictionary);

            dictionary = new Dictionary<string, object>();
            dictionary.Add("No.", "123");
            dictionary.Add("First Name", "12/01/2018");
            dictionary.Add("Nom", "Beany Babies");
            dictionaries.Add(dictionary);

            IEnumerable<CsvDefn> results = parser.ParseDictionaries(dictionaries);
            Assert.AreEqual("Frederick", results.First().Name);
        }
    }
}

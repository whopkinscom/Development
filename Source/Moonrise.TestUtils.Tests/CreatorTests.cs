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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Validation;
using Moonrise.Utils.Test.ObjectCreation;

namespace Moonrise.TestUtils.Tests
{
    [TestClass]
    public class CreatorTests
    {
        public interface INormalTypes
        {
            [ObjectCreation(MinDecimal = 40.5, MaxDecimal = 100.5)]
            decimal Decimal { get; set; }
        }

        public class NormalTypes
        {
            public DateTime DateTime { get; set; }

            public decimal Decimal { get; set; }

            [Range(1d, double.MaxValue)]
            public double Double { get; set; }

            [Range(1, int.MaxValue)]
            public int Int { get; set; }

            [Range(1, byte.MaxValue)]
            public byte Byte { get; set; }
            [Range(1, sbyte.MaxValue)]
            public sbyte SByte { get; set; }
            [Range(1, short.MaxValue)]
            public short Short { get; set; }
            [Range(1, ushort.MaxValue)]
            public ushort UShort { get; set; }
            [Range(1, float.MaxValue)]
            public float Float { get; set; }
            [Range(1, uint.MaxValue)]
            public uint UInt { get; set; }
            [Range(long.MinValue, long.MaxValue)]
            public long Long { get; set; }
            [Range(ulong.MinValue, ulong.MaxValue)]
            public ulong ULong { get; set; }
            [Range(1, char.MaxValue)]
            public char Char { get; set; }
        }

        public class NullableTypes
        {
            public bool? NullableBool { get; set; }

            public DateTime? NullableDateTime { get; set; }

            public int? NullableInt { get; set; }
        }

        public class Container
        {
            public class InnerOne
            {
                public int No { get; set; }
                public InnerTwo Nested { get; set; }
            }

            public class InnerTwo
            {
                public int Number { get; set; }
                public InnerOne Nested { get; set; }
                public Container Recursive { get; set; }
            }

            public InnerOne NeatlyNested { get; set; }
        }

        public class HasIEnumerable
        {
            public IEnumerable<int> IntList { get; set; }
            public IEnumerable<NullableTypes> NullablesList { get; set; }
        }

        [TestMethod]
        public void CreatesNormalTypes()
        {
            Creator creator = new Creator
                              {
                                  RespectValidation = true
                              };

            NormalTypes target = creator.CreateFilled<NormalTypes, INormalTypes>();
            Assert.IsNotNull(target);
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.DateTime));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Int));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Double));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Decimal));
            Assert.IsTrue((target.Decimal >= 40.5M) && (target.Decimal <= 100.5M));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Byte));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.SByte));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Short));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.UShort));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Long));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.ULong));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.UInt));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Char));
            Assert.IsFalse(NonDefaultAttribute.IsDefault(target.Float));
        }

        [TestMethod]
        public void CreatesNullableTypes()
        {
            Creator creator = new Creator();
            NullableTypes target = creator.CreateFilled<NullableTypes>();
            Assert.IsNotNull(target);
            Assert.IsNotNull(target.NullableDateTime);
            Assert.IsNotNull(target.NullableInt);
            Assert.IsNotNull(target.NullableBool);
        }

        [TestMethod]
        public void GetRandomDecimals()
        {
            Creator creator = new Creator();
            creator.MinDecimal = 10M;
            creator.MaxDecimal = 100M;

            for (int i = 0; i < 500; i++)
            {
                decimal target = creator.GetRandomDecimal();
                decimal target1 = creator.GetRandomDecimal(0m, 500m);
                decimal target2 = creator.GetRandomDecimal(1000m, 5000m);
                Assert.IsTrue(target2 > target1);
            }
        }

        [TestMethod]
        public void Recursion_Blocked()
        {
            Creator creator = new Creator();
            Container container = creator.CreateFilled<Container>();
            Assert.IsNull(container.NeatlyNested.Nested.Nested);
            Assert.IsNull(container.NeatlyNested.Nested.Recursive);
        }

        [TestMethod]
        public void IEnumerables_Created()
        {
            Creator creator = new Creator
                              {
                                  MinItems = 5,
                                  MaxItems = 10
                              };

            HasIEnumerable container = creator.CreateFilled<HasIEnumerable>();
            Assert.IsTrue(container.IntList.Any());
            Assert.IsTrue(container.NullablesList.Any());
        }

        [TestMethod]
        public void DoNotCreate_InnerTwo()
        {
            Creator creator = new Creator();
            creator.DoNotCreate<Container.InnerTwo>();
            Container container = creator.CreateFilled<Container>();
            Assert.IsNull(container.NeatlyNested.Nested);
        }
    }
}

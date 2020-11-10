using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moonrise.Utils.Standard.Extensions;

namespace Moonrise.StandardUtils.Tests.Extensions
{
    [TestClass]
    public class ClassExtensionsTests
    {
        [TestMethod]
        public void MethodName_Works()
        {
            Assert.AreEqual($"{nameof(ClassExtensionsTests)}.{nameof(MethodName_Works)}", this.MethodName());
        }

        [TestMethod]
        public void FQMethodName_Works()
        {
            Assert.AreEqual($"{nameof(Moonrise)}.{nameof(StandardUtils)}.{nameof(Tests)}.{nameof(Extensions)}.{nameof(ClassExtensionsTests)}.{nameof(FQMethodName_Works)}", this.FQMethodName());
        }
    }
}

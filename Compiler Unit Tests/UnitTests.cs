using Microsoft.VisualStudio.TestTools.UnitTesting;
using AllanMilne.PALCompiler;
using System.IO;

namespace Compiler_Unit_Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void SyntaxInvalidChar()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM InvalidChar
            WITH i, n, factorial AS INTEGER
            IN
                i = n
                n := 80
                factorial = 502
            END");
            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 2);
        }

    }
}

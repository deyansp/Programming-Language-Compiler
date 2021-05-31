using Microsoft.VisualStudio.TestTools.UnitTesting;
using AllanMilne.PALCompiler;
using System.IO;

namespace Compiler_Unit_Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void Syntax_Invalid_Char()
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

        [TestMethod]
        public void Syntax_Invalid_No_Program_Name()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM
            WITH i AS REAL
            IN
                i = 7.8
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 1);
        }

        [TestMethod]
        public void Syntax_Invalid_No_Expression_After_UNTIL()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM NoExpressionAferUNTIL
            WITH
                i, n, factorial AS INTEGER
            IN
                INPUT i
                n = 1
                UNTIL REPEAT
                    factorial = factorial * i
                ENDLOOP
                OUTPUT factorial
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 1);
        }

        [TestMethod]
        public void Syntax_Valid_Empty_ELSE()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM ValidEmptyELSE
            WITH f AS INTEGER
            IN
                f = (9 / 3) * 145
                IF f < 300
	            THEN OUTPUT 1
                ELSE
                ENDIF
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 0);
        }
    }
}

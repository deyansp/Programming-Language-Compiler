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

        [TestMethod]
        public void Semantics_Valid_Types_And_Assignments()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM ValidTypesAndAssignments
            WITH
                a, b AS REAL
                i AS INTEGER
            IN
                a = 7.87
                b = 2.0
                a = a + b

                i = 64
                i = i * 8

                IF i > 128
	            THEN OUTPUT i
	            ELSE OUTPUT 128
                ENDIF
    
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 0);
        }

        [TestMethod]
        public void Semantics_Valid_Boolean_Expression()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM BooleanExprSameTypes
            WITH
            IN 

            IF 2 + 3 > 5
            THEN OUTPUT 1
            ELSE OUTPUT 0
            ENDIF

            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 0);
        }

        [TestMethod]
        public void Semantics_Invalid_Types_And_Assignments()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM ValidTypesAndAssignments
            PROGRAM InvalidTypesAndAssignments
            WITH
                a, b AS REAL
                i AS INTEGER
            IN
                a = 7.87
                b = 2
                a = a + b

                d = 2

                i = 64
                b = a * i
                i = 21.02
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 4);
        }

        [TestMethod]
        public void Semantics_Invalid_Expression_Types()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM InvalidTypeExpressions 
            WITH
                i AS INTEGER
                j AS REAL
            IN
                INPUT i, j
    
                i = (7.0 * j + 2.0)
                j = (3.3 / 1.5 - 7)
    
                OUTPUT i, j
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 2);
        }

        [TestMethod]
        public void Semantics_Invalid_Undeclared_INPUT_Variables()
        {
            PALParser parser = new PALParser();
            StringReader reader = new StringReader(@"PROGRAM UndeclaredINPUTVars
            WITH
                i AS INTEGER
                k AS REAL
            IN
                INPUT i, k, x, y, z
            END");

            parser.Parse(reader);
            Assert.IsTrue(parser.Errors.Count == 3);
        }
    }
}

using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;

namespace AllanMilne.PALCompiler
{
    public class PALPureParser : RecoveringRdParser
    {
        //--- The constructor method.
        public PALPureParser()
        : base(new PALScanner())
        { }

        private List<String> variables = new List<String>();

        protected override void recStarter()
        {
            recProgram();
        }

        // starting point for syntax analysis
        protected void recProgram()
        {
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");
            recStatement();
            mustBe("END");
            mustBe(Token.EndOfFile);
        }

        // <VarDecls> ::= (<IdentList> AS <Type>)* ;
        protected void recVarDecls()
        {
            while (have(Token.IdentifierToken))
            {
                recIdentList();
                mustBe("AS");
                recType();
            }
        }

        // <IdentList> ::= Identifier( , Identifier)* ;
        protected void recIdentList()
        {
            mustBe(Token.IdentifierToken);
            variables.Add(scanner.CurrentToken.ToString());

            while (have(","))
            {
                mustBe(",");
                variables.Add(scanner.CurrentToken.ToString());
                mustBe(Token.IdentifierToken);
            }

            //check for duplicates in semantics analysis
        }

        // <Type> ::= REAL | INTEGER ;
        protected void recType()
        {
            if (have("REAL"))
                mustBe(Token.RealToken);
            else if (have("INTEGER"))
                mustBe(Token.IntegerToken);
            else
                syntaxError("variable type must be REAL or INTEGER");
        }

        // <Statement> ::= <Assignment> | <Loop> | <Conditional> | <I-o> ;
        protected void recStatement()
        {
            if (have(Token.IdentifierToken))
                recAssignment();
            else if (have("UNTIL"))
            {
                recLoop();
            }
            else if (have("IF"))
            {

            }
            else if (have("INPUT") || have("OUTPUT"))
            {

            }
            else
                syntaxError("expected Assignment, Loop, Conditional, or I/O statement");
        }

        protected void recBlockOfStatements()
        {
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") 
                    || have("INPUT") || have("OUTPUT"))
            {
                recStatement();
            }
        }

        // <Assignment> ::= Identifier = <Expression> ;
        protected void recAssignment()
        {
            mustBe(Token.IdentifierToken);
            mustBe("=");
            recExpression();
        }

        // <Expression> ::= <Term> ( (+|-) <Term>)* ;
        protected void recExpression()
        {
            recTerm();

            while (have("+") || have("-"))
            {
                if (have("+"))
                    mustBe("+");
                else
                    mustBe("-");

                recTerm();
            }
        }

        // <Term> ::= <Factor> ((*|/) <Factor>)* ;
        protected void recTerm()
        {
            recFactor();

            while (have("*") || have("/"))
            {
                if (have("*"))
                    mustBe("*");
                else
                    mustBe("/");

                recFactor();
            }
        }

        // <Factor> ::= (+|-)? ( <Value> | "(" <Expression> ")" ) ;
        protected void recFactor()
        {
            if (have("+"))
                mustBe("+");
            else if (have("-"))
                mustBe("-");

            if (have("("))
            {
                mustBe("(");
                recExpression();
                mustBe(")");
            }
            else
                recValue();
        }

        // <Value> ::= Identifier | IntegerValue | RealValue ;
        protected void recValue()
        {
            if (have(Token.IdentifierToken))
                mustBe(Token.IdentifierToken);

            else if (have(Token.IntegerToken))
                mustBe(Token.IntegerToken);

            else if (have(Token.RealToken))
                mustBe(Token.RealToken);
            else
                syntaxError("Value must be IDENTIFIER, INTEGER or REAL");
        }


        // <Loop> ::= UNTIL <BooleanExpr> REPEAT (<Statement>)* ENDLOOP ;
        protected void recLoop()
        {
            mustBe("UNTIL");
            //recBoolExpression();
            mustBe("REPEAT");
            recBlockOfStatements();
            mustBe("ENDLOOP");
        }

    } // end PALPureParser class.

} // end namespace.
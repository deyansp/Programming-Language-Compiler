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
            recBlockOfStatements();
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
                mustBe("REAL");
            else if (have("INTEGER"))
                mustBe("INTEGER");
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
                recConditional();
            }
            else if (have("INPUT") || have("OUTPUT"))
            {
                recIO();
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
            recBooleanExpr();
            mustBe("REPEAT");
            recBlockOfStatements();
            mustBe("ENDLOOP");
        }

        // <BooleanExpr> ::= <Expression> ("<" | "=" | ">") <Expression> ;
        protected void recBooleanExpr()
        {
            recExpression();

            if (have("<"))
                mustBe("<");

            else if (have("="))
                mustBe("=");

            else if (have(">"))
                mustBe(">");
            else
                syntaxError("invalid BOOLEAN EXPRESSION");

            recExpression();
        }

        // <Conditional> ::= IF<BooleanExpr> THEN(<Statement>)*
        //                   (ELSE (<Statement>)* )? 
        //                   ENDIF ;
        protected void recConditional()
        {
            mustBe("IF");
            recBooleanExpr();
            mustBe("THEN");
            recBlockOfStatements();

            if (have("ELSE"))
            {
                mustBe("ELSE");
                recBlockOfStatements();
            }

            mustBe("ENDIF");
        }

        //<I-o> ::= INPUT<IdentList> | 
        //          OUTPUT<Expression>( , <Expression>)* ;
        protected void recIO()
        {
            if (have("INPUT"))
            {
                mustBe("INPUT");
                recIdentList();
            }

            else if (have("OUTPUT"))
            {
                mustBe("OUTPUT");
                recExpression();

                while (have(","))
                {
                    mustBe(",");
                    recExpression();
                }
            }

            else
                syntaxError("Invalid I/O statement");
        }


    } // end PALPureParser class.

} // end namespace.
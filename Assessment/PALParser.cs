using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;

namespace AllanMilne.PALCompiler
{
    public class PALParser : RecoveringRdParser
    {
        private PALSemantics semantics;

        //--- The constructor method.
        public PALParser()
        : base(new PALScanner())
        {
            semantics = new PALSemantics(this);
        }
        
        private List<String> variables = new List<String>();

        protected override void recStarter()
        {
            recProgram();
        }

        // starting point for syntax analysis
        protected void recProgram()
        {
            Scope.OpenScope();
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");
            recStatement();
            recBlockOfStatements();
            mustBe("END");
            mustBe(Token.EndOfFile);
            Scope.CloseScope();
        }

        // <VarDecls> ::= (<IdentList> AS <Type>)* ;
        protected void recVarDecls()
        {
            while (have(Token.IdentifierToken))
            {
                var varNames = recIdentList();
                mustBe("AS");
                int type = recType();

                foreach(var id in varNames)
                {
                    // declaring each variable with its name and type
                    semantics.declareVariable(id, type);
                }
            }
        }

        // <IdentList> ::= Identifier( , Identifier)* ;
        protected List<IToken> recIdentList()
        {
            List<IToken> identifiers = new List<IToken>();
            identifiers.Add(scanner.CurrentToken);
            mustBe(Token.IdentifierToken);

            while (have(","))
            {
                mustBe(",");
                identifiers.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }

            return identifiers;
        }

        // <Type> ::= REAL | INTEGER ;
        protected int recType()
        {
            int type = LanguageType.Undefined;

            if (have("REAL"))
            {
                type = LanguageType.Real;
                mustBe("REAL");
            }
            else if (have("INTEGER"))
            {
                type = LanguageType.Integer;
                mustBe("INTEGER");
            }
            else
                syntaxError("variable type must be REAL or INTEGER");
            
            return type;
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
            // the variable being assigned to
            IToken lhs = scanner.CurrentToken;
            mustBe(Token.IdentifierToken);

            bool needToCheck = have("=");
            mustBe("=");

            IToken expectedToken = scanner.CurrentToken;
            int rhs = recExpression();
            
            // only checking assignment semantics if there is an equals sign
            if (needToCheck)
                semantics.checkAssignment(expectedToken, lhs, rhs);
        }

        // <Expression> ::= <Term> ( (+|-) <Term>)* ;
        protected int recExpression()
        {
            int type = recTerm();

            while (have("+") || have("-"))
            {
                IToken operation = scanner.CurrentToken;

                if (have("+"))
                    mustBe("+");
                else
                    mustBe("-");
                
                // save token for correct error reporting
                IToken expectedToken = scanner.CurrentToken;
                
                int rhs = recTerm();
                type = semantics.checkExpression(operation, expectedToken, type, rhs);
            }

            return type;
        }

        // <Term> ::= <Factor> ((*|/) <Factor>)* ;
        protected int recTerm()
        {
            int type = recFactor();

            while (have("*") || have("/"))
            {
                IToken operation = scanner.CurrentToken;
                
                if (have("*"))
                    mustBe("*");
                else
                    mustBe("/");
                
                // save token for correct error reporting
                IToken expectedToken = scanner.CurrentToken;
                
                int rhs = recFactor();
                type = semantics.checkExpression(operation, expectedToken, type, rhs);
            }
            return type;
        }

        // <Factor> ::= (+|-)? ( <Value> | "(" <Expression> ")" ) ;
        protected int recFactor()
        {
            if (have("+"))
                mustBe("+");
            else if (have("-"))
                mustBe("-");

            if (have("("))
            {
                mustBe("(");
                int type = recExpression();
                mustBe(")");
                return type;
            }
            else
                return recValue();
        }

        // <Value> ::= Identifier | IntegerValue | RealValue ;
        protected int recValue()
        {
            int valueType = LanguageType.Undefined;
            IToken token = scanner.CurrentToken;
            
            if (have(Token.IdentifierToken))
            {
                mustBe(Token.IdentifierToken);
                valueType = semantics.checkVariable(token);
            }

            else if (have(Token.IntegerToken))
            {
                mustBe(Token.IntegerToken);
                valueType = semantics.checkValueType(token);
            }

            else if (have(Token.RealToken))
            {
                mustBe(Token.RealToken);
                valueType = semantics.checkValueType(token);
            }
            
            else
                syntaxError("Value must be IDENTIFIER, INTEGER or REAL");

            return valueType;
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
            int lhs = recExpression();

            if (have("<"))
                mustBe("<");

            else if (have("="))
                mustBe("=");

            else if (have(">"))
                mustBe(">");
            else
                syntaxError("invalid BOOLEAN EXPRESSION");
            
            // save token for correct error reporting
            IToken expectedToken = scanner.CurrentToken;

            int rhs = recExpression();
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


    } // end PALParser class

} // end namespace
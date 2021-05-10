using AllanMilne.Ardkit;
using System;
using System.Collections.Generic;

namespace AllanMilne.PALCompiler
{
    public class PALParser : RecoveringRdParser
    {
        private PALSemantics semantics;

        // the constructor
        public PALParser()
        : base(new PALScanner())
        {
            semantics = new PALSemantics(this);
        }

        protected override void recStarter()
        {
            recProgram();
        }

        // starting point for analysis
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
                syntaxError("<Type>");
            
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
                syntaxError("<Statement>");
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

            // save token for correct error reporting
            IToken rhsToken = scanner.CurrentToken;
            int rhs = recExpression();

            // only checking assignment semantics if there is an equals sign, i.e valid assignment syntax
            if (needToCheck)
            {
                // check if the variable being assigned to exists
                int variable = semantics.checkVariable(lhs);
                
                // only check assignment type compatibility if the variable exists
                if (variable != LanguageType.Undefined)
                    semantics.checkTypesSame(rhsToken, variable, rhs);
            }
        }

        // <Expression> ::= <Term> ( (+|-) <Term>)* ;
        protected int recExpression()
        {
            int type = recTerm();

            while (have("+") || have("-"))
            {
                if (have("+"))
                    mustBe("+");
                else
                    mustBe("-");
                
                // save token for correct error reporting
                IToken rhsToken = scanner.CurrentToken;
                
                int rhs = recTerm();
                type = semantics.checkExpression(rhsToken, type, rhs);
            }

            return type;
        }

        // <Term> ::= <Factor> ((*|/) <Factor>)* ;
        protected int recTerm()
        {
            int type = recFactor();

            while (have("*") || have("/"))
            {
                if (have("*"))
                    mustBe("*");
                else
                    mustBe("/");
                
                // save token for correct error reporting
                IToken rhsToken = scanner.CurrentToken;
                
                int rhs = recFactor();
                type = semantics.checkExpression(rhsToken, type, rhs);
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
                syntaxError("<Value>");

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
                syntaxError("<BooleanExpr>");
            
            // save token for correct error reporting
            IToken rhsToken = scanner.CurrentToken;

            int rhs = recExpression();

            semantics.checkTypesSame(rhsToken, lhs, rhs);
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
                List<IToken> identifiers = recIdentList();

                // checking if the variables exist
                foreach(var id in identifiers)
                {
                    semantics.checkVariable(id);
                }
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
                syntaxError("<I-O>");
        }


    } // end PALParser class

} // end namespace
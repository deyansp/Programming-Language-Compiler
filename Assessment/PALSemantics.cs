using System;
using System.Collections.Generic;
using System.Text;
using AllanMilne.Ardkit;

namespace AllanMilne.PALCompiler
{
    class PALSemantics : Semantics
    {
        public PALSemantics(IParser p) : base(p)
        { }

        // stores newly defined variables in the current Scope's symbols table
        public void declareVariable(IToken id, int type)
        {
            if (!id.Is(Token.IdentifierToken)) return;  // only proceed if an identifier.
            Scope symbols = Scope.CurrentScope;
            if (symbols.IsDefined(id.TokenValue))
            {
                semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
            }
            else
            {
                symbols.Add(new VarSymbol(id, type));
            }
        }

        // Check if a variable is defined in the current scope and return its type
        public int checkVariable(IToken id)
        {
            if (!Scope.CurrentScope.IsDefined(id.TokenValue))
            {
                semanticError(new NotDeclaredError(id));
                return LanguageType.Undefined;
            }
            else return checkValueType(id);
        }

        // returns a variable's type
        public int checkValueType(IToken token)
        {
            int thisType = LanguageType.Undefined;

            if (token.Is(Token.IdentifierToken))
                thisType = Scope.CurrentScope.Get(token.TokenValue).Type;
            else if (token.Is(Token.IntegerToken))
                thisType = LanguageType.Integer;
            else if (token.Is(Token.RealToken))
                thisType = LanguageType.Real;

            return thisType;
        }

        public bool checkTypesSame (IToken token, int type1, int type2)
        {
            if (type1 != type2)
            {
                semanticError(new TypeConflictError(token, type2, type1));
                return false;
            }
            
            return true;
        }

        public int checkExpression(IToken expected, int lhs, int rhs)
        {
            // if both sides are invalid there is no point in checking further
            // as Undefined will be returned anyway and the error has been reported
            // by the Parser's recValue method
            if (lhs == LanguageType.Undefined && rhs == LanguageType.Undefined)
            {
                return LanguageType.Undefined;
            }

            // if both sides are of the same type, return that as the overall expression type
            if (checkTypesSame(expected, lhs, rhs))
                return lhs;
            // otherwise return the larger type of the two (type inference)
            else
                return lhs > rhs ? lhs : rhs;
        }
    }
}

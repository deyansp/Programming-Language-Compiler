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

        public bool variableExists(IToken id)
        {
            Scope symbols = Scope.CurrentScope;
            return symbols.IsDefined(id.TokenValue);
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
    }
}

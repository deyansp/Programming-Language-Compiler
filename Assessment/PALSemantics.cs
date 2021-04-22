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

		// stores newly defined variables in the current Scope object's symbols
		public void DeclareId(IToken id)
		{
			if (!id.Is(Token.IdentifierToken)) return;  // only proceed if an identifier.
			Scope symbols = Scope.CurrentScope;
			if (symbols.IsDefined(id.TokenValue))
			{
				semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
			}
			else
			{
				symbols.Add(new VarSymbol(id, currentType));
			}
		}

		// Check the usage of an identifier.
		public void CheckId(IToken id)
		{
			if (!id.Is(Token.IdentifierToken)) return;  // only proceed if we have an identifier.
			if (!Scope.CurrentScope.IsDefined(id.TokenValue))
			{
				semanticError(new NotDeclaredError(id));
			}
			else CheckType(id); // check type compatibility
		}

		public void CheckType(IToken token)
		{
			int thisType = LanguageType.Undefined;
			if (token.Is(Token.IdentifierToken))
				thisType = Scope.CurrentScope.Get(token.TokenValue).Type;
			else if (token.Is(Token.IntegerToken))
				thisType = LanguageType.Integer;
			else if (token.Is(Token.RealToken))
				thisType = LanguageType.Real;
			
			// if not already set then set the current type being processed
			if (currentType == LanguageType.Undefined)
				currentType = thisType;
			if (currentType != thisType)
			{
				semanticError(new TypeConflictError(token, thisType, currentType));
			}
		}
	}
}

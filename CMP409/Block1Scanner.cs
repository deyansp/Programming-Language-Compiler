/*
	File: Block1Scanner.cs
	Purpose: A scanner for the Block-1 language.
	Version: 4.1
	Date: 13th April 2011.
	Author: Allan C. Milne.

	Namespace: AllanMilne.Block1
	Requires: Ardkit.dll
	Exposes: Block1Scanner.

	 Description:
	Uses version 2 of the Ardkit toolkit and reflects EBNF version 3.
	The Block-1 language is defined in the file Block-1.BNF.txt.
	A finite state machine (FSM) is used to implement the getNextToken() method that realises the scanning operation;
	a state transition pattern is used to implement the FSM.
	the FSM can be found in Block-1.FSM.txt.
*/


using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AllanMilne.Ardkit;


namespace AllanMilne.Block1
{

    //=== The Block-1 scanner implementation.
    public class Block1Scanner : Scanner
    {

        //--- The keywords of the Block-1 language.
        private static List<String> keywords = new List<String>(new String[] {
                                "begin", "end", "int", "real", "let",
                                "for", "to", "do", "get", "put"
   });

        protected override IToken getNextToken()
        {
            StringBuilder strbuf = null;             // for building actual token.
            int state = 0;                 // always start at FSM state S0.
            int startLine = 0, startCol = 0;   // start position of token.

            IToken token = null;
            while (token == null)
            {   // repeat until a token is found.
                switch (state)
                {
                    case 0:   // start state.
                        if (Char.IsWhiteSpace(currentChar)) state = 0;
                        else
                        {
                            startLine = line; startCol = column;   // start of actual token.
                            strbuf = new StringBuilder();              // for actual token.
                            if (Char.IsLetter(currentChar)) state = 1;
                            else if (Char.IsDigit(currentChar)) state = 2;
                            else if (currentChar == ':') state = 4;
                            else if ("+-*/(),".IndexOf(currentChar) != -1)      // single char punctuation.
                                state = 6;
                            else if (currentChar == eofChar) state = 7;
                            else state = 8;  // invalid.
                        }
                        break;

                    case 1:   // Identifier state.
                        if (Char.IsLetter(currentChar) ||
                            Char.IsDigit(currentChar) ||
                            currentChar == '_' || currentChar == '.')
                            state = 1;
                        else
                        {   // identifier might be a keyword.
                            String s = strbuf.ToString();      // language is case sensitive so do not change case.
                            if (keywords.Contains(s))      // if is a keyword.
                                token = new Token(s, startLine, startCol);
                            else token = new Token(Token.IdentifierToken, s, startLine, startCol);
                        }
                        break;

                    case 2:   // IntValue or leads to RealValue state.
                        if (Char.IsDigit(currentChar))
                            state = 2;
                        else if (currentChar == '.')
                            state = 3;
                        else token = new Token(Token.IntegerToken, strbuf.ToString(), startLine, startCol);
                        break;

                    case 3:    // RealValue state.
                        if (Char.IsDigit(currentChar))
                            state = 3;
                        else token = new Token(Token.RealToken, strbuf.ToString(), startLine, startCol);
                        break;

                    case 4:   // ":" state -> ":=".
                        if (currentChar == '=')
                            state = 5;
                        else token = new Token(Token.InvalidToken, strbuf.ToString(), startLine, startCol);
                        break;
                    case 5:   // ":=" state.
                        token = new Token(":=", startLine, startCol);
                        break;

                    case 6:   // all single character punctuation chars.
                        token = new Token(strbuf.ToString(), startLine, startCol);
                        break;

                    case 7:   // end-of-file.
                        token = new Token(Token.EndOfFile, startLine, startCol);
                        break;
                    case 8:   // invalid character.
                        token = new Token(Token.InvalidChar, strbuf.ToString(), startLine, startCol);
                        break;
                }

                if (token == null)
                {                 // have not yet found the token.
                    if (state != 0)
                    {                 // not in state 0, so not whitespace.
                        strbuf.Append(currentChar);   // build up token string.
                    }
                    getNextChar();                    // char has been used so get next one.
                }
            } // end while not found token.

            return token;

        } // end getNextToken method.

    } // end Block1Scanner class.

} // end namespace.
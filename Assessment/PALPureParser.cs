using System;
using System.IO;
using System.Collections.Generic;

using AllanMilne.Ardkit;

namespace AllanMilne.PALCompiler
{
    public class PALPureParser : RecoveringRdParser
    {
        //--- The constructor method.
        public PALPureParser()
        : base(new PALScanner())
        {  }

        private List<String> variables = new List<String>();

        protected override void recStarter()
        {
            recProgram();
        }
        
        protected void recProgram()
        {
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");
            //recStatement();
            mustBe("END");
        }

        protected void recVarDecls()
        {
            while (have(Token.IdentifierToken))
            {
                recIdentList();
                mustBe("AS");
                recType();
            }
        }

        protected void recIdentList()
        {
            mustBe(Token.IdentifierToken);
            variables.Add(scanner.CurrentToken.ToString());
            
            while(have(","))
            {
                mustBe(",");
                variables.Add(scanner.CurrentToken.ToString());
                mustBe(Token.IdentifierToken);
            }

            //check here for duplicates?
        }

        protected void recType()
        {
            if (have("REAL"))
                mustBe(Token.RealToken);
            else if (have("INTEGER"))
                mustBe(Token.IntegerToken);
            else
                syntaxError("variable type must be REAL or INTEGER");
        }


    } // end PALPureParser class.

} // end namespace.
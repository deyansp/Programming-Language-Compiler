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
        enum PALType
        {
           Boolean,
           Integer,
           Real,
           Invalid
        }

        struct Declaration
        {
            PALType varType;

        }

        Dictionary<String, PALType> variables;
    }
}

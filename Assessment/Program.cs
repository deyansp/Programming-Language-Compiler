using System;
using System.IO;
using System.Collections.Generic;
using AllanMilne.Ardkit;

namespace AllanMilne.PALCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid usage: <filename>");
                return;
            }

            //--- Open the input source file.
            StreamReader infile = null;
            try
            {
                infile = new StreamReader(args[0]);
            }
            catch (IOException e) { Console.WriteLine(e.ToString()); }

            //--- Parsing and then outputting any syntax errors found
            PALParser parser = new PALParser();
            parser.Parse(infile);

            foreach(var err in parser.Errors)
            {
                Console.WriteLine(err.ToString());
            }

            try
            {
                infile.Close();
            }
            catch (IOException e) { Console.WriteLine(e.ToString()); }

        } // end Main method.
    }
}


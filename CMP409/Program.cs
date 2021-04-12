/*
	File:    CountLets.cs
	Purpose: Application to count the let statements in a Block-1 program.
	Author:  Allan C. Milne.
	Version: 1.1
	Date:    29th January 2010.

	Usage:    CountLets <filename>

	Uses: Ardkit.dll, Block1Scanner.
	Exposes: CountLets.

	Description:
	A command-line application to count the number of let statements in a Block-1 source program.
*/


using System;
using System.IO;
using System.Collections.Generic;

using AllanMilne.Ardkit;
using AllanMilne.Block1;

//=== The main class and application entry point.
class CountLets
{

    private static ComponentInfo info = new ComponentInfo(
       "CountLets", "1.1", "January 2010", "A.C.Milne", "Count the number of assignment (let) statements");
    public static ComponentInfo Info
    { get { return info; } }

    //=== the compiler entry point.
    public static void Main(String[] args)
    {

        if (args.Length != 1)
        {
            Console.WriteLine("Invalid usage: CountLets <filename>");
            return;
        }

        //--- Open the input source file.
        StreamReader infile = null;
        try
        {
            infile = new StreamReader(args[0]);
        }
        catch (IOException e) { ioError("opening", args[0], e); return; }

        //--- Do what you gotta do!
        CountLets program = new CountLets(infile);
        program.start(args[0]);

        try
        {
            infile.Close();
        }
        catch (IOException e) { ioError("closing", args[0], e); return; }

    } // end Main method.


    //--- The instance attributes.
    private TextReader source;    // the input source file stream.

    //--- constructor method.
    public CountLets(TextReader src)
    { source = src; }

    //--- The application logic.
    private void start(String filename)
    {
        List<ICompilerError> errors = new List<ICompilerError>();
        Block1Scanner scanner = new Block1Scanner();

        prologue(filename);
        scanner.Init(source, errors);

        int count = 0;
        while (!scanner.EndOfFile)
        {
            if (scanner.NextToken().TokenType.Equals("let"))
                count++;
        }
        Console.WriteLine("{0:d} assignment (let) statements found.", count);
        Console.WriteLine();
        Console.WriteLine("=== End of program.");
    } // end start method.

    //--- Display title prologue text.
    private void prologue(String filename)
    {
        Console.WriteLine();
        Console.WriteLine(Ardkit.Info.Copyright);
        Console.WriteLine(CountLets.Info.Copyright);
        Console.WriteLine("Counting assignments in {0:s}. ", filename);
        Console.WriteLine();
    } // end prologue method.

    //--- An I/O exception has been caught.
    private static void ioError(String function, String filename, IOException e)
    {
        Console.WriteLine("An I/O error occurred {0:s} file {1:s}.", function, filename);
        Console.WriteLine(e);
    } // end ioError method.

} // end Block1 class.
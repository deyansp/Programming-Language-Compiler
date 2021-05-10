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

            // compiler test mode
            else if (args[0] == "TXT_FILES_TEST")
            {
                runTestsFromTxtFiles();
            }

            // otherwise compile the file provided as an argument
            else
                runCompiler(args[0]);

        } // end Main method.
        
        static void runTestsFromTxtFiles()
        {
            // getting the root project directory
            DirectoryInfo baseDir = new DirectoryInfo(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
            
            // the folder with code txt files
            var testDir = baseDir.GetDirectories("Tests");

            if (testDir.Length != 1)
            {
                Console.WriteLine("Unable to locate 'Tests' folder");
                return;
            }

            // storing all txt file paths in a list
            List<FileInfo> testFiles = new List<FileInfo>(testDir[0].EnumerateFiles("*.txt", SearchOption.AllDirectories));

            // checking each file
            foreach(var test in testFiles)
            {
                Console.WriteLine("\nFile: " + test.Name);
                runCompiler(test.FullName);
            }
        }
        
        static void runCompiler(string filePath)
        {
            // Open the input source file.
            StreamReader infile = null;
            try
            {
                infile = new StreamReader(filePath);
            }
            catch (IOException e) { Console.WriteLine(e.ToString()); }

            // Parsing and then outputting any errors found
            PALParser parser = new PALParser();
            parser.Parse(infile);

            foreach (var err in parser.Errors)
            {
                Console.WriteLine(err.ToString());
            }

            try
            {
                infile.Close();
            }
            catch (IOException e) { Console.WriteLine(e.ToString()); }
        }
    }
}


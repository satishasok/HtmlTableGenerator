using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlTableGeneratorErrors;

namespace HtmlTableGenerator
{
    class Program
    {
        /// <Summary>
        ///     Main entry point for the commandline app 
        /// </Summary>
        static int Main(string[] args)
        {
            // Handling command-line arguments
            string inputFilePath = "";
            // show command-line usage help
            if (args.Length != 1 || args[0] == "-h" || args[0] == "-help" || args[0] == "usage")
            {
                ShowUsage();
                return 1; //fail
            }
            else
            {
                // extract the input file name from the command line argument
                inputFilePath = args[0];
            }

            HtmlTableGenerator htmlTableGenerator = null;
            try
            {
                // If the input file does not exist display an error message
                if (!File.Exists(inputFilePath))
                {
                    throw new InputError(HtmlTableGeneratorErrorCodes.file_does_not_exist, inputFilePath);
                }

                htmlTableGenerator = new HtmlTableGenerator(inputFilePath);

                htmlTableGenerator.LoadTableStringsFromFile();

            }
            catch (InputError inpErr)
            {
                inpErr.PrintMessage();
                return 1; // fail
            }
            catch (Exception e)
            {
                // error verifying password. Display error message.
                System.Console.WriteLine(string.Format(@"HtmlTableGenerator: Error while while parsing the input file ""{0}"" Error details: {1}", inputFilePath, e.Message));
                return 1; // fail
            }

            if (htmlTableGenerator == null)
            {
                // error verifying password. Display error message.
                System.Console.WriteLine(string.Format(@"HtmlTableGenerator: Error while while parsing the input file ""{0}""", inputFilePath));
                return 1; // fail
            }

            try
            {
                // builds the HtmlTable
                htmlTableGenerator.BuildTheTable();
                // dumps the output table in both stdout and to a file.
                htmlTableGenerator.OutputTheTableAsHtml();
            }
            catch (Exception e)
            {
                // error verifying password. Display error message.
                System.Console.WriteLine(string.Format(@"HtmlTableGenerator: Error while while building and printing tree the input file ""{0}"" Error details: {1}", inputFilePath, e.Message));
                return 1; // fail
            }

            return 0; // success
        }

        /// <Summary>
        ///     Displays the command line usage help text 
        /// </Summary>
        /// <arguments>
        ///     No Arguments
        /// </arguments>
        /// <returns>
        ///     void
        /// </returns>
        static public void ShowUsage()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  HtmlTableGenerator <inputFilePath> ");
            System.Console.WriteLine("     Where,");
            System.Console.WriteLine("        inputFilePath is full or relative path to the input file.");
        }
    }
}

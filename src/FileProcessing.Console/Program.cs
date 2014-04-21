using System;
using System.IO;
using BJSS.FileProcessing.Xml;
using BJSS.FileProcessing;
using System.Threading;

namespace BJSS.FileProcessing.Console
{
    class Program
    {
        static void Main()
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("Test Console");
            System.Console.WriteLine();
            System.Console.ResetColor();

            System.Console.WriteLine("Enter INPUT folder (Default: '{0}'):", Environment.CurrentDirectory);
            string inputFolder = System.Console.ReadLine();

            System.Console.WriteLine("Enter OUTPUT folder (Default: '{0}\\Output'):", Environment.CurrentDirectory);
            string outputFolder = System.Console.ReadLine();

            System.Console.WriteLine("Enter XSLT file (Default: 'books-to-html.xslt'):");
            string xsltFile = System.Console.ReadLine();

            if (string.IsNullOrEmpty(inputFolder))
            {
                inputFolder = Environment.CurrentDirectory;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Environment.CurrentDirectory + "\\Output";
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            if (string.IsNullOrEmpty(xsltFile))
            {
                xsltFile = "books-to-html.xslt";
            }

            // Define an xml file transformer.
            var transformer = new DefaultXmlFileTransformer(xsltFile);

            // Declare file watcher with init params.
            var fileWatcher = new DefaultFileSystemWatcher(inputFolder, "*.xml");

            // Create file processor using default file system.
            var fileProcessor = new FileProcessor(fileWatcher, transformer)
            {
                OutputLocation = new OutputLocation
                {
                    Path = outputFolder,
                    NamingConvention = (path) => Path.GetFileNameWithoutExtension(path) + ".html"
                }
            };

            // Error handler.
            fileProcessor.Error = err =>
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(err);
                System.Console.ResetColor();
            };

            // File processing handler.
            fileProcessor.FileProcessed = fileInfo =>
            {
                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.WriteLine(fileInfo.InputFile);
                System.Console.ResetColor();
            };

            fileProcessor.Started = () =>
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("Listening for files...");
                System.Console.WriteLine();
                System.Console.ResetColor();
            };

            fileProcessor.Stopped = () =>
            {
                System.Console.WriteLine("Process stopped!");
                System.Console.WriteLine();
            };

            // Start file processor.
            fileProcessor.Start();

            System.Console.WriteLine("Press any key to stop");
            System.Console.WriteLine();
            System.Console.ReadLine();

            fileProcessor.Stop();

            Thread.Sleep(1000);
        }
    }
}

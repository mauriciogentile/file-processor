using System;

namespace BJSS.FileProcessing
{
    public class FileProcessedEventArgs : EventArgs
    {
        public FileProcessedEventArgs(string inputFile, string outputFile)
        {
            InputFile = inputFile;
            OutputFile = outputFile;
        }

        public string InputFile { get; private set; }
        public string OutputFile { get; private set; }
    }
}
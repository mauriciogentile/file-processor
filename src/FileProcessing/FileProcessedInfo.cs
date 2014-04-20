using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BJSS.FileProcessing
{
    public class FileProcessedInfo
    {
        public FileProcessedInfo(string inputFile, string outputFile)
        {
            InputFile = inputFile;
            OutpuFile = outputFile;
        }

        public string InputFile { get; private set; }
        public string OutpuFile { get; private set; }
    }
}

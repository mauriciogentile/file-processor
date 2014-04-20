using System;

namespace BJSS.FileProcessing
{
    public interface IFileProcessor
    {
        Action<FileProcessedInfo> FileProcessed { get; set; }
        Action Started { get; set; }
        Action Stopped { get; set; }
        Action<Exception> Error { get; set; }
        void Start(string outputPath);
        void Stop();
    }
}
using System;
using System.IO;

namespace BJSS.FileProcessing
{
    public interface IFileProcessor
    {
        event EventHandler<FileProcessedEventArgs> FileProcessed;
        event EventHandler Started;
        event EventHandler Stopped;
        event ErrorEventHandler Error;
        void Start();
        void Stop();
    }
}
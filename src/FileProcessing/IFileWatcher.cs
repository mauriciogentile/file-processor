using System;
using System.IO;

namespace BJSS.FileProcessing
{
    public interface IFileWatcher : IDisposable
    {
        bool Enabled { get; set; }
        Action<string> FileDetected { get; set; }
    }
}

using System;

namespace BJSS.FileProcessing
{
    public interface IFileWatcher : IDisposable
    {
        bool EnableRaisingEvents { get; set; }
        Action<string> FileDetected { get; set; }
    }
}

using System;

namespace Ringo.FileProcessing
{
    public interface IFileWatcher : IDisposable
    {
        bool EnableRaisingEvents { get; set; }
        Action<string> FileDetected { get; set; }
    }
}

using System;
using System.IO;
using BJSS.FileProcessing.Util;
using System.Threading;

namespace BJSS.FileProcessing
{
    /// <summary>
    /// A FileSystemWatcher that wraps the FileSystemWatcher class. <see cref="System.IO.FileSystemWatcher"/>
    /// </summary>
    public class DefaultFileSystemWatcher : Disposable, IFileWatcher
    {
        readonly FileSystemWatcher _fileSystemWatcher;

        /// <summary>
        /// Creates an instance of the class DefaultFileSystemWatcher.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="filters">e.g. *.*, *.xml, *.txt</param>
        /// <param name="includeSubdirectories">Gets or sets a value indicating whether subdirectories within the specified path should be monitored.</param>
        public DefaultFileSystemWatcher(string folderPath, string filters = "*.*", bool includeSubdirectories = false)
        {
            _fileSystemWatcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Path = folderPath,
                Filter = filters
            };

            _fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
        }

        /// <summary>
        /// A lambda expression that handles the detected file.
        /// </summary>
        public Action<string> FileDetected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating wether the component is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return _fileSystemWatcher.EnableRaisingEvents; }
            set { _fileSystemWatcher.EnableRaisingEvents = value; }
        }

        void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            NotifySubscribers(e.FullPath);
        }

        void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            NotifySubscribers(e.FullPath);
        }

        void NotifySubscribers(string filePath)
        {
            if (FileDetected != null)
            {
                // Waits for any file holder to release the file
                // as FileSystemWatcher could start emitting events while the file still being taken by the os.
                short tries = 5;
                while (IsFileLocked(filePath) && tries >= 0)
                {
                    tries--;
                    Thread.Sleep(100);
                }
                
                // If file still locked let the process to fail and emit error.
                FileDetected(filePath);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Created -= _fileSystemWatcher_Created;
                _fileSystemWatcher.Renamed -= _fileSystemWatcher_Renamed;
                _fileSystemWatcher.Dispose();
            }

            base.Dispose();
        }

        static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}

using BJSS.FileProcessing.Util;
using System;
using System.IO;

namespace BJSS.FileProcessing
{
    /// <summary>
    /// A component for processing and transform files using pluggable transformers, file watchers and file writers.
    /// </summary>
    public class FileProcessor : Disposable, IFileProcessor
    {
        static readonly object locker = new object();
        readonly IFileWatcher _fileSystemWatcher;
        readonly IFileSystem _fileSystem;
        readonly IFileTransformer _fileTransformer;
        string _outputPath;

        public Action<FileProcessedInfo> FileProcessed { get; set; }
        public Action Started { get; set; }
        public Action Stopped { get; set; }
        public Action<Exception> Error { get; set; }

        /// <summary>
        /// Creates an instance of the FileProcessor class using LocalFileSystem as default.
        /// </summary>
        /// <param name="fileTransformer"><see cref="BJSS.FileProcessing.IFileTransformer"/></param>
        /// <param name="fileSystemWatcher"><see cref="BJSS.FileProcessing.IFileWatcher"/></param>
        public FileProcessor(IFileTransformer fileTransformer, IFileWatcher fileSystemWatcher)
            : this(fileTransformer, fileSystemWatcher, new LocalFileSystem())
        {
        }

        /// <summary>
        /// Creates an instance of the FileProcessor class.
        /// </summary>
        /// <param name="fileTransformer"><see cref="BJSS.FileProcessing.IFileTransformer"/></param>
        /// <param name="fileSystemWatcher"><see cref="BJSS.FileProcessing.IFileWatcher"/></param>
        /// <param name="fileSystem"><see cref="BJSS.FileProcessing.IFileSystem"/></param>
        public FileProcessor(IFileTransformer fileTransformer, IFileWatcher fileSystemWatcher, IFileSystem fileSystem)
        {
            if (fileTransformer == null)
            {
                throw new ArgumentNullException("fileTransformer");
            }

            if (fileSystemWatcher == null)
            {
                throw new ArgumentNullException("fileSystemWatcher");
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            _fileSystem = fileSystem;
            _fileTransformer = fileTransformer;
            _fileSystemWatcher = fileSystemWatcher;

            // Subscribe to file detection.
            _fileSystemWatcher.FileDetected = filePath => ProcessFile(filePath);
        }

        /// <summary>
        /// Starts listening for files based on the file watcher parameters. <see cref="IFileWatcher"/>
        /// </summary>
        /// <param name="outputPath">The destination location for the transformed files.</param>
        public void Start(string outputPath)
        {
            lock (locker)
            {
                if (!_fileSystem.LocationExists(outputPath))
                {
                    throw new DirectoryNotFoundException("'outputPath' is pointing to an unexisting folder or location.");
                }

                if (_fileSystemWatcher.Enabled)
                {
                    return;
                }

                _outputPath = outputPath;

                _fileSystemWatcher.Enabled = true;

                if (Started != null)
                {
                    Started();
                }
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                if (!_fileSystemWatcher.Enabled)
                {
                    return;
                }

                _fileSystemWatcher.Enabled = false;

                if (Stopped != null)
                {
                    Stopped();
                }
            }
        }

        protected virtual void ProcessFile(string filePath)
        {
            try
            {
                // New file location for the output file.
                string newFilePath = _fileSystem.Combine(_outputPath, _fileSystem.GetFileName(filePath));

                using (var stream = new MemoryStream())
                {
                    // Writes the transformation into a stream.
                    _fileTransformer.Transform(filePath, stream);

                    // Writes the stream on the file system.
                    _fileSystem.CreateFile(stream, newFilePath);
                    stream.Close();
                }

                if (FileProcessed != null)
                {
                    // Emits input and output file locations.
                    FileProcessed(new FileProcessedInfo(filePath, newFilePath));
                }
            }
            catch (Exception exc)
            {
                if (Error != null)
                {
                    Error(new ApplicationException("Error processing file '" + filePath + "'.", exc));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                _fileSystemWatcher.Dispose();
            }

            base.Dispose();
        }
    }
}

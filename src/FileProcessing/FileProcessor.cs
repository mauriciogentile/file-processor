using BJSS.FileProcessing.Util;
using System;
using System.IO;
using Validation;

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

        /// <summary>
        /// Occurs when a file is detected by the <see cref="BJSS.FileProcessing.IFileSystem"/> and then processed.
        /// </summary>
        public event EventHandler<FileProcessedEventArgs> FileProcessed;
        
        /// <summary>
        /// Occurs when there is an error processing a file.
        /// </summary>
        public event ErrorEventHandler Error;
        
        public event EventHandler Started;
        public event EventHandler Stopped;

        /// <summary>
        /// Sets up output path and naming convention for new generated files.
        /// </summary>
        public OutputLocation OutputLocation { get; set; }

        /// <summary>
        /// Creates an instance of the FileProcessor class using LocalFileSystem as default.
        /// </summary>
        public FileProcessor(IFileWatcher fileSystemWatcher, IFileTransformer fileTransformer)
            : this(fileSystemWatcher, fileTransformer, new LocalFileSystem())
        {
        }

        /// <summary>
        /// Creates an instance of the FileProcessor class.
        /// </summary>
        public FileProcessor(IFileWatcher fileSystemWatcher, IFileTransformer fileTransformer, IFileSystem fileSystem)
        {
            Requires.NotNull(fileTransformer, "fileTransformer");
            Requires.NotNull(fileSystemWatcher, "fileSystemWatcher");
            Requires.NotNull(fileSystem, "fileSystem");

            _fileSystem = fileSystem;
            _fileTransformer = fileTransformer;
            _fileSystemWatcher = fileSystemWatcher;

            // Subscribe to file detection.
            _fileSystemWatcher.FileDetected = ProcessFile;
        }

        /// <summary>
        /// Starts listening for files and process them.
        /// </summary>
        public void Start()
        {
            lock (locker)
            {
                if (_fileSystemWatcher.EnableRaisingEvents)
                {
                    return;
                }

                if (OutputLocation == null)
                {
                    throw new InvalidOperationException("'OutputLocation' has not been set.");
                }

                if (OutputLocation.Path == null || !_fileSystem.LocationExists(OutputLocation.Path))
                {
                    throw new DirectoryNotFoundException("'outputLocation' is pointing to an unexisting folder or location.");
                }

                _fileSystemWatcher.EnableRaisingEvents = true;

                if (Started != null)
                {
                    Started(this, EventArgs.Empty);
                }
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                if (!_fileSystemWatcher.EnableRaisingEvents)
                {
                    return;
                }

                _fileSystemWatcher.EnableRaisingEvents = false;

                if (Stopped != null)
                {
                    Stopped(this, EventArgs.Empty);
                }
            }
        }

        protected virtual void ProcessFile(string filePath)
        {
            try
            {
                // Make sure it has a naming convention for the new file's name;
                OutputLocation.NamingConvention = OutputLocation.NamingConvention ?? new Func<string, string>((path) => _fileSystem.GetFileName(path));

                // New file location for the output file.
                string newFilePath = _fileSystem.Combine(OutputLocation.Path, OutputLocation.NamingConvention(filePath));

                using (var stream = new MemoryStream())
                {
                    // Writes the transformation into a stream.
                    _fileTransformer.Transform(filePath, stream);

                    // Writes the stream on the file system.
                    _fileSystem.CreateFile(stream, newFilePath);

                    // Releases stream.
                    stream.Close();
                }

                if (FileProcessed != null)
                {
                    // Emits input and output file locations.
                    FileProcessed(this, new FileProcessedEventArgs(filePath, newFilePath));
                }
            }
            catch (Exception exc)
            {
                if (Error != null)
                {
                    Error(this, new ErrorEventArgs(new ApplicationException("Error processing file '" + filePath + "'.", exc)));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                _fileSystemWatcher.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

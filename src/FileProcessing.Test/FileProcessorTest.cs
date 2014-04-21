using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BJSS.FileProcessing.Xml;
using Moq;
using NUnit.Framework;
using System.Threading;

namespace BJSS.FileProcessing.Test
{
    [TestFixture]
    public class FileProcessorTest
    {
        Mock<IFileTransformer> _transformerMock;
        Mock<IFileWatcher> _fileWatcherMock;
        Mock<IFileSystem> _fileSystemMock;
        OutputLocation _defaultOutputLocation;

        [SetUp]
        public void SetUp()
        {
            _defaultOutputLocation = new OutputLocation { Path = Environment.CurrentDirectory };

            _transformerMock = new Mock<IFileTransformer>();
            _transformerMock.
                Setup(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()));

            _fileWatcherMock = new Mock<IFileWatcher>()
                .SetupProperty(x => x.FileDetected)
                .SetupProperty(x => x.EnableRaisingEvents);

            _fileSystemMock = new Mock<IFileSystem>();
            _fileSystemMock.Setup(x => x.CreateFile(It.IsAny<Stream>(), It.IsAny<string>()));
            _fileSystemMock.Setup(x => x.LocationExists(It.IsAny<string>())).Returns<string>((path) => Directory.Exists(path));
            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>((path) => Path.GetFileName(path));
            _fileSystemMock.Setup(x => x.Combine(It.IsAny<string[]>())).Returns<string[]>((paths) => Path.Combine(paths));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_filewatcher_is_null()
        {
            new FileProcessor(null, _transformerMock.Object, _fileSystemMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_transformer_is_null()
        {
            new FileProcessor(_fileWatcherMock.Object, null, _fileSystemMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_filesystem_is_null()
        {
            new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void start_should_fail_if_no_output_location_is_set()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object);

            target.Start();
        }

        [Test]
        public void should_call_onstarted_handler_when_started_for_the_first_time()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            bool startedCalled = false;

            target.Started += delegate { startedCalled = true; };

            target.Start();

            Assert.IsTrue(startedCalled);
        }

        [Test]
        public void should_not_call_onstarted_handler_when_try_to_start_it_multiple_times()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            int startedCalledCount = 0;

            target.Started += delegate { startedCalledCount++; };

            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(target.Start));
            }

            Task.WaitAll(tasks.ToArray());
            
            Assert.IsTrue(startedCalledCount == 1);
        }

        [Test]
        public void should_call_onstopped_handler_when_stopped()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            bool stoppedCalled = false;

            target.Stopped += delegate { stoppedCalled = true; };

            target.Start();
            target.Stop();

            Assert.IsTrue(stoppedCalled);
        }

        [Test]
        public void should_not_call_onstopped_handler_when_try_to_stop_it_multiple_times()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            int stoppedCalledCount = 0;

            target.Stopped += delegate { stoppedCalledCount++; };

            target.Start();

            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(target.Stop));
            }

            Task.WaitAll(tasks.ToArray());
            
            Assert.AreEqual(stoppedCalledCount, 1);
        }

        [Test]
        public void should_call_error_callback_if_process_fails()
        {
            _transformerMock.
                Setup(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()))
                .Throws(new InvalidOperationException());

            Exception expected = null;

            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            target.Error += delegate(object sender, ErrorEventArgs arg)
            {
                expected = arg.GetException();
            };

            target.Start();

            var newFile = Guid.NewGuid() + ".temp.xml";

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            // Assertions
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.GetType(), typeof(ApplicationException));
            Assert.AreEqual(expected.InnerException.GetType(), typeof(InvalidOperationException));
            _transformerMock.Verify(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _fileSystemMock.Verify(x => x.CreateFile(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_call_file_detected_handler_after_processing()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            string actual = null;

            var reseter = new AutoResetEvent(false);

            target.FileProcessed += delegate(object sender, FileProcessedEventArgs arg)
            {
                actual = arg.OutputFile;
                reseter.Set();
            };

            target.Start();

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            reseter.WaitOne(2000);

            Assert.AreEqual(newFile, actual);
        }

        [Test]
        public void should_save_file_in_output_folder_after_tranformation()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };

            target.Start();

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            // Assertions
            _transformerMock.Verify(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _fileSystemMock.Verify(x => x.CreateFile(It.IsAny<Stream>(), newFile), Times.Once);
        }

        [Test]
        public void should_create_new_files_using_naming_convention()
        {
            var target = new FileProcessor(_fileWatcherMock.Object, _transformerMock.Object, _fileSystemMock.Object)
            {
                OutputLocation = _defaultOutputLocation
            };
            target.OutputLocation.NamingConvention = (path) => "MyConvention.xyz";

            target.Start();

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");
            var expectedNewFile = Path.Combine(Environment.CurrentDirectory, "MyConvention.xyz");

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            // Assertions
            _transformerMock.Verify(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _fileSystemMock.Verify(x => x.CreateFile(It.IsAny<Stream>(), expectedNewFile), Times.Once);
        }
    }
}

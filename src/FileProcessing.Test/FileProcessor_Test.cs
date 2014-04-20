using System;
using System.IO;
using System.Threading.Tasks;
using BJSS.FileProcessing.Xml;
using Moq;
using NUnit.Framework;
using System.Threading;

namespace BJSS.FileProcessing.Test
{
    [TestFixture]
    public class FileProcessor_Test
    {
        Mock<IFileTransformer> _transformerMock;
        Mock<IFileWatcher> _fileWatcherMock;
        Mock<IFileSystem> _fileSystemMock;

        [SetUp]
        public void SetUp()
        {
            _transformerMock = new Mock<IFileTransformer>();
            _transformerMock.
                Setup(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()));

            _fileWatcherMock = new Mock<IFileWatcher>()
                .SetupProperty(x => x.FileDetected)
                .SetupProperty(x => x.Enabled);

            _fileSystemMock = new Mock<IFileSystem>();
            _fileSystemMock.Setup(x => x.CreateFile(It.IsAny<Stream>(), It.IsAny<string>()));
            _fileSystemMock.Setup(x => x.LocationExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>((path) => Path.GetFileName(path));
            _fileSystemMock.Setup(x => x.Combine(It.IsAny<string[]>())).Returns<string[]>((paths) => Path.Combine(paths));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_first_argument_is_null()
        {
            new FileProcessor(null, _fileWatcherMock.Object, _fileSystemMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_second_argument_is_null()
        {
            new FileProcessor(_transformerMock.Object, null, _fileSystemMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void constructor_should_fail_if_third_argument_is_null()
        {
            new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, null);
        }

        [Test]
        public void should_call_onstarted_handler_when_started_for_the_first_time()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            bool startedCalled = false;

            target.Started = () => { startedCalled = true; };

            target.Start(Environment.CurrentDirectory);

            Assert.IsTrue(startedCalled);
        }

        [Test]
        public void should_not_call_onstarted_handler_when_started_twice()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            int startedCalledCount = 0;

            target.Started = () => { startedCalledCount++; };

            target.Start(Environment.CurrentDirectory);
            target.Start(Environment.CurrentDirectory);

            Assert.IsTrue(startedCalledCount == 1);
        }

        [Test]
        public void should_call_onstopped_handler_when_stopped()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            bool stoppedCalled = false;

            target.Stopped = () => { stoppedCalled = true; };

            target.Start(Environment.CurrentDirectory);
            target.Stop();

            Assert.IsTrue(stoppedCalled);
        }

        [Test]
        public void should_not_call_onstopped_handler_when_stopped_twice()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            int stoppedCalledCount = 0;

            target.Stopped = () => { stoppedCalledCount++; };

            target.Start(Environment.CurrentDirectory);
            target.Stop();
            target.Stop();

            Assert.AreEqual(stoppedCalledCount, 1);
        }

        [Test]
        public void should_call_handle_error_if_process_fails()
        {
            var transformerMock = new Mock<IFileTransformer>();
            transformerMock.
                Setup(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()))
                .Throws(new InvalidOperationException());

            Exception expected = null;

            var target = new FileProcessor(transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object)
            {
                Error = exc =>
                {
                    expected = exc;
                }
            };

            target.Start(Environment.CurrentDirectory);

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            // Assertions
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.GetType(), typeof(ApplicationException));
            Assert.AreEqual(expected.InnerException.GetType(), typeof(InvalidOperationException));
            transformerMock.Verify(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _fileSystemMock.Verify(x => x.CreateFile(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void should_call_file_detected_handler_after_processing()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            string actual = null;

            var reseter = new AutoResetEvent(false);

            target.FileProcessed = file =>
            {
                actual = file.OutputFile;
                reseter.Set();
            };

            target.Start(Environment.CurrentDirectory);

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");
            var fileName = Path.GetFileName(newFile);

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            reseter.WaitOne(2000);

            Assert.AreEqual(newFile, actual);
        }

        [Test]
        public void should_save_file_in_output_folder_after_tranformation()
        {
            var target = new FileProcessor(_transformerMock.Object, _fileWatcherMock.Object, _fileSystemMock.Object);

            target.Start(Environment.CurrentDirectory);

            var newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".temp.xml");

            Task.Factory.StartNew(() => _fileWatcherMock.Object.FileDetected(newFile)).Wait();

            // Assertions
            _transformerMock.Verify(x => x.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
            _fileSystemMock.Verify(x => x.CreateFile(It.IsAny<Stream>(), newFile), Times.Once);
        }
    }
}

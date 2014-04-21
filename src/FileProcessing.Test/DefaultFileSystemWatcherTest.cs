using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BJSS.FileProcessing.Test
{
    [TestFixture]
    public class DefaultFileSystemWatcherTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_fail_if_folder_is_not_specified()
        {
            new DefaultFileSystemWatcher(null)
            {
                EnableRaisingEvents = true
            };
        }

        [Test]
        public void should_detect_new_files_in_folder_if_enabled()
        {
            var target = new DefaultFileSystemWatcher(Environment.CurrentDirectory)
            {
                EnableRaisingEvents = true
            };

            string newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".xml");
            string actual = null;

            var reseter = new AutoResetEvent(false);

            target.FileDetected = file =>
            {
                actual = file;
                reseter.Set();
            };

            Task.Factory.StartNew(() => File.Create(newFile).Close());

            reseter.WaitOne(2000);

            target.Dispose();

            Assert.AreEqual(newFile, actual);
        }

        [Test]
        public void should_not_detect_new_files_in_folder_if_not_enabled()
        {
            var target = new DefaultFileSystemWatcher(Environment.CurrentDirectory)
            {
                EnableRaisingEvents = false
            };

            string newFile = Guid.NewGuid() + ".xml";
            string expected = null;

            var reseter = new AutoResetEvent(false);

            target.FileDetected = file =>
            {
                expected = file;
                reseter.Set();
            };

            Task.Factory.StartNew(() => File.Create(newFile).Close());

            reseter.WaitOne(2000);

            target.Dispose();

            Assert.IsNull(expected);
        }

        [Test]
        public void should_detect_renamed_files_in_folder()
        {
            var target = new DefaultFileSystemWatcher(Environment.CurrentDirectory, "*.xml")
            {
                EnableRaisingEvents = true
            };

            string newFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".nonxml");
            string renamedFile = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".renamed.xml");
            string actual = null;

            var reseter = new AutoResetEvent(false);

            target.FileDetected = file =>
            {
                actual = file;
                reseter.Set();
            };

            Task.Factory
                .StartNew(() => File.Create(newFile).Close())
                .ContinueWith(t => File.Move(newFile, renamedFile));

            reseter.WaitOne(3000);

            target.Dispose();

            Assert.AreEqual(renamedFile, actual);
        }

        [Test]
        public void should_not_detect_files_if_disposed()
        {
            var target = new DefaultFileSystemWatcher(Environment.CurrentDirectory)
            {
                EnableRaisingEvents = true
            };

            string newFile0 = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".xml");
            string newFile1 = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ".xml");
            string actual = null;

            var reseter = new AutoResetEvent(false);

            target.FileDetected = file =>
            {
                actual = file;
                reseter.Set();
            };

            Task.Factory.StartNew(() => File.Create(newFile0).Close());

            reseter.WaitOne(2000);

            Assert.AreEqual(newFile0, actual);

            target.Dispose();

            Task.Factory.StartNew(() => File.Create(newFile1).Close());

            reseter.WaitOne(2000);

            Assert.AreEqual(newFile0, actual);
        }
    }
}

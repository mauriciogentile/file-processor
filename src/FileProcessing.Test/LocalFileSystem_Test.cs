using System;
using System.IO;
using NUnit.Framework;
using System.Reflection;

namespace BJSS.FileProcessing.Test
{
    [TestFixture]
    public class LocalFileSystem_Test
    {
        [Test]
        public void exists_should_return_true_if_file_exist()
        {
            var target = new LocalFileSystem();
            var actual = target.Exists(Assembly.GetExecutingAssembly().Location);

            Assert.IsTrue(actual);
        }

        [Test]
        public void exists_should_return_false_if_file_doesnt_exist()
        {
            var target = new LocalFileSystem();
            var exists = target.Exists(Guid.NewGuid().ToString());

            Assert.IsFalse(exists);
        }

        [Test]
        public void combine_should_return_a_combination_of_paths()
        {
            var target = new LocalFileSystem();

            var expected = Environment.CurrentDirectory + "\\Momo";
            var actual = target.Combine(Environment.CurrentDirectory, "Momo");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void get_filename_should_get_file_name_given_a_full_path()
        {
            var target = new LocalFileSystem();

            var expected = "Momo.txt";
            var actual = target.GetFileName(Environment.CurrentDirectory + "\\Momo.txt");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void should_create_a_new_file_given_a_stream()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("Hello!");
                    writer.Flush();

                    var file = Guid.NewGuid().ToString();

                    var target = new LocalFileSystem();
                    target.CreateFile(stream, file);

                    FileAssert.AreEqual(stream, File.Open(file, FileMode.Open));
                }
            }
        }
    }
}

using System;
using System.IO;
using BJSS.FileProcessing.Xml;
using NUnit.Framework;

namespace BJSS.FileProcessing.Test.Xml
{
    [TestFixture]
    public class DefaultXmlFileTransformer_Test
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void should_fail_if_argument_is_null()
        {
            new DefaultXmlFileTransformer(null);
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void should_fail_if_file_doesnt_exist()
        {
            new DefaultXmlFileTransformer(Guid.NewGuid().ToString());
        }

        [Test]
        public void should_transform_to_file_from_xml_using_xslt()
        {
            var target = new DefaultXmlFileTransformer("TestFiles\\books-to-html.xslt");

            using (var stream = File.Create("books.html"))
            {
                target.Transform("TestFiles\\books.xml", stream);
                stream.Flush();
                stream.Close();
            }

            FileAssert.AreEqual(new FileInfo("TestFiles\\books.html"), new FileInfo("books.html"));
        }
    }
}

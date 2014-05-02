using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Validation;

namespace Ringo.FileProcessing.Xml
{
    /// <summary>
    /// Transforms an input file into a xslt transformation using the XslCompiledTransform class. <see cref="System.Xml.Xsl.XslCompiledTransform"/>
    /// </summary>
    public sealed class DefaultXmlFileTransformer : IFileTransformer
    {
        static object locker = new object();
        readonly string _xsltFilePath;
        XslCompiledTransform _xslCompiledTransform;

        /// <param name="xsltFilePath">The path to the XSLT file</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public DefaultXmlFileTransformer(string xsltFilePath)
        {
            Requires.NotNullOrEmpty(xsltFilePath, "xsltFilePath");

            if (!File.Exists(xsltFilePath))
            {
                throw new FileNotFoundException("XSLT file was not found");
            }

            _xsltFilePath = xsltFilePath;
        }

        /// <summary>
        /// Transform specified file using XslCompiledTransform classs and writes the output into a stream.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="output"></param>
        public void Transform(string filePath, Stream output)
        {
            try
            {
                // Create a writer for writing the transformed file.
                using (var writer = XmlWriter.Create(output))
                {
                    var transform = GetOrLoadTransform();
                    // Execute the transformation.
                    transform.Transform(new XPathDocument(filePath), writer);
                }
            }
            catch (Exception exc)
            {
                throw new ApplicationException("Error transforming file '" + filePath + "'.", exc);
            }
        }

        XslCompiledTransform GetOrLoadTransform()
        {
            if (_xslCompiledTransform == null)
            {
                lock (locker)
                {
                    if (_xslCompiledTransform == null)
                    {
                        // Create and load the transform.
                        _xslCompiledTransform = new XslCompiledTransform();
                        _xslCompiledTransform.Load(_xsltFilePath);
                    }
                }
            }

            return _xslCompiledTransform;
        }
    }
}

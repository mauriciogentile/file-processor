using System.IO;

namespace Ringo.FileProcessing
{
    public interface IFileTransformer
    {
        void Transform(string filePath, Stream output);
    }
}

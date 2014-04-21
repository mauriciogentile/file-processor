using System.IO;

namespace BJSS.FileProcessing
{
    public interface IFileTransformer
    {
        void Transform(string filePath, Stream output);
    }
}

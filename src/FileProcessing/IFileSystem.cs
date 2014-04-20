﻿using System.IO;

namespace BJSS.FileProcessing
{
    public interface IFileSystem
    {
        bool Exists(string filePath);
        bool LocationExists(string locationPath);
        string Combine(params string[] paths);
        void CreateFile(Stream stream, string fileName);
    }
}

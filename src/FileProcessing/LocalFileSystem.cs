﻿using System;
using System.IO;

namespace BJSS.FileProcessing
{
    /// <summary>
    /// A class that wraps a file system write operation.
    /// </summary>
    public class LocalFileSystem : IFileSystem
    {
        /// <summary>
        /// Creates an instance of the LocalFileSystem class.
        /// </summary>
        /// <param name="folderPath">The folder to be written</param>
        public LocalFileSystem()
        {
        }

        /// <summary>
        /// Determines wether the file exists.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Determines wether a folder exists.
        /// </summary>
        /// <param name="locationPath"></param>
        /// <returns></returns>
        public bool LocationExists(string locationPath)
        {
            return Directory.Exists(locationPath);
        }

        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths"></param>
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Returns the file name and extension of the specified path.
        /// </summary>
        /// <param name="path"></param>
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Creates or overrides a file in the specified path.
        /// </summary>
        /// <param name="inputStream">The content</param>
        /// <param name="fileName">The new file to be created</param>
        public void CreateFile(Stream inputStream, string destinationPath)
        {
            using (FileStream fileStream = File.Create(destinationPath))
            {
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }
        }
    }
}
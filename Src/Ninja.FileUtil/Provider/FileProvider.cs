﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ninja.FileUtil.Provider
{
    public class FileProvider : IFileProvider
    {
        private readonly ProviderSettings settings;

        public FileProvider(ProviderSettings settings)
        {
            this.settings = settings;
        }

        public ReadFile[] GetFiles()
        {
            var files = new List<ReadFile>();

            var paths = GetPathLists();

            if (!paths.Any()) return files.ToArray();

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                    continue;

                var lines = ReadAllLines(path);


                var fileInfo = new FileInfo(path);

                files.Add(new ReadFile
                {
                    FilePath = path,
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    Lines = lines
                });

                if (settings.ArchiveOnRead)
                {
                    var archivePath = settings.FolderPath + "/" +
                                      (!string.IsNullOrWhiteSpace(settings.ArchiveFolderPath)
                        ? settings.ArchiveFolderPath
                        : "Archived");


                    if (!Directory.Exists(archivePath))
                        Directory.CreateDirectory(archivePath);

                    var archiveLocation = Path.Combine(archivePath, fileInfo.Name);

                   
                }
            }

            return files.ToArray();
        }

        public bool TryDeleteFile(string fileLocation)
        {
            try
            {
                var destfileInfo = new FileInfo(fileLocation);

                if (destfileInfo.Exists)
                    destfileInfo.Delete();

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
        public bool TryCreateFile(string fileLocation, string toWrite)
        {
            try
            {
                using (var fs = File.Create(fileLocation))
                {
                    var data = new UTF8Encoding(true).GetBytes(toWrite);
                    fs.Write(data, 0, data.Length);
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool TryMoveFile(string sourceFile, string destinationFile)
        {
            try
            {
                var fileInfo = new FileInfo(sourceFile);

                fileInfo.MoveTo(destinationFile);
                return true;
            }
            catch (IOException) //file is still locked
            {
                return false;
            }
        }
        public string[] GetPathLists()
        {
            var filePaths = !string.IsNullOrWhiteSpace(settings.FileNameFormat)
                ? Directory.GetFiles(settings.FolderPath, settings.FileNameFormat, SearchOption.TopDirectoryOnly)
                : Directory.GetFiles(settings.FolderPath);

            return filePaths;
        }

        public string[] ReadAllLines(string path)
        {
            var lines = new List<string>();
            using (var sr = new StreamReader(File.Open(path, FileMode.Open)))
            {
                var line = sr.ReadLine();
                if (line != null)
                    lines.Add(line);

                sr.Close();
            }

            return lines.ToArray();
        }
    }
}

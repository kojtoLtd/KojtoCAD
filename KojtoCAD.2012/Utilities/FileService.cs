using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using KojtoCAD.Utilities.ErrorReporting.Exceptions;
using KojtoCAD.Utilities.Interfaces;

namespace KojtoCAD.Utilities
{
    public class FileService : IFileService
    {
        private ILogger _logger = NullLogger.Instance;
        
        public ILogger Logger
        {
            get { return _logger; }     
            set { _logger = value; }
        }

        public string GetFile(string rootFolderToStartSearching, string filename, string fileExtension = null, bool traverseSubdirectories = true)
        {
            return FolderSearch(rootFolderToStartSearching, filename, fileExtension, traverseSubdirectories);
        }

        public string GetUsersTempDir()
        {
            return Path.GetTempPath();
        }

        public void CopyDirectory(string source, string destination)
        {
            var dir = new DirectoryInfo(source);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source);
            }

            Directory.CreateDirectory(destination);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destination, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            var dirs = dir.GetDirectories();
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destination, subdir.Name);
                CopyDirectory(subdir.FullName, temppath);
            }
        }

        public Task CopyDirectoryAsync(string source, string destination)
        {
            return Task.Run(() => CopyDirectory(source, destination));
        }

        public Task CopyDirectoryAsync(string source, string destination, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(source, destination));
                    foreach (var filename in Directory.EnumerateFiles(dirPath))
                    {
                        using (var sourceStream = File.Open(filename, FileMode.Open))
                        {
                            using (var destinationStream = File.Create(filename.Replace(source, destination)))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                sourceStream.CopyTo(destinationStream);
                            }
                        }
                    }
                }

                Directory.CreateDirectory(destination);
                foreach (var filename in Directory.EnumerateFiles(source))
                {
                    using (var sourceStream = File.Open(filename, FileMode.Open))
                    {
                        using (
                            var destinationStream =
                                File.Create(destination + filename.Substring(filename.LastIndexOf('\\'))))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            sourceStream.CopyTo(destinationStream);
                        }
                    }
                }
            }, cancellationToken);
        }

        public Task DeleteDirectoryAsync(string path, bool recursive, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(path))
            {
                return Task.FromResult(true);
            }
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (recursive)
                {
                    foreach (var dirPath in Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Reverse())
                    {
                        foreach (var filename in Directory.EnumerateFiles(dirPath))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            File.Delete(filename);
                        }
                        Directory.Delete(dirPath);
                    }
                }

                foreach (var filename in Directory.EnumerateFiles(path))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    File.Delete(filename);
                }

                cancellationToken.ThrowIfCancellationRequested();
                Directory.Delete(path);
            }, cancellationToken);
        }

        private string FolderSearch(string rootFolderToStartSearching, string fileName, string fileExtension, bool traverseSubdirectories)
        {
            try
            {
                var filesPath = Directory.GetFiles(rootFolderToStartSearching, "*" + fileName + "*" + fileExtension,
                                                   traverseSubdirectories
                                                       ? SearchOption.AllDirectories
                                                       : SearchOption.TopDirectoryOnly);
                var filesPathExact = filesPath.Where(fp =>
                                                     fp.Substring(fp.LastIndexOf("\\", StringComparison.Ordinal) + 1,
                                                                  (fp.LastIndexOf(".", StringComparison.Ordinal)-
                                                                  fp.LastIndexOf("\\", StringComparison.Ordinal))-1).ToUpper() 
                                                                  ==fileName.ToUpper()).ToArray();
                if (filesPathExact.Count() == 1)
                {
                    return filesPathExact.First();
                }

                if (filesPath.Length > 1)
                {
                    var multipleFilesFoundException = new MultipleFilesFoundException(filesPathExact.ToArray().ToString());
                    _logger.Error("FolderSearch results is not single. Probably LINQ problem.", multipleFilesFoundException);
                    throw multipleFilesFoundException;
                }

                return filesPath.Any() ? filesPath[0] : null;
            }
            catch (Exception exception)
            {
                _logger.Error("Folder search error.", exception);
                throw new FileServiceException("Error while searching in folder : ", exception);
            }
        }
    }
}
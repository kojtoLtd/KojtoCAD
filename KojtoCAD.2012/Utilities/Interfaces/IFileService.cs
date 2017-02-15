using System.Threading;
using System.Threading.Tasks;

namespace KojtoCAD.Utilities.Interfaces
{
    /// <summary>
    /// Provides (full file path) + (file name) when given the file name
    /// </summary>
    public interface IFileService
    {
        string GetFile(string rootFolderToStartSearching, string filename, string fileExtension = null,
            bool traverseSubdirectories = true);

        string GetUsersTempDir();

        void CopyDirectory(string source, string destination);
        Task CopyDirectoryAsync(string source, string destination);

        Task CopyDirectoryAsync(string source, string destination, CancellationToken cancellationToken);

        Task DeleteDirectoryAsync(string path, bool recursive, CancellationToken cancellationToken);
    }
}
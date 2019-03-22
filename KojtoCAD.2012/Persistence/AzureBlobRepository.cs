using System;
using System.Linq;
using KojtoCAD.Persistence.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;

namespace KojtoCAD.Persistence
{
    public class AzureBlobRepository : IBlobRepository
    {
        public CloudBlobDirectory[] GetDirectories(Uri container, string path)
        {
            var storageContainer = new CloudBlobContainer(container);
            var directory = storageContainer.GetDirectoryReference(path.Replace("\\", "/"));
            return directory.ListBlobs().Select(x => x as CloudBlobDirectory).Where(x => x != null).ToArray();
        }

        public CloudBlockBlob[] GetBlobsFromDirectory(Uri container, string path)
        {
            var storageContainer = new CloudBlobContainer(container);
            var directory = storageContainer.GetDirectoryReference(path.Replace("\\", "/"));
            return directory.ListBlobs(true).Select(x => x as CloudBlockBlob).Where(x => x != null).ToArray();
        }
    }
}

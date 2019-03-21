using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace KojtoCAD.Persistence.Interfaces
{
    public interface IBlobRepository
    {
        CloudBlobDirectory[] GetDirectories(Uri container, string path);
        CloudBlockBlob[] GetBlobsFromDirectory(Uri container, string path);
    }
}

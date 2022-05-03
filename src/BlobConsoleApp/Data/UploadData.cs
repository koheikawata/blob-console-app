using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BlobConsoleApp.Data;

public abstract class UploadData : IUploadData
{
    public abstract string FolderName { get; }
    public bool IsValidFile { get; set; }
    public string[] FileFullPaths { get; set; }
    public string FileFullPath { get; set; }
    public string FileName { get; set; }
    public BlobClient BlobClient { get; set; }

    private IDictionary<string, string> blobMetadata;

    public UploadData()
    {
        this.blobMetadata = new Dictionary<string, string>();
    }

    public abstract void JudgeValidFile();

    public virtual async Task UploadBlobAsync()
    {
        await this.BlobClient.UploadAsync(this.FileFullPath, true).ConfigureAwait(false);
    }

    public abstract IUploadData GetMetadata();
    public virtual async Task UploadMetadataAsync()
    {
        this.blobMetadata.Clear();
        PropertyInfo[] propertiesIUploadData = typeof(IUploadData).GetProperties();
        IEnumerable<PropertyInfo> propertiesThis = this.GetType().GetRuntimeProperties();

        foreach (PropertyInfo propertyThis in propertiesThis)
        {
            int count = 0;
            foreach (PropertyInfo propertyIUploadData in propertiesIUploadData)
            {
                if (propertyThis.Name == propertyIUploadData.Name) ++count;
            }
            if (count == 0)
            {
                this.blobMetadata[propertyThis.Name.ToString()] = propertyThis.GetValue(this).ToString();
            }
        }
        await this.BlobClient.SetMetadataAsync(this.blobMetadata).ConfigureAwait(false);
    }

    public virtual void DeleteFiles()
    {
        File.Delete(this.FileFullPath);
        File.Delete(this.FileFullPath + ".json");
    }
}

using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace BlobConsoleApp.Data;

public interface IUploadData
{
    public string FolderName { get; }
    public bool IsValidFile { get; set; }
    public string[] FileFullPaths { get; set; }
    public string FileFullPath { get; set; }
    public string FileName { get; set; }
    public BlobClient BlobClient { get; set; }
    public void JudgeValidFile();
    public Task UploadBlobAsync();
    public IUploadData GetMetadata();
    public Task UploadMetadataAsync();
    public void DeleteFiles();
}

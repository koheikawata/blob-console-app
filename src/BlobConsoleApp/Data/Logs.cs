using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Azure.Storage.Blobs;

namespace BlobConsoleApp.Data;

internal class Logs : UploadData
{
    public override string FolderName => "logs";
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public string Location { get; set; }

    public override void JudgeValidFile()
    {
        this.IsValidFile = (Path.GetExtension(this.FileName) == ".csv" && this.FileFullPaths.Contains(this.FileFullPath + ".json"));
    }

    public override IUploadData GetMetadata()
    {
        bool IsValidFile = this.IsValidFile;
        string[] FileFullPaths = this.FileFullPaths;
        string FileFullPath = this.FileFullPath;
        string FileName = this.FileName;
        BlobClient BlobClient = this.BlobClient;

        string jsonString = File.ReadAllText(this.FileFullPath + ".json");
        Logs log = JsonSerializer.Deserialize<Logs>(jsonString)!;

        log.IsValidFile = IsValidFile;
        log.FileFullPaths = FileFullPaths;
        log.FileFullPath = FileFullPath;
        log.FileName = FileName;
        log.BlobClient = BlobClient;

        return log;
    }
}

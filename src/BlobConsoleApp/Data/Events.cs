using System;
using System.IO;

namespace BlobConsoleApp.Data;

public class Events : UploadData
{
    public override string FolderName => "events";
    public DateTime EventTime { get; set; }
    public string DeviceId { get; set; }

    public override void JudgeValidFile()
    {
        this.IsValidFile = (Path.GetExtension(this.FileName) == ".json");
    }

    public override IUploadData GetMetadata()
    {
        this.EventTime = DateTime.UtcNow;
        this.DeviceId = "device-1234";
        return this;
    }

    public override void DeleteFiles()
    {
        File.Delete(this.FileFullPath);
    }
}

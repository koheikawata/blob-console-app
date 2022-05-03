using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BlobConsoleApp.Data;

namespace BlobConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        CancellationTokenSource cts = new();
        AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
        Console.CancelKeyPress += (sender, cpe) => cts.Cancel();

        await UploadFile(configuration);
        await WhenCancelled(cts.Token);
    }

    private static async Task UploadFile(IConfiguration configuration)
    {
        string connectionString = configuration.GetValue<string>("StorageAccount:ConnectionString");
        string containerName = configuration.GetValue<string>("StorageAccount:BlobContainer");
        string localPath = configuration.GetValue<string>("LocalPath");

        BlobContainerClient containerClient = new(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync();

        List<IUploadData> uploadDataList = new()
        {
            new Logs(),
            new Events(),
        };

        while (true)
        {
            for (int i = 0; i < uploadDataList.Count; ++i)
            {
                uploadDataList[i].FileFullPaths = Directory.GetFiles(Path.Combine(localPath, uploadDataList[i].FolderName), "*", SearchOption.AllDirectories);

                foreach (string fileFullPath in uploadDataList[i].FileFullPaths)
                {
                    uploadDataList[i].FileFullPath = fileFullPath;
                    uploadDataList[i].FileName = Path.GetFileName(fileFullPath);
                    uploadDataList[i].BlobClient = containerClient.GetBlobClient($"/{uploadDataList[i].FolderName}/{DateTime.Now.Year}/{DateTime.Now.Month}/{uploadDataList[i].FileName}");

                    uploadDataList[i].JudgeValidFile();
                    if (uploadDataList[i].IsValidFile is false) continue;

                    await uploadDataList[i].UploadBlobAsync().ConfigureAwait(true);

                    uploadDataList[i] = uploadDataList[i].GetMetadata();

                    await uploadDataList[i].UploadMetadataAsync();

                    uploadDataList[i].DeleteFiles();
                }
            }
            await Task.Delay(5000);
        }
    }

    public static Task WhenCancelled(CancellationToken cancellationToken)
    {
        TaskCompletionSource tcs = new();
        cancellationToken.Register(() => tcs.SetResult());
        return tcs.Task;
    }
}

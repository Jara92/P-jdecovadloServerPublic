using Minio;
using Minio.DataModel.Args;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly PujcovadloServerConfiguration _configuration;

    public MinioFileStorage(IMinioClient client, PujcovadloServerConfiguration configuration)
    {
        _minioClient = client;
        _configuration = configuration;
    }

    /// <inheritdoc cref="IFileStorage"/>
    public async Task<string> Save(string filePath, string targetDirectory, string mimeType, string extension)
    {
        // Build a new file name
        var newFileName = Guid.NewGuid() + extension;

        // Check if the bucket exists
        var beArgs = new BucketExistsArgs().WithBucket(targetDirectory);
        bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);

        // If the bucket does not exist, create it.
        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(targetDirectory);
            await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
        }

        // Upload a file to bucket.
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(targetDirectory)
            .WithObject(newFileName)
            .WithFileName(filePath)
            .WithContentType(mimeType);

        var result = await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

        return result.ObjectName;
    }

    /// <inheritdoc cref="IFileStorage"/>
    public async Task Delete(string directory, string filename)
    {
        // Delete the file from the bucket
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(directory)
            .WithObject(filename);

        await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
    }

    /// <inheritdoc cref="IFileStorage"/>
    public Task<string> GetFilePublicUrl(string directory, string fileName)
    {
        var endpoint = _configuration.MinioEndpoint;
        var protocol = _configuration.MinioUseSSL ? "https" : "http";

        return Task.FromResult($"{protocol}://{endpoint}/{directory}/{fileName}");
    }
}
using Minio;
using Minio.DataModel.Args;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Data;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;

    public MinioFileStorage(IMinioClient client)
    {
        _minioClient = client;
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

        await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);

        return newFileName;
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
    public async Task<string> GetFilePath(string directory, string fileName)
    {
        // Get object URL
        PresignedGetObjectArgs args = new PresignedGetObjectArgs()
            .WithBucket(directory)
            .WithObject(fileName)
            .WithExpiry(60 * 60 * 24); // 24 hours todo: make it configurable

        var url = await _minioClient.PresignedGetObjectAsync(args);

        // If the URL is null, throw an exception
        if (url == null) throw new Exception(); // TODO: custom exception

        return url;
    }
}
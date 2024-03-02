using PujcovadloServer.Business.Interfaces;

namespace FunctionalTests.Mocks;

public class MockFileStorage : IFileStorage
{
    private int fileNameId = 0;

    private string _publicUrl = "https://example.com/";

    private IList<string> _savedFiles = new List<string>();

    public Task<string> Save(string filePath, string targetDirectory, string mimeType, string extension)
    {
        string filename = (fileNameId++) + extension;

        _savedFiles.Add(Path.Combine(targetDirectory, filename));

        return Task.FromResult(filename);
    }

    public Task Delete(string directory, string filename)
    {
        _savedFiles.Remove(Path.Combine(directory, filename));

        return Task.CompletedTask;
    }

    public Task<string> GetFilePublicUrl(string directory, string fileName)
    {
        return Task.FromResult(_publicUrl + Path.Combine(directory, fileName));
    }
}
namespace PujcovadloServer.Business.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Saves a new file to the specified directory.
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="targetDirectory">Target directory to save the file.</param>
    /// <param name="mimeType">Mime type of the file.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>New file name</returns>
    public Task<string> Save(string filePath, string targetDirectory, string mimeType, string extension);

    /// <summary>
    /// Deletes the file from the specified directory.
    /// </summary>
    /// <param name="directory">File directory.</param>
    /// <param name="filename">Filename</param>
    /// <returns></returns>
    public Task Delete(string directory, string filename);

    /// <summary>
    /// Returns the path to the file in the specified directory.
    /// </summary>
    /// <param name="directory">File directory</param>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Path to the file.</returns>
    public Task<string> GetFilePublicUrl(string directory, string fileName);
}
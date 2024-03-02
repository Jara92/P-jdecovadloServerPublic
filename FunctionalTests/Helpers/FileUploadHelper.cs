using System.Net.Http.Headers;

namespace FunctionalTests.Helpers;

public class FileUploadHelper
{
    public static MultipartFormDataContent CreateFileUploadForm(string path, string mimeType = "image/jpeg")
    {
        // Get filepath from the test project
        var assemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var rootDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, "../../../../"));
        path = Path.Combine(rootDirectory, path);

        // get file info and open the stream
        var file = new FileInfo(path);
        var stream = file.OpenRead();

        // build form content
        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        content.Add(streamContent, "file", file.Name);

        return content;
    }
}
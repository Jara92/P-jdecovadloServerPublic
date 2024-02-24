namespace PujcovadloServer.Areas.Api.Services;

public class FileUploadService
{
    private readonly PujcovadloServerConfiguration _config;

    /// <summary>
    /// Defined file signatures for each file extension.
    /// Uploaded file signature is checked against these signatures.
    /// Uploaded file signature must match one of these signatures.
    /// </summary>
    private readonly Dictionary<string, List<Byte[]>> _fileSignatures = new()
    {
        {
            ".jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
            }
        },
        {
            ".jpg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                // Add more JPEG signatures as needed
            }
        },
        {
            ".png", new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            }
        },
        {
            ".gif", new List<byte[]>
            {
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 },
            }
        }
    };

    public FileUploadService(PujcovadloServerConfiguration config)
    {
        _config = config;
    }

    public async Task<string> SaveUploadedImage(IFormFile file)
    {
        // Build file path
        string fileExtension = GetFileExtension(file);
        var filePath = BuildFilePath(file.FileName, fileExtension);

        // Check if the file is empty
        CheckFileSize(file);

        // Check file extension
        CheckAllowedExtension(fileExtension);

        // Check mime type
        CheckAllowedMimeType(file.ContentType);

        // Check file signature
        await CheckFileSignature(file);

        // Save the file to the file system
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    public string GetFileExtension(IFormFile file)
    {
        return Path.GetExtension(file.FileName).ToLower();
    }

    public string GetMimeType(IFormFile file)
    {
        return file.ContentType;
    }

    public async void CleanUp(string filePath)
    {
        await Task.Run(() =>
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        });
    }

    protected string BuildFilePath(string fileName, string extension)
    {
        var filePath = Path.GetTempFileName();

        return filePath;
    }

    protected void CheckFileSize(IFormFile file)
    {
        if (file == null || file.Length <= 0) throw new ArgumentException("File is empty.");

        // Check if the file is too large
        if (file.Length > _config.MaxImageSize)
            throw new ArgumentException("File is too large. Max. size is " + _config.MaxImageSize + " bytes.");
    }

    protected void CheckAllowedExtension(string extension)
    {
        if (!_config.AllowedImageExtensions.Contains(extension))
        {
            throw new ArgumentException("File extension is not allowed.");
        }
    }

    protected void CheckAllowedMimeType(string mimeType)
    {
        if (!_config.AllowedImageMimeTypes.Contains(mimeType))
        {
            throw new ArgumentException("File mime type is not allowed.");
        }
    }

    protected async Task CheckFileSignature(IFormFile file)
    {
        // file extension
        var extension = this.GetFileExtension(file);

        // Check if the file signature is allowed
        if (!_fileSignatures.ContainsKey(extension)) throw new ArgumentException("File signature is not allowed.");

        // Read the file signature
        using (var reader = new BinaryReader(file.OpenReadStream()))
        {
            // Load expected signatures for this extension
            var signatures = _fileSignatures[extension];

            // Get firs bytes of the file
            // TODO: make async
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

            // If read bytes does not match any of the signatures throw an exception            
            if (!signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature)))
                throw new ArgumentException("File signature is not allowed.");
        }
    }
}
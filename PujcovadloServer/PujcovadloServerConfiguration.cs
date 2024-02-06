namespace PujcovadloServer;

public class PujcovadloServerConfiguration
{
    private readonly IConfiguration _configuration;
    
    public PujcovadloServerConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    /// <summary>
    /// Maximum image size in bytes which is acceptable.
    /// </summary>
    public int MaxImageSize => _configuration.GetValue<int>("Business:MaxImageSize");
    
    /// <summary>
    /// Maximum images which can be associated with an item.
    /// </summary>
    public int MaxImagesPerItem => _configuration.GetValue<int>("Business:MaxImagesPerItem");
    
    /// <summary>
    /// Allowed file extensions which are accepted when uploading an image.
    /// </summary>
    public string[] AllowedImageExtensions => _configuration.GetSection("Business:AllowedImageExtensions").Get<string[]>() ?? Array.Empty<string>();
    
    /// <summary>
    /// Allowed mime types which are accepted when uploading an image.
    /// </summary>
    public string[] AllowedImageMimeTypes => _configuration.GetSection("Business:AllowedImageMimeTypes").Get<string[]>() ?? Array.Empty<string>();
}
namespace PujcovadloServer;

public class PujcovadloServerConfiguration
{
    private readonly IConfiguration _configuration;

    public PujcovadloServerConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Default directory for images to be stored.
    /// </summary>
    public string ImagesPath => _configuration.GetValue<string>("Business:ImagesPath") ??
                                throw new Exception("ImagesPath is not defined in the configuration.");

    /// <summary>
    /// Maximum image size in bytes which is acceptable.
    /// </summary>
    public int MaxImageSize => _configuration.GetValue<int>("Business:MaxImageSize");

    /// <summary>
    /// Maximum images which can be associated with an item.
    /// </summary>
    public virtual int MaxImagesPerItem => _configuration.GetValue<int>("Business:MaxImagesPerItem");

    /// <summary>
    /// Maximum images which can be associated with a pickup protocol.
    /// </summary>
    public virtual int MaxImagesPerPickupProtocol =>
        _configuration.GetValue<int>("Business:MaxImagesPerPickupProtocol");

    /// <summary>
    /// Maximum images which can be associated with a return protocol.
    /// </summary>
    public virtual int MaxImagesPerReturnProtocol =>
        _configuration.GetValue<int>("Business:MaxImagesPerReturnProtocol");

    /// <summary>
    /// Allowed file extensions which are accepted when uploading an image.
    /// </summary>
    public string[] AllowedImageExtensions =>
        _configuration.GetSection("Business:AllowedImageExtensions").Get<string[]>() ?? Array.Empty<string>();

    /// <summary>
    /// Allowed mime types which are accepted when uploading an image.
    /// </summary>
    public string[] AllowedImageMimeTypes =>
        _configuration.GetSection("Business:AllowedImageMimeTypes").Get<string[]>() ?? Array.Empty<string>();
}
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using NuGet.Packaging;
using PujcovadloServer.Authentication.Exceptions;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Helpers;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class ImageFacade
{
    private readonly ImageService _imageService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;
    private readonly PujcovadloServerConfiguration _configuration;

    public ImageFacade(ImageService imageService, IAuthenticateService authenticateService, IMapper mapper,
        PujcovadloServerConfiguration configuration)
    {
        _imageService = imageService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Image> Create(Image image, string filePath)
    {
        var user = await _authenticateService.GetCurrentUser();

        // Current user is the owner of the image
        image.Owner = user;

        // Move the file to the images directory
        image.Path = await MoveImageFile(image, filePath);

        // save the image
        await _imageService.Create(image);

        return image;
    }

    /// <summary>
    /// Moves temporary image file to the images directory.
    /// </summary>
    /// <param name="image">Image to be saved.</param>
    /// <param name="filePath">Path to the tmp file which contains image data.</param>
    /// <returns>Filename of the new image.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the temporary file was not found.</exception>
    private async Task<string> MoveImageFile(Image image, string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath)) throw new FileNotFoundException();

        // Create directory if it does not exist
        Directory.CreateDirectory(_configuration.ImagesPath);

        // Make sure the directory exists
        await Task.Run(() => Directory.CreateDirectory(_configuration.ImagesPath));

        // Generate new file name
        var newFileName = Guid.NewGuid() + image.Extension;

        // Move the file to the images directory
        var newFilePath = Path.Combine(_configuration.ImagesPath, newFileName);

        // Move the temporary file to the images directory under the new name
        await Task.Run(() => File.Move(filePath, newFilePath));

        return newFileName;
    }

    public async Task<byte[]> GetImageBytes(Image image)
    {
        // Build absolute path to the image
        var filePath = Path.Combine(_configuration.ImagesPath, image.Path);
        
        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Get all bytes of the file and return the file with the specified file contents 
            return await File.ReadAllBytesAsync(filePath);
        }
        
        throw new FileNotFoundException("Image not found.");
    }

    public async Task<Image> GetImage(int itemId, int imageId)
    {
        // Get image and check that it is not null
        var image = await _imageService.Get(imageId);
        if (image == null) throw new EntityNotFoundException();

        // Check that the image is associated with the item
        if (image.Item?.Id != itemId) throw new ArgumentException("Image is not associated with the item.");

        return image;
    }

    public async Task<Image> GetImage(string name)
    {
        var image = await _imageService.GetByPath(name);
        if(image == null) throw new EntityNotFoundException("Image not found.");
        
        return image;
    }

    public async Task DeleteImage(int itemId, int imageId)
    {
        var image = await GetImage(itemId, imageId);

        await DeleteImage(image);
    }

    public async Task DeleteImage(Image image)
    {
        await _imageService.Delete(image);
    }
}
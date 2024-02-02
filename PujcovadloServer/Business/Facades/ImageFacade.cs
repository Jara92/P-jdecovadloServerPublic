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
    private readonly IAuthorizationService _authorizationService;

    public ImageFacade(ImageService imageService, IAuthenticateService authenticateService, IMapper mapper,
        IAuthorizationService authorizationService)
    {
        _imageService = imageService;
        _authenticateService = authenticateService;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Image> Create(Image image)
    {
        var user = await _authenticateService.GetCurrentUser();

        image.Owner = user;

        await _imageService.Create(image);

        return image;
    }

    public async Task<Image> GetImage(Item item, int imageId)
    {
        // Get image and check that it is not null
        var image = await _imageService.Get(imageId);
        if (image == null) throw new EntityNotFoundException();
        
        // Check that the image is associated with the item
        if (image.Item?.Id != item.Id) throw new ArgumentException("Image is not associated with the item.");
        
        return image;
    }

    public async Task DeleteImage(Item item, int imageId)
    {
        var image = await GetImage(item, imageId);

        await _imageService.Delete(image);
    }
}
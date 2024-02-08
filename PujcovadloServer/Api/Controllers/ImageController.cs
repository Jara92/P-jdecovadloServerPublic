using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Api.Filters;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;

namespace PujcovadloServer.Api.Controllers;

[ApiController]
[Route("api/images")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ImageController : ACrudController<Image>
{
    private readonly ImageFacade _imageFacade;

    public ImageController(ImageFacade imageFacade, AuthorizationService authorizationService, LinkGenerator urlHelper)
        : base(authorizationService, urlHelper)
    {
        _imageFacade = imageFacade;
    }

    /// <summary>
    /// Returns image bytes by filename.
    /// </summary>
    /// <param name="filename">Image filname</param>
    /// <returns>Image's binary data.</returns>
    /// <response code="200">Returns image's binary data.</response>
    /// <response code="403">If user is not authorized to view the image.</response>
    /// <response code="404">If image was not found.</response>
    [HttpGet("{filename}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(string filename)
    {
        // Find the image by filename
        var image = await _imageFacade.GetImage(filename);
        
        await _authorizationService.CheckPermissions(image, ImageAuthorizationHandler.Operations.Read);

        // Get the image bytes
        var imageBytes = await _imageFacade.GetImageBytes(image);

        // Return the image data as a file
        return File(imageBytes, "application/octet-stream", image.Name);
    }
}
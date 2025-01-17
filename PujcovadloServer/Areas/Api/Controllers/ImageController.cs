using Microsoft.AspNetCore.Mvc;
using PujcovadloServer.Areas.Api.Filters;
using PujcovadloServer.Areas.Api.Services;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Controllers;

[Area("Api")]
[ApiController]
[Route("api/images")]
[ServiceFilter(typeof(ExceptionFilter))]
public class ImageController : ACrudController<Image>
{
    private readonly ImageFacade _imageFacade;
    private readonly ImageResponseGenerator _responseGenerator;

    public ImageController(ImageFacade imageFacade, ImageResponseGenerator responseGenerator,
        AuthorizationService authorizationService, LinkGenerator urlHelper)
        : base(authorizationService, urlHelper)
    {
        _imageFacade = imageFacade;
        _responseGenerator = responseGenerator;
    }

    /// <summary>
    /// Returns image by given id.
    /// </summary>
    /// <param name="id">Image id.</param>
    /// <returns>Returns image file.</returns>
    /// <response code="200">Returns image file.</response>
    /// <response code="400">If the request is not valid.</response>
    /// <response code="403">If the user is not authorized to read the image.</response>
    /// <response code="404">If the image was not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> GetImage(int id)
    {
        // get the image and return it
        var image = await _imageFacade.GetImage(id);
        await _authorizationService.CheckPermissions(image, ItemOperations.Read);

        // Generate response
        var response = await _responseGenerator.GenerateImageDetailResponse(image);

        return Ok(response);
    }
}
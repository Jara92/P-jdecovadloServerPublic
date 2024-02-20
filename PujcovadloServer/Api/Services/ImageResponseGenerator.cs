using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for Image entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ImageResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public ImageResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates ImageResponse for single image.
    /// </summary>
    /// <param name="image">Image to be converted to a response</param>
    /// <returns>ImageResponse which represents the Image.</returns>
    private async Task<ImageResponse> GenerateSingleImageResponse(Image image)
    {
        var response = _mapper.Map<ImageResponse>(image);

        // Add link to the image using ItemImageController
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ImageController.GetImage), "Image",
                values: new { id = image.Id }), "SELF", "GET"));

        AddCommonLinks(response, image);

        return response;
    }

    /// <summary>
    /// generates links which are common for list and detail responses.
    /// </summary>
    /// <param name="response">Response to be advanced.</param>
    /// <param name="image">Image which is the response about.</param>
    private void AddCommonLinks(ImageResponse response, Image image)
    {
        // Add link to the image data
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ImageDataController.GetImage), "ImageData",
                values: new { filename = image.Path }), "DATA", "GET"));

        // Image item
        if (image.Item != null)
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Get), "Item",
                    values: new { image.Item.Id }), "ITEM", "GET"));

            // Add delete link
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.DeleteImage), "Item",
                    values: new { itemId = image.Item.Id, imageId = image.Id }), "DELETE", "DELETE"));
        }

        // Pickup protocol
        if (image.PickupProtocol != null)
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(PickupProtocolController.GetProtocol), "PickupProtocol",
                    values: new { loanId = image.PickupProtocol.Loan.Id }), "PICKUP_PROTOCOL", "GET"));

            // Add delete link
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(PickupProtocolController.DeleteImage), "PickupProtocol",
                    values: new { loanId = image.PickupProtocol.Loan.Id, imageId = image.Id }), "DELETE", "DELETE"));
        }

        // Return protocol
        if (image.ReturnProtocol != null)
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ReturnProtocolController.GetProtocol), "ReturnProtocol",
                    values: new { loanId = image.ReturnProtocol.Loan.Id }), "RETURN_PROTOCOL", "GET"));

            // Add delete link
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ReturnProtocolController.DeleteImage), "ReturnProtocol",
                    values: new { loanId = image.ReturnProtocol.Loan.Id, imageId = image.Id }), "DELETE", "DELETE"));
        }
    }

    /// <summary>
    /// Returns ResponseList with all images as responses.
    /// </summary>
    /// <param name="images">Categories to be converted to a response.</param>
    /// <returns>ImageResponse</returns>
    public async Task<ResponseList<ImageResponse>> GenerateResponseList(IList<Image> images)
    {
        // Create empty list
        var responseImages = new List<ImageResponse>();

        foreach (var image in images)
        {
            responseImages.Add(await GenerateSingleImageResponse(image));
        }

        // More links here..

        // return response list
        return new ResponseList<ImageResponse>
        {
            Data = responseImages,
            Links = new List<LinkResponse>()
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the image.
    /// </summary>
    /// <param name="loan">Image to be converted to a response.</param>
    /// <returns>ImageResponse which represents the image</returns>
    public async Task<ImageResponse> GenerateImageDetailResponse(Image loan)
    {
        var response = _mapper.Map<ImageResponse>(loan);

        AddCommonLinks(response, loan);

        return response;
    }

    public string? GetLink(Image image)
    {
        return _urlHelper.GetUriByAction(_httpContext, nameof(ImageController.GetImage), "Image",
            values: new { id = image.Id });
    }
}
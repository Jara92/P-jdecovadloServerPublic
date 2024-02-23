using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.Item;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for Item entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ItemResponseGenerator : ABaseResponseGenerator
{
    private readonly ItemFacade _itemFacade;
    private readonly ImageFacade _imageFacade;
    private readonly IMapper _mapper;

    public ItemResponseGenerator(ItemFacade itemFacade, ImageFacade imageFacade, IMapper mapper,
        LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _itemFacade = itemFacade;
        _imageFacade = imageFacade;
        _mapper = mapper;
    }

    /// <summary>
    /// generates ItemResponse for single item.
    /// </summary>
    /// <param name="item">Item to be converted to a response</param>
    /// <param name="owner">Is the response meant for the owner? </param>
    /// <returns>ItemResponse which represents the Item.</returns>
    private async Task<ItemResponse> GenerateSingleItemResponse(Item item)
    {
        var response = _mapper.Map<ItemResponse>(item);

        // Link to item detail
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Get), "Item", values: new { item.Id }),
            "SELF", "GET"));

        // item delete action if has permission
        if (await _authorizationService.CanPerformOperation(item, ItemOperations.Delete))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Delete), "Item",
                    values: new { item.Id }), "DELETE", "DELETE"));
        }

        // Link to item owner
        if (item.Owner.Profile != null)
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ProfileController.GetProfile), "Profile",
                    values: new { item.Owner.Profile.Id }), "OWNER", "GET"));
        }

        // Main image link
        if (item.MainImage != null && response.MainImage != null)
        {
            await AddImageLinks(item, item.MainImage, response.MainImage);
        }

        // Image links
        for (var i = 0; i < item.Images.Count; i++)
        {
            await AddImageLinks(item, item.Images[i], response.Images[i]);
        }

        return response;
    }

    private async Task AddImageLinks(Item item, Image image, ImageResponse imageResponse)
    {
        // LInk to image detail
        imageResponse.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ImageController.GetImage), "Image",
                values: new { id = imageResponse.Id }), "SELF", "GET"));

        var imageUrl = await _imageFacade.GetImagePath(image);

        // Link to image data
        imageResponse.Links.Add(new LinkResponse(imageUrl, "DATA", "GET"));
    }

    /// <summary>
    /// Adds Item links which are common for all detailed responses of the item.
    /// </summary>
    /// <param name="item">Item which is the response about.</param>
    /// <param name="response">Response for the links to be added.</param>
    private async Task AddItemDetailLinks(Item item, ItemDetailResponse response)
    {
        // Main image link
        if (item.MainImage != null && response.MainImage != null)
        {
            await AddImageLinks(item, item.MainImage, response.MainImage);
        }

        // Image links
        for (var i = 0; i < item.Images.Count; i++)
        {
            await AddImageLinks(item, item.Images[i], response.Images[i]);
        }
    }

    /// <summary>
    /// Returns ResponseList with all items as responses.
    /// </summary>
    /// <param name="items">Items to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the items.</param>
    /// <param name="action">Action to used for retrieving the items.</param>
    /// <param name="controller">Controller used for retrieving the items.</param>
    /// <param name="owner">Is the response meant for the owner? </param>
    /// <returns></returns>
    public async Task<ResponseList<ItemResponse>> GenerateResponseList(PaginatedList<Item> items, ItemFilter filter,
        string action, string controller)
    {
        // Create empty list
        var responseItems = new List<ItemResponse>();

        foreach (var item in items)
        {
            responseItems.Add(await GenerateSingleItemResponse(item));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(items, filter, action, controller);

        // Todo: links

        // return response list
        return new ResponseList<ItemResponse>()
        {
            Data = responseItems,
            Links = links
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the item.
    /// </summary>
    /// <param name="item">Item to be converted to a response.</param>
    /// <returns>ItemDetailResponse which represents the item</returns>
    public async Task<ItemDetailResponse> GenerateItemDetailResponse(Item item)
    {
        var response = _mapper.Map<ItemDetailResponse>(item);

        // Add link for detailed response
        await AddItemDetailLinks(item, response);

        // Add links for detailed reponse
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item"), "LIST", "GET"));

        return response;
    }

    /// <summary>
    /// generates ItemOwnerResponse with all information about the item.
    /// </summary>
    /// <param name="item">Item to be converted to a response</param>
    /// <returns>ItemOwnerResponse</returns>
    public async Task<ItemOwnerResponse> GenerateItemOwnerResponse(Item item)
    {
        var response = _mapper.Map<ItemOwnerResponse>(item);

        // Add links for detailed response
        await AddItemDetailLinks(item, response);

        // Add links for owner
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item"), "LIST", "GET"));

        // item update action if has permission
        if (await _authorizationService.CanPerformOperation(item, ItemOperations.Update))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Update), "Item",
                    values: new { item.Id }), "UPDATE", "PUT"));
        }

        // item delete action if has permission
        if (await _authorizationService.CanPerformOperation(item, ItemOperations.Delete) &&
            await _itemFacade.CanDelete(item))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Delete), "Item",
                    values: new { item.Id }), "DELETE", "DELETE"));
        }

        return response;
    }
}
using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.Business.Entities;
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
    private readonly IMapper _mapper;

    public ItemResponseGenerator(IMapper mapper, LinkGenerator urlHelper, IHttpContextAccessor httpContextAccessor) :
        base(httpContextAccessor, urlHelper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates ItemResponse for single item.
    /// </summary>
    /// <param name="item">Item to be converted to a response</param>
    /// <returns>ItemResponse which represents the Item.</returns>
    private ItemResponse GenerateSingleItemResponse(Item item)
    {
        var response = _mapper.Map<ItemResponse>(item);

        // Link to item detail
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Get), "Item", values: new { item.Id }),
            "SELF", "GET"));

        // TODO: add link to owner

        foreach (var image in response.Images)
        {
            // LInk to image detail
            image.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ItemImageController.GetImage), "ItemImage",
                    values: new { id = item.Id, imageId = image.Id }), "SELF", "GET"));

            // Link to image data
            image.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ImageController.GetImage), "Image",
                    values: new { fileName = image.Path }), "DATA", "GET"));
        }

        return response;
    }

    /// <summary>
    /// Adds Item links which are common for all detailed responses of the item.
    /// </summary>
    /// <param name="item">Item which is the response about.</param>
    /// <param name="response">Response for the links to be added.</param>
    private void AddItemDetailLinks(Item item, ItemDetailResponse response)
    {
        // todo: Add common links
    }

    /// <summary>
    /// Returns ResponseList with all items as responses.
    /// </summary>
    /// <param name="items">Items to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the items.</param>
    /// <param name="action">Action to used for retrieving the items.</param>
    /// <param name="controller">Controller used for retrieving the items.</param>
    /// <returns></returns>
    public ResponseList<ItemResponse> GenerateResponseList(PaginatedList<Item> items, ItemFilter filter, string action,
        string controller)
    {
        // Create empty list
        var responseItems = new List<ItemResponse>();

        foreach (var item in items)
        {
            responseItems.Add(GenerateSingleItemResponse(item));
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
    public ItemDetailResponse GenerateItemDetailResponse(Item item)
    {
        var response = _mapper.Map<ItemDetailResponse>(item);

        // Add link for detailed response
        AddItemDetailLinks(item, response);

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
    public ItemOwnerResponse GenerateItemOwnerResponse(Item item)
    {
        var response = _mapper.Map<ItemOwnerResponse>(item);

        // Add links for detailed response
        AddItemDetailLinks(item, response);

        // Add links for owner
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(MyItemController.Index), "MyItem"), "LIST", "GET"));

        return response;
    }
}
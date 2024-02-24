using AutoMapper;
using PujcovadloServer.Areas.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Services;

/// <summary>
/// This class generates responses for ItemCategory entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ItemCategoryResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public ItemCategoryResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates ItemCategoryResponse for single category.
    /// </summary>
    /// <param name="category">Category to be converted to a response</param>
    /// <returns>ItemCategoryResponse which represents the ItemCategory.</returns>
    private async Task<ItemCategoryResponse> GenerateSingleItemCategoryResponse(ItemCategory category)
    {
        var response = _mapper.Map<ItemCategoryResponse>(category);

        // Link to category detail
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemCategoryController.Get), "ItemCategory",
                values: new { category.Id }),
            "SELF", "GET"));

        // Link to category categories
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item",
                values: new { categoryId = category.Id }), "ITEMS", "GET"));

        return response;
    }

    /// <summary>
    /// Returns ResponseList with all categories as responses.
    /// </summary>
    /// <param name="categories">Categories to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the categories.</param>
    /// <param name="action">Action to used for retrieving the categories.</param>
    /// <param name="controller">Controller used for retrieving the categories.</param>
    /// <returns>ItemCategoryResponse</returns>
    public async Task<ResponseList<ItemCategoryResponse>> GenerateResponseList(PaginatedList<ItemCategory> categories,
        ItemCategoryFilter filter,
        string action, string controller)
    {
        // Create empty list
        var responseItems = new List<ItemCategoryResponse>();

        foreach (var category in categories)
        {
            responseItems.Add(await GenerateSingleItemCategoryResponse(category));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(categories, filter, action, controller);

        // Todo: links

        // return response list
        return new ResponseList<ItemCategoryResponse>
        {
            _data = responseItems,
            _links = links
        };
    }

    /// <summary>
    /// generates itemDetailResponse with detailed information about the category.
    /// </summary>
    /// <param name="category">Category to be converted to a response.</param>
    /// <returns>ItemDetailResponse which represents the category</returns>
    public async Task<ItemCategoryResponse> GenerateItemCategoryDetailResponse(ItemCategory category)
    {
        var response = _mapper.Map<ItemCategoryResponse>(category);

        // Add links for detailed reponse
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item"), "LIST", "GET"));

        // Link to category categories
        response._links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(ItemController.Index), "Item",
                values: new { categoryId = category.Id }), "ITEMS", "GET"));

        return response;
    }
}
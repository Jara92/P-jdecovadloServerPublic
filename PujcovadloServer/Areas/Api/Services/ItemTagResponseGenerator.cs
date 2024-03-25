using AutoMapper;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Areas.Api.Services;

/// <summary>
/// This class generates responses for ItemTag entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ItemTagResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public ItemTagResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates ItemTagResponse for single tag.
    /// </summary>
    /// <param name="tag">Tag to be converted to a response</param>
    /// <returns>ItemTagResponse which represents the ItemTag.</returns>
    private async Task<ItemTagResponse> GenerateSingleItemTagResponse(ItemTag tag)
    {
        return _mapper.Map<ItemTagResponse>(tag);
    }

    /// <summary>
    /// Returns ResponseList with all categories as responses.
    /// </summary>
    /// <param name="categories">Categories to be converted to a response.</param>
    /// <param name="filter">Filter used for retrieving the categories.</param>
    /// <param name="action">Action to used for retrieving the categories.</param>
    /// <param name="controller">Controller used for retrieving the categories.</param>
    /// <returns>ItemTagResponse</returns>
    public async Task<ResponseList<ItemTagResponse>> GenerateResponseList(PaginatedList<ItemTag> categories,
        ItemTagFilter filter,
        string action, string controller)
    {
        // Create empty list
        var responseItems = new List<ItemTagResponse>();

        foreach (var tag in categories)
        {
            responseItems.Add(await GenerateSingleItemTagResponse(tag));
        }

        // Init links with pagination links
        var links = GeneratePaginationLinks(categories, filter, action, controller);

        // return response list
        return new ResponseList<ItemTagResponse>
        {
            _data = responseItems,
            _links = links
        };
    }
}
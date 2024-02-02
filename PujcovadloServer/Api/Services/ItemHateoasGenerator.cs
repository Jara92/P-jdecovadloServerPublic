using PujcovadloServer.Api.Controllers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

public class ItemHateoasGenerator : AHateoasGenerator<Item, ItemResponse>
{
    public ItemHateoasGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator urlHelper) : base(httpContextAccessor, urlHelper)
    {
    }

    public override void AddLinks(ItemResponse response, Item entity)
    {
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, nameof(ItemController.Get), 
                "Item", values: new { id = response.Id }), "SELF", "GET"));
        
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, nameof(ItemController.Index), 
                "Item"), "LIST", "GET"));
        
        foreach(var image in response.Images)
        {
            image.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, nameof(ItemImageController.GetImage), "ItemImage",
                    values: new { id = response.Id, imageId = image.Id }), "SELF", "GET"));
        }
    }
    
    public override ResponseList<ItemResponse> GetWithLinks(IList<ItemResponse> responses, PaginatedList<Item> entities)
    {
       foreach(var response in responses)
       {
           AddLinks(response, entities.First(x => x.Id == response.Id));
       }
       
       return new ResponseList<ItemResponse>
       {
           Data = responses
       };
    }
}
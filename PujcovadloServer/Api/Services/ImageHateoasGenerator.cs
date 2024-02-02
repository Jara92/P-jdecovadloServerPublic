using PujcovadloServer.Api.Controllers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Lib;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

public class ImageHateoasGenerator : AHateoasGenerator<Image, ImageResponse>
{
    public ImageHateoasGenerator(IHttpContextAccessor httpContextAccessor, LinkGenerator urlHelper) : base(httpContextAccessor, urlHelper)
    {
    }

    public override void AddLinks(ImageResponse response, Image entity)
    {
        if(entity.Item != null)
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, nameof(ItemImageController.GetImage), "ItemImage",
                    values: new { id = entity.Item.Id, imageId = response.Id }), "SELF", "GET"));
        }
    }
    
    public override ResponseList<ImageResponse> GetWithLinks(IList<ImageResponse> responses, PaginatedList<Image> entities)
    {
       foreach(var response in responses)
       {
           AddLinks(response, entities.First(x => x.Id == response.Id));
       }

       var links = new List<LinkResponse>();
       
       // Add item link
       var first = entities.FirstOrDefault();
       if(first != null && first.Item != null)
       {
           links.Add(new LinkResponse(_urlHelper.GetUriByAction(_httpContextAccessor.HttpContext, 
               nameof(ItemController.Get), "Item", values: first.Item.Id), "ITEM", "GET"));
       }
       
       return new ResponseList<ImageResponse>
       {
           Data = responses,
           Links = links
       };
    }
}
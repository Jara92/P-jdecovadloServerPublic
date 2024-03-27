using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.EntityAggregations;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;
using Profile = AutoMapper.Profile;

namespace PujcovadloServer.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemResponse>();
        CreateMap<Item, ItemDetailResponse>();
        CreateMap<Item, ItemOwnerResponse>();
        CreateMap<ItemRequest, Item>()
            // Ignore categories and tags because they are handled separately
            .ForMember(r => r.Categories, opt => opt.Ignore())
            .ForMember(r => r.Tags, opt => opt.Ignore());

        CreateMap<ItemCategory, ItemCategoryResponse>();
        CreateMap<ItemCategoryRequest, ItemCategory>();

        CreateMap<ItemTag, ItemTagResponse>();
        CreateMap<ItemTagRequest, ItemTag>();

        CreateMap<Image, ImageResponse>();

        CreateMap<ApplicationUser, UserResponse>();

        CreateMap<Loan, LoanResponse>()
            .ForMember(r => r.Owner, opt => opt.MapFrom(l => l.Item.Owner))
            .ForMember(r => r.ItemName, opt => opt.MapFrom(l => l.Item.Name))
            .ForMember(r => r.ItemImage, opt => opt.MapFrom(l => l.Item.MainImage))
            ;
        CreateMap<LoanRequest, Loan>()
            // Ignore item because it need to be handled separately
            .ForMember(r => r.Item, opt => opt.Ignore());

        CreateMap<PickupProtocol, PickupProtocolResponse>();
        CreateMap<PickupProtocolRequest, PickupProtocol>();

        CreateMap<ReturnProtocol, ReturnProtocolResponse>();
        CreateMap<ReturnProtocolRequest, ReturnProtocol>();

        CreateMap<Review, ReviewResponse>();
        CreateMap<ReviewRequest, Review>();

        CreateMap<PujcovadloServer.Business.Entities.Profile, ProfileResponse>()
            .ForMember(p => p._aggregations, opt => opt.MapFrom(p => p.Aggregations))
            ;
        CreateMap<ProfileUpdateRequest, PujcovadloServer.Business.Entities.Profile>();

        CreateMap<ProfileAggregations, ProfileAggregationsResponse>();

        CreateMap<ApplicationUser, UserResponse>();

        CreateMap<Item, Areas.Admin.Requests.ItemRequest>()
            .ForMember(r => r.Categories, opt => opt.MapFrom(i => i.Categories.Select(c => c.Id)))
            .ForMember(r => r.Tags, opt => opt.MapFrom(i => i.Tags.Select(t => t.Id)));
        CreateMap<Areas.Admin.Requests.ItemRequest, Item>()
            .ForMember(r => r.Categories, opt => opt.Ignore())
            .ForMember(r => r.Tags, opt => opt.Ignore());

        // CreateMap<ItemCategory, ItemCategoryDto>();
        //CreateMap<ItemCategoryDto, ItemCategory>();

        // Admin area
        CreateMap<Item, Areas.Admin.Responses.ItemResponse>();

        CreateMap<ItemCategory, Areas.Admin.Responses.ItemCategoryResponse>()
            .ForMember(r => r.ParentName, opt => opt.MapFrom(c => (c.Parent == null) ? null : c.Parent.Name));
        CreateMap<Areas.Admin.Requests.ItemCategoryRequest, ItemCategory>();
        CreateMap<ItemCategory, Areas.Admin.Requests.ItemCategoryRequest>();

        CreateMap<ItemTag, Areas.Admin.Responses.ItemTagResponse>();
        CreateMap<Areas.Admin.Requests.ItemTagRequest, ItemTag>();
        CreateMap<ItemTag, Areas.Admin.Requests.ItemTagRequest>();

        CreateMap<Areas.Admin.Requests.LoanRequest, Loan>();
        CreateMap<Loan, Areas.Admin.Requests.LoanRequest>();
        CreateMap<Loan, Areas.Admin.Responses.LoanResponse>();

        CreateMap<Areas.Admin.Requests.PickupProtocolRequest, PickupProtocol>();
        CreateMap<PickupProtocol, Areas.Admin.Requests.PickupProtocolRequest>();
        CreateMap<PickupProtocol, Areas.Admin.Responses.PickupProtocolResponse>();

        CreateMap<Areas.Admin.Requests.ReturnProtocolRequest, ReturnProtocol>();
        CreateMap<ReturnProtocol, Areas.Admin.Requests.ReturnProtocolRequest>();
        CreateMap<ReturnProtocol, Areas.Admin.Responses.ReturnProtocolResponse>();

        CreateMap<ApplicationUser, Areas.Admin.Responses.UserResponse>();
        CreateMap<Areas.Admin.Requests.UserRequest, ApplicationUser>();
        CreateMap<ApplicationUser, Areas.Admin.Requests.UserRequest>();

        CreateMap<Review, Areas.Admin.Responses.ReviewResponse>();
        CreateMap<Areas.Admin.Requests.ReviewRequest, Review>();
        CreateMap<Review, Areas.Admin.Requests.ReviewRequest>();
    }
}
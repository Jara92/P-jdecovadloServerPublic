using AutoMapper;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemResponse>();
        CreateMap<Item, ItemDetailResponse>();
        CreateMap<Item, ItemOwnerResponse>();
        CreateMap<ItemRequest, Item>();

        CreateMap<ItemCategory, ItemCategoryResponse>();
        CreateMap<ItemCategoryRequest, ItemCategory>();

        CreateMap<ItemTag, ItemTagResponse>();
        CreateMap<ItemTagRequest, ItemTag>();

        CreateMap<Image, ImageResponse>();

        CreateMap<ApplicationUser, UserResponse>();

        CreateMap<Loan, LoanResponse>();
        CreateMap<TenantLoanRequest, Loan>();

        CreateMap<PickupProtocol, PickupProtocolResponse>();
        CreateMap<PickupProtocolRequest, PickupProtocol>();

        CreateMap<ReturnProtocol, ReturnProtocolResponse>();
        CreateMap<ReturnProtocolRequest, ReturnProtocol>();

        // CreateMap<ItemCategory, ItemCategoryDto>();
        //CreateMap<ItemCategoryDto, ItemCategory>();
    }
}
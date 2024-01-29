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

        CreateMap<ApplicationUser, UserResponse>();
        
        CreateMap<Loan, LoanResponse>();
        CreateMap<TenantLoanRequest, Loan>();

        // CreateMap<ItemCategory, ItemCategoryDto>();
        //CreateMap<ItemCategoryDto, ItemCategory>();
    }
}
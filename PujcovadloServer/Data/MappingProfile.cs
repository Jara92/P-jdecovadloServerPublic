using AutoMapper;
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
        CreateMap<ItemRequest, Item>();
        
        CreateMap<ItemCategory, ItemCategoryResponse>();
        CreateMap<ItemCategoryRequest, ItemCategory>();
        
        // CreateMap<ItemCategory, ItemCategoryDto>();
        //CreateMap<ItemCategoryDto, ItemCategory>();
    }
}
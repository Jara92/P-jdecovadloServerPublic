using AutoMapper;
using PujcovadloServer.Models;
using PujcovadloServer.Requests;
using PujcovadloServer.Responses;

namespace PujcovadloServer.data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemResponse>();
        CreateMap<ItemRequest, Item>();
        
        CreateMap<ItemCategory, ItemCategoryResponse>();
       // CreateMap<ItemCategoryRequest, ItemCategory>();
        
        // CreateMap<ItemCategory, ItemCategoryDto>();
        //CreateMap<ItemCategoryDto, ItemCategory>();
    }
}
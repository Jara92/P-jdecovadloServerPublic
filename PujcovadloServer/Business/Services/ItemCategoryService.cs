using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ItemCategoryService(IItemCategoryRepository repository) : ACrudService<ItemCategory, ItemCategoryFilter>(repository);
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;

namespace PujcovadloServer.Services;

public class ItemCategoryService(IItemCategoryRepository repository) : ACrudService<ItemCategory>(repository);
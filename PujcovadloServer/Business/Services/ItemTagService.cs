using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;

namespace PujcovadloServer.Business.Services;

public class ItemTagService(IItemTagRepository repository)
    : ACrudService<ItemTag, IItemTagRepository, ItemTagFilter>(repository)
{
    public async Task<ItemTag?> GetByName(string name)
    {
        return await repository.GetByName(name);
    }

    public virtual async Task<ItemTag> GetOrCreate(string name)
    {
        // get tag by name
        var tag = await GetByName(name);

        // Create a new tag if it does not exist
        if (tag == null)
        {
            tag = new ItemTag { Name = name };
            await Create(tag);
        }

        return tag;
    }

    public virtual async Task<List<ItemTag>> GetOrCreate(List<string> names)
    {
        //var tags = await Task.WhenAll(names.Select(async tagName => await GetOrCreate(tagName)));

        var tags = new List<ItemTag>();
        foreach (var name in names)
        {
            tags.Add(await GetOrCreate(name));
        }

        return tags;
    }
}
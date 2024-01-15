using System.Text.RegularExpressions;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories;

namespace PujcovadloServer.Facades;

public class ItemsFacade
{
    private readonly ItemsRepository _itemsRepository;
    
    public ItemsFacade(ItemsRepository _itemsRepository)
    {
        this._itemsRepository = _itemsRepository;
    }
    
    public void CreateItem(Item item)
    {
        item.Alias = CreateUrlStub(item.Name);
        
        _itemsRepository.Create(item);
    }
    
    public void UpdateItem(Item item)
    {
        item.Alias = CreateUrlStub(item.Name);
        
        _itemsRepository.Update(item);
    }
    
    private  string CreateUrlStub(string input)
    {
        // Replace spaces with dashes, remove special characters, and convert to lowercase
        string urlStub = Regex.Replace(input, @"[^a-zA-Z0-9]", "-").ToLower();
        
        // Remove consecutive dashes
        urlStub = Regex.Replace(urlStub, @"-+", "-");
        
        // Remove leading and trailing dashes
        urlStub = urlStub.Trim('-');
        
        return urlStub;
    }
}
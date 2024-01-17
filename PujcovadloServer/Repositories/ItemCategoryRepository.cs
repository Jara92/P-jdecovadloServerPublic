using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PujcovadloServer.data;
using PujcovadloServer.Exceptions;
using PujcovadloServer.Models;
using PujcovadloServer.Repositories.Interfaces;

namespace PujcovadloServer.Repositories;

public class ItemCategoryRepository : ACrudRepository<ItemCategory>, IItemCategoryRepository
{
    public ItemCategoryRepository(PujcovadloServerContext context) : base(context)
    {
    }
}
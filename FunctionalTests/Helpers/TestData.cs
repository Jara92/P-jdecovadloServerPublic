using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace FunctionalTests.Helpers;

public class TestData
{
    public static ItemCategory ItemCategoryVrtacky =
        new() { Id = 1, Name = "Vrtacky", Description = "Vrtacky description" };

    public static ItemCategory ItemCategoryPneumatickeVrtacky = new()
        { Id = 2, Name = "Vrtacky", Parent = ItemCategoryVrtacky, Description = "Pneumaticke vrtacky description" };

    public static ItemCategory ItemCategoryKladiva =
        new() { Id = 3, Name = "Kladiva", Description = "Kladiva description" };

    public static ItemTag ItemTagVrtackaNarex = new() { Id = 1, Name = "Vrtačka Narex" };
    public static ItemTag ItemTagVrtackaBosch = new() { Id = 2, Name = "Vrtačka Bosh" };

    public static Item Item1 = new()
    {
        Id = 1, Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
        Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100
    };

    public static Item Item2 = new()
    {
        Id = 2, Name = "Item2", Description = "Description1", Status = ItemStatus.Approving,
        Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100
    };
}
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace FunctionalTests.Helpers;

public class TestData
{
    public ItemCategory ItemCategoryVrtacky;
    public ItemCategory ItemCategoryPneumatickeVrtacky;
    public ItemCategory ItemCategoryKladiva;

    public ItemTag ItemTagVrtackaNarex;
    public ItemTag ItemTagVrtackaBosch;

    public Item Item1;

    public Item Item2;

    public Item ItemDeleted;

    public Item ItemWithRunningLoans;

    public Image Item1Image1;

    public Image Item1Image2;

    public Image Item2Image1;

    public Image Item2Image2;

    public Loan Loan1;

    public TestData()
    {
        ItemCategoryVrtacky =
            new() { Id = 1, Name = "Vrtacky", Description = "Vrtacky description" };

        ItemCategoryPneumatickeVrtacky = new()
            { Id = 2, Name = "Vrtacky", Parent = ItemCategoryVrtacky, Description = "Pneumaticke vrtacky description" };

        ItemCategoryKladiva =
            new() { Id = 3, Name = "Kladiva", Description = "Kladiva description" };

        ItemTagVrtackaNarex = new() { Id = 1, Name = "Vrtačka Narex" };
        ItemTagVrtackaBosch = new() { Id = 2, Name = "Vrtačka Bosh" };

        Item1 = new()
        {
            Id = 1, Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null
        };

        Item2 = new()
        {
            Id = 2, Name = "Item2", Description = "Description1", Status = ItemStatus.Approving,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null
        };

        ItemDeleted = new()
        {
            Id = 3, Name = "Item deleted", Description = "Description1", Status = ItemStatus.Deleted,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null, DeletedAt = DateTime.Today.AddDays(-2)
        };

        ItemWithRunningLoans = new()
        {
            Id = 4, Name = "Item with running loans", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null
        };

        Item1Image1 = new()
        {
            Id = 10, Item = Item1, Owner = Item1.Owner, Name = "Nazev image 1", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Item1Image2 = new()
        {
            Id = 2, Item = Item1, Owner = Item1.Owner, Name = "Nazev image 2", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Item2Image1 = new()
        {
            Id = 3, Item = Item2, Owner = Item2.Owner, Name = "Nazev image 1", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Item2Image2 = new()
        {
            Id = 4, Item = Item2, Owner = Item2.Owner, Name = "Nazev image 2", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Loan1 = new()
        {
            Id = 1, Item = ItemWithRunningLoans, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(1), Status = LoanStatus.Active
        };
    }
}
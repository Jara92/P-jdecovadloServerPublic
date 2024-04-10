using NetTopologySuite.Geometries;
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

    public Item ItemApproving;

    public Item ItemDenied;

    public Item ItemWithRunningLoans;

    public Item ItemWithoutRunningLoans;

    public Image Item1Image1;

    public Image Item1Image2;

    public Image Item2Image1;

    public Image Item2Image2;

    public Loan Loan1;


    public TestData()
    {
        ItemCategoryVrtacky =
            new() { Name = "Vrtacky", Description = "Vrtacky description" };

        ItemCategoryPneumatickeVrtacky = new()
            { Name = "Vrtacky", Parent = ItemCategoryVrtacky, Description = "Pneumaticke vrtacky description" };

        ItemCategoryKladiva =
            new() { Name = "Kladiva", Description = "Kladiva description" };

        ItemTagVrtackaNarex = new() { Name = "Vrtačka Narex" };
        ItemTagVrtackaBosch = new() { Name = "Vrtačka Bosh" };

        Item1 = new()
        {
            Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        Item2 = new()
        {
            Name = "Item2", Description = "Description1", Status = ItemStatus.Approving,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        ItemDeleted = new()
        {
            Name = "Item deleted", Description = "Description1", Status = ItemStatus.Deleted,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null, DeletedAt = DateTime.Today.AddDays(-2),
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        ItemWithRunningLoans = new()
        {
            Name = "Item with running loans", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        ItemApproving = new()
        {
            Name = "Item approving", Description = "Description1", Status = ItemStatus.Approving,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        ItemDenied = new()
        {
            Name = "Item denied", Description = "Description1", Status = ItemStatus.Denied,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        ItemWithoutRunningLoans = new()
        {
            Name = "Item without running loans", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null,
            Location = new Point(50.0, 14.0) { SRID = 4326 }
        };

        Item1Image1 = new()
        {
            Item = Item1, Owner = Item1.Owner, Name = "Nazev image 1", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId,
        };

        Item1Image2 = new()
        {
            Item = Item1, Owner = Item1.Owner, Name = "Nazev image 2", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Item2Image1 = new()
        {
            Item = Item2, Owner = Item2.Owner, Name = "Nazev image 1", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Item2Image2 = new()
        {
            Item = Item2, Owner = Item2.Owner, Name = "Nazev image 2", Extension = ".png",
            MimeType = "image/png", Path = "random.png", OwnerId = Item1.OwnerId
        };

        Loan1 = new()
        {
            Item = ItemWithRunningLoans, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(-1), To = DateTime.Today.AddDays(1), Status = LoanStatus.Active
        };
    }
}
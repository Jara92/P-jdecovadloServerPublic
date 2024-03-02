using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace IntegrationTests.Helpers;

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

    public Loan LoanInquired;

    public Loan LoanCancelled;

    public Loan LoanAccepted;

    public Loan LoanAcceptedWithPickupProtocol;

    public Loan LoanDenied;

    public Loan LoanPreparedForPickup;

    public Loan LoanPickupDenied;

    public Loan LoanActive;

    public Loan LoanActiveHasBothProtocols;

    public Loan LoanPreparedForReturn;

    public Loan LoanPreparedForReturnHasBothProtocols;

    public Loan LoanReturnDenied;

    public Loan LoanReturned;

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

        ItemApproving = new()
        {
            Id = 5, Name = "Item approving", Description = "Description1", Status = ItemStatus.Approving,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null
        };

        ItemDenied = new()
        {
            Id = 6, Name = "Item denied", Description = "Description1", Status = ItemStatus.Denied,
            Parameters = "Parameters2", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100,
            MainImage = null, MainImageId = null
        };

        ItemWithoutRunningLoans = new()
        {
            Id = 7, Name = "Item without running loans", Description = "Description1", Status = ItemStatus.Public,
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

        LoanInquired = new()
        {
            Id = 2, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Inquired
        };

        LoanCancelled = new()
        {
            Id = 3, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Cancelled
        };

        LoanAccepted = new()
        {
            Id = 4, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Accepted
        };

        LoanDenied = new()
        {
            Id = 5, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Denied
        };

        LoanPreparedForPickup = new()
        {
            Id = 6, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.PreparedForPickup,
            PickupProtocol = new PickupProtocol { Id = 2, Description = "All ok" }
        };

        LoanPickupDenied = new()
        {
            Id = 7, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.PickupDenied,
            PickupProtocol = new PickupProtocol { Id = 6, Description = "All ok" }
        };

        LoanActive = new()
        {
            Id = 8, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Active
        };

        LoanPreparedForReturn = new()
        {
            Id = 9, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.PreparedForReturn
        };

        LoanReturnDenied = new()
        {
            Id = 10, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.ReturnDenied,
            PickupProtocol = new PickupProtocol() { Id = 5, Description = "All ok" },
            ReturnProtocol = new ReturnProtocol { Id = 5, Description = "All ok" }
        };

        LoanReturned = new()
        {
            Id = 11, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Returned
        };

        LoanAcceptedWithPickupProtocol = new()
        {
            Id = 12, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Accepted,
            PickupProtocol = new PickupProtocol
            {
                Id = 1, Description = "All ok",
                Images = new List<Image>()
                {
                    new()
                    {
                        Name = "test1", Extension = ".jpg", Path = "test1.jpg", OwnerId = UserHelper.OwnerId,
                        Owner = UserHelper.Owner, MimeType = "image/jpeg",
                    }
                }
            }
        };

        LoanActiveHasBothProtocols = new()
        {
            Id = 13, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.Active,
            PickupProtocol = new PickupProtocol { Id = 3, Description = "All ok" },
            ReturnProtocol = new ReturnProtocol { Id = 3, Description = "All ok" }
        };

        LoanPreparedForReturnHasBothProtocols = new()
        {
            Id = 14, Item = Item1, Tenant = UserHelper.Tenant, TenantId = UserHelper.TenantId,
            From = DateTime.Today.AddDays(1), To = DateTime.Today.AddDays(3), Status = LoanStatus.PreparedForReturn,
            PickupProtocol = new PickupProtocol { Id = 4, Description = "All ok" },
            ReturnProtocol = new ReturnProtocol { Id = 4, Description = "All ok" }
        };
    }
}
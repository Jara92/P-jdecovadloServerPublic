using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;

namespace IntegrationTests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(PujcovadloServerContext db)
    {
        db.Database.EnsureCreated();

        // Add users
        db.Users.Add(UserHelper.User);
        db.Users.Add(UserHelper.Owner);
        db.Users.Add(UserHelper.Tenant);

        // Add data
        db.Item.Add(new PujcovadloServer.Business.Entities.Item
        {
            Id = 1, Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100
        });

        db.Item.Add(new PujcovadloServer.Business.Entities.Item
        {
            Id = 2, Name = "Item1", Description = "Description1", Status = ItemStatus.Approving,
            Parameters = "Parameters1", Owner = UserHelper.Owner, OwnerId = UserHelper.OwnerId, PricePerDay = 100
        });

        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(PujcovadloServerContext db)
    {
        // Remove all data from the database
        db.Database.EnsureDeleted();

        InitializeDbForTests(db);
    }

    public static void DeleteDbForTests(PujcovadloServerContext db)
    {
        db.Database.EnsureDeleted();
    }
}
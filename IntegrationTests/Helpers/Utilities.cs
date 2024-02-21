using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Data;

namespace IntegrationTests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(PujcovadloServerContext db)
    {
        db.Database.EnsureCreated();

        var user1 = new ApplicationUser()
        {
            Id = "1", UserName = "User1", Email = "tester@exmaple.com", EmailConfirmed = true, FirstName = "Tester",
            LastName = "Testovac"
        };

        db.Users.Add(user1);

        // Add data
        db.Item.Add(new PujcovadloServer.Business.Entities.Item
        {
            Id = 1, Name = "Item1", Description = "Description1", Status = ItemStatus.Public,
            Parameters = "Parameters1", Owner = user1, PricePerDay = 100
        });

        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(PujcovadloServerContext db)
    {
        // Remove all data from the database
        db.Database.EnsureDeleted();

        InitializeDbForTests(db);
    }
}
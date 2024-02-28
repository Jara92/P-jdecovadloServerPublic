using PujcovadloServer.Data;

namespace FunctionalTests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(PujcovadloServerContext db)
    {
        db.Database.EnsureCreated();

        // Add users
        db.Users.Add(UserHelper.User);
        db.Users.Add(UserHelper.Owner);
        db.Users.Add(UserHelper.Tenant);

        // Add item categories
        db.ItemCategory.Add(TestData.ItemCategoryVrtacky);
        db.ItemCategory.Add(TestData.ItemCategoryPneumatickeVrtacky);
        db.ItemCategory.Add(TestData.ItemCategoryKladiva);

        // Add item tags
        db.ItemTag.Add(TestData.ItemTagVrtackaNarex);
        db.ItemTag.Add(TestData.ItemTagVrtackaBosch);

        // Add data
        db.Item.Add(TestData.Item1);
        db.Item.Add(TestData.Item2);

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
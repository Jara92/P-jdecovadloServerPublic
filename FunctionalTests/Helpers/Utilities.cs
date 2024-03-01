using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Data;

namespace FunctionalTests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(PujcovadloServerContext db, TestData data)
    {
        db.Database.Migrate();
        db.Database.EnsureCreated();

        // Add users
        db.Users.Add(UserHelper.User);
        db.Users.Add(UserHelper.Owner);
        db.Users.Add(UserHelper.Tenant);

        // Add item categories
        db.ItemCategory.Add(data.ItemCategoryVrtacky);
        db.ItemCategory.Add(data.ItemCategoryPneumatickeVrtacky);
        db.ItemCategory.Add(data.ItemCategoryKladiva);

        // Add item tags
        db.ItemTag.Add(data.ItemTagVrtackaNarex);
        db.ItemTag.Add(data.ItemTagVrtackaBosch);

        // Add data
        db.Item.Add(TestData.Item1);
        db.Item.Add(TestData.Item2);

        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(PujcovadloServerContext db, TestData data)
    {
        // Remove all data from the database
        db.Database.EnsureDeleted();


        InitializeDbForTests(db, data);
    }

    public static void DeleteDbForTests(PujcovadloServerContext db)
    {
        db.Database.EnsureDeleted();
    }
}
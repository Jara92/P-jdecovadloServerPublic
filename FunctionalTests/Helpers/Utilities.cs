using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Data;

namespace FunctionalTests.Helpers;

public static class Utilities
{
    public static void InitializeDbForTests(PujcovadloServerContext db, TestData data)
    {
        //bool created = db.Database.EnsureCreated();
        db.Database.Migrate();

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
        db.Item.Add(data.Item1);
        db.Item.Add(data.Item2);
        db.Item.Add(data.ItemDeleted);
        db.Item.Add(data.ItemApproving);
        db.Item.Add(data.ItemDenied);
        db.Item.Add(data.ItemWithRunningLoans);
        db.Item.Add(data.ItemWithoutRunningLoans);

        // Add images
        db.Image.Add(data.Item1Image1);
        db.Image.Add(data.Item1Image2);
        db.Image.Add(data.Item2Image1);
        db.Image.Add(data.Item2Image2);
        db.SaveChanges();

        // Set main images now to avoid circular references exception
        data.Item2.MainImage = data.Item2Image1;

        db.Update(data.Item2);
        db.SaveChanges();

        // Add loans
        db.Loan.Add(data.Loan1);

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
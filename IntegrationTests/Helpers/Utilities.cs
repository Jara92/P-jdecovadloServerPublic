using Microsoft.EntityFrameworkCore;
using PujcovadloServer.Data;

namespace IntegrationTests.Helpers;

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
        db.Loan.Add(data.LoanInquired);
        db.Loan.Add(data.LoanCancelled);
        db.Loan.Add(data.LoanAccepted);
        db.Loan.Add(data.LoanAcceptedWithPickupProtocol);
        db.Loan.Add(data.LoanDenied);
        db.Loan.Add(data.LoanPreparedForPickup);
        db.Loan.Add(data.LoanPickupDenied);
        db.Loan.Add(data.LoanActive);
        db.Loan.Add(data.LoanActiveHasBothProtocols);
        db.Loan.Add(data.LoanPreparedForReturn);
        db.Loan.Add(data.LoanPreparedForReturnHasBothProtocols);
        db.Loan.Add(data.LoanReturnDenied);
        db.Loan.Add(data.LoanReturned);

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
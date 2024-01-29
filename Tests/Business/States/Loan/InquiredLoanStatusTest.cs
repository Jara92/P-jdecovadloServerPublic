using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace Tests.Business.States.Loan;

public class InquiredLoanStatusTest : ALoanStatusTest
{
    [SetUp]
    public void Setup()
    {
        Setup(LoanStatus.Inquired);
    }

    #region Tenanttests

    [Test]
    public void HandleTenant_ChangesStatusToCancelled()
    {
        // Act
        _state.HandleTenant(_loan, LoanStatus.Cancelled);

        // Should be able to cancel the loan
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Cancelled));
    }

    [Test]
    public void HandleTenant_ChangesStatusToInquired()
    {
        // Act 
        _state.HandleTenant(_loan, LoanStatus.Inquired);

        // Should be the same and not throw an exception
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Inquired));
    }

    [Test]
    public void HandleTenant_ThrowsException_WhenDisallowedStatus()
    {
        var disallowed = new List<LoanStatus>
        {
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.Active,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned,
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<ActionNotAllowedException>(() => _state.HandleTenant(_loan, status));
        }
    }

    #endregion

    #region OwnerTests

    [Test]
    public void HandleOwner_ChangesStatusToAllowed()
    {
        // Define allowed statuses
        var allowed = new List<LoanStatus>
        {
            LoanStatus.Denied,
            LoanStatus.Accepted,
        };

        // Check all allowed statuses
        foreach (var status in allowed)
        {
            _loan.Status = LoanStatus.Inquired;

            // Act
            _state.HandleOwner(_loan, status);

            // Should be able to change the status
            Assert.That(_loan.Status, Is.EqualTo(status));
        }
    }

    [Test]
    public void HandleOwner_ChangesStatusToInquired()
    {
        // Act 
        _state.HandleOwner(_loan, LoanStatus.Inquired);

        // Should be the same and not throw an exception
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Inquired));
    }

    [Test]
    public void HandleOwner_ThrowsException_WhenDisallowedStatus()
    {
        // Disallowed statuses
        var disallowed = new List<LoanStatus>
        {
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.Active,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned,
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<ActionNotAllowedException>(() => _state.HandleOwner(_loan, status));
        }
    }

    #endregion
}
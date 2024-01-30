using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace Tests.Business.States.Loan;

public class PickupDeniedLoanStateTest : ALoanStateTest
{
    [SetUp]
    public void Setup()
    {
        Setup(LoanStatus.PickupDenied);
    }

    #region Tenanttests

    [Test]
    public void HandleTenant_ChangesStatusToAllowed()
    {
        var allowed = new List<LoanStatus>
        {
            _status,
            LoanStatus.Cancelled,
        };
        
        // Check all allowed statuses
        foreach (var status in allowed)
        {
            _loan.Status = _status;

            // Act
            _state.HandleTenant(_loan, status);

            // Should be able to change the status
            Assert.That(_loan.Status, Is.EqualTo(status));
        }
    }

    [Test]
    public void HandleTenant_ThrowsException_WhenDisallowedStatus()
    {
        var disallowed = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Accepted,
            LoanStatus.Denied,
            LoanStatus.PreparedForPickup,
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
            _status,
            // TODO: allow active?
            LoanStatus.PreparedForPickup,
            LoanStatus.Cancelled
        };

        // Check all allowed statuses
        foreach (var status in allowed)
        {
            _loan.Status = _status;

            // Act
            _state.HandleOwner(_loan, status);

            // Should be able to change the status
            Assert.That(_loan.Status, Is.EqualTo(status));
        }
    }

    [Test]
    public void HandleOwner_ThrowsException_WhenDisallowedStatus()
    {
        // Disallowed statuses
        var disallowed = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Denied,
            LoanStatus.Accepted,
            LoanStatus.Active,
            LoanStatus.ReturnDenied,
            LoanStatus.PreparedForReturn,
            LoanStatus.Returned
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<ActionNotAllowedException>(() => _state.HandleOwner(_loan, status));
        }
    }

    #endregion
}
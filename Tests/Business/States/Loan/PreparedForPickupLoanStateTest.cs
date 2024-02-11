using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace Tests.Business.States.Loan;

public class PrepareForPickupLoanStateTest : ALoanStateTest
{
    [SetUp]
    public void Setup()
    {
        Setup(LoanStatus.PreparedForPickup);
    }

    #region Tenanttests

    [Test]
    public void HandleTenant_ChangesStatusToAllowed()
    {
        var allowed = new List<LoanStatus>
        {
            _status,
            LoanStatus.Cancelled,
            LoanStatus.PickupDenied
        };

        // Check all allowed statuses
        foreach (var status in allowed)
        {
            _loan.Status = _status;
            _loan.PickupProtocol = new PickupProtocol { Id = 1 };

            // Act
            _state.HandleTenant(_loan, status);

            // Should be able to change the status
            Assert.That(_loan.Status, Is.EqualTo(status));
        }
    }

    [Test]
    public void HandleTenant_TransitionToActiveButPickupProtocolIsNotSet_Fails()
    {
        // Arrange
        _loan.PickupProtocol = null;

        // Act - OperationNotAllowedException is thrown
        Assert.Throws<ActionNotAllowedException>(() => _state.HandleTenant(_loan, LoanStatus.Active));
        // Make sure the status is not changed
        Assert.That(_loan.Status, Is.EqualTo(_status));
        // Make sure the protocol is not set
        Assert.That(_loan.PickupProtocol, Is.Null);
    }

    [Test]
    public void HandleTenant_TransitionToActivePickupProtocolSet_StatusUpdates()
    {
        _loan.PickupProtocol = new PickupProtocol { Id = 1 };

        // act
        _state.HandleTenant(_loan, LoanStatus.Active);

        // Status is updated to active
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Active));
        // Protocol is confirmed
        Assert.That(_loan.PickupProtocol.ConfirmedAt, Is.Not.Null);
    }

    [Test]
    public void HandleTenant_ThrowsException_WhenDisallowedStatus()
    {
        var disallowed = new List<LoanStatus>
        {
            LoanStatus.Inquired,
            LoanStatus.Accepted,
            LoanStatus.Denied,
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
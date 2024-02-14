using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace Tests.Business.States.Loan;

public class ActiveLoanStateTest : ALoanStateTest
{
    [SetUp]
    public void Setup()
    {
        Setup(LoanStatus.Active);
    }

    #region Tenanttests

    [Test]
    public void HandleTenant_ChangesStatusToAllowed()
    {
        var allowed = new List<LoanStatus>
        {
            _status
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
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.PreparedForReturn,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned,
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<OperationNotAllowedException>(() => _state.HandleTenant(_loan, status));
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
            LoanStatus.Returned
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
    public void HandleOwner_ChangeStatusToPreparedForReturnButNoReturnProtocolSet_ThrowsException()
    {
        // Arrange
        _loan.Status = _status;
        _loan.ReturnProtocol = null;

        // Act
        Assert.Throws<OperationNotAllowedException>(() => _state.HandleOwner(_loan, LoanStatus.PreparedForReturn));

        // Check that the status was not changed
        Assert.That(_loan.Status, Is.EqualTo(_status));

        // Check that Return protocol is still null
        Assert.That(_loan.ReturnProtocol, Is.Null);
    }

    [Test]
    public void HandleOwner_ChangeStatusToPreparedForReturnWhenReturnProtocolIsSet_Succeeds()
    {
        // Arrange
        _loan.Status = _status;
        var returnProtocol = new ReturnProtocol { Id = 1, Description = "descipriotn" };
        _loan.ReturnProtocol = returnProtocol;

        // Act
        _state.HandleOwner(_loan, LoanStatus.PreparedForReturn);

        // Check that the status was changed
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PreparedForReturn));

        // Check that Return protocol is still set
        Assert.That(_loan.ReturnProtocol, Is.EqualTo(returnProtocol));

        // Check that Return protocol is not signed
        Assert.That(_loan.ReturnProtocol.ConfirmedAt, Is.Null);
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
            LoanStatus.Cancelled,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.ReturnDenied,
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<OperationNotAllowedException>(() => _state.HandleOwner(_loan, status));
        }
    }

    #endregion
}
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace Tests.Business.States.Loan;

public class PrepareForReturnLoanStateTest : ALoanStateTest
{
    [SetUp]
    public void Setup()
    {
        Setup(LoanStatus.PreparedForReturn);
    }

    #region Tenanttests

    [Test]
    public void HandleTenant_ChangesStatusToAllowed()
    {
        var allowed = new List<LoanStatus>
        {
            _status,
            LoanStatus.ReturnDenied,
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
    public void HandleTenant_ChangeStatusToReturnedWhenReturnProtocolDoesNotExist_ThrowsException()
    {
        // Arrange
        _loan.Status = _status;
        _loan.ReturnProtocol = null;

        // Act
        Assert.Throws<OperationNotAllowedException>(() => _state.HandleTenant(_loan, LoanStatus.Returned));

        // Assert that the status has not changed
        Assert.That(_loan.Status, Is.EqualTo(_status));

        // Assert that return protocol is still null
        Assert.That(_loan.ReturnProtocol, Is.Null);
    }

    [Test]
    public void HandleTenant_ChangeStatusToReturnedWhenReturnProtocolExists_Succeeds()
    {
        // Arrange
        _loan.Status = _status;
        var returnProtocol = new ReturnProtocol { Id = 1, Description = "Description" };
        _loan.ReturnProtocol = returnProtocol;

        // Act
        _state.HandleTenant(_loan, LoanStatus.Returned);

        // Assert that the status has changed
        Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Returned));

        // Assert that return protocol is still null
        Assert.That(_loan.ReturnProtocol, Is.EqualTo(returnProtocol));

        // Check that the return protocol has been signed
        Assert.That(returnProtocol.ConfirmedAt, Is.Not.Null);
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
            LoanStatus.Active,
            LoanStatus.PickupDenied,
            LoanStatus.PreparedForPickup,
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
            _status
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
            LoanStatus.Cancelled,
            LoanStatus.Active,
            LoanStatus.PreparedForPickup,
            LoanStatus.PickupDenied,
            LoanStatus.ReturnDenied,
            LoanStatus.Returned
        };

        foreach (var status in disallowed)
        {
            // Act & Assert
            Assert.Throws<OperationNotAllowedException>(() => _state.HandleOwner(_loan, status));
        }
    }

    #endregion
}
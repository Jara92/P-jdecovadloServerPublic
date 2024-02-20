using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class PickupDeniedLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Tenant can cancel the loan
            case LoanStatus.Cancelled:
                loan.Status = newStatus;
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Owner can canel or again prepare the loan for pickup
            case LoanStatus.Cancelled:
            case LoanStatus.PreparedForPickup:
                loan.Status = newStatus;
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    public override bool CanUpdatePickupProtocol(Entities.Loan loan) => true;
}
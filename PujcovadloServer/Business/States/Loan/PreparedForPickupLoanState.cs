using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class PreparedForPickupLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Tenant can cancel, deny or pickup the loan (active)
            case LoanStatus.Cancelled:
            case LoanStatus.PickupDenied:
            case LoanStatus.Active:
                loan.Status = newStatus;
                break;
            default:
                throw new ActionNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Owner can cancel the loan (only when there is some problem)
            case LoanStatus.Cancelled:
                loan.Status = newStatus;
                break;
            default:
                throw new ActionNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }
}
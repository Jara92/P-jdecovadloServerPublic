using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class PreparedForReturnLoanState : ILoanState
{
    /// <inheritdoc cref="ILoanState"/>
    public void HandleTenant(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Tenant can return or deny
            case LoanStatus.ReturnDenied:
            case LoanStatus.Returned:
                loan.Status = newStatus;
                break;
            default:
                throw new ActionNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    public void HandleOwner(Entities.Loan loan, LoanStatus newStatus)
    {
        // owner can do nothing now
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class InquiredLoanState : ILoanState
{
    /// <inheritdoc cref="ILoanState"/>
    public void HandleTenant(Entities.Loan loan, LoanStatus newStatus)
    {
        if (loan.Status == newStatus) return;
        
        switch (newStatus)
        {
            // Tenant can cancel the loan
            case LoanStatus.Cancelled:
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
        // TODO: FIND A BETTER WAY
        if (loan.Status == newStatus) return;
        
        switch (newStatus)
        {
            // Owner can accept or deny the loan
            case LoanStatus.Accepted:
            case LoanStatus.Denied:
                loan.Status = newStatus;
                break;
            default:
                throw new ActionNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }
}
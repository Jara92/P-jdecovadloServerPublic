using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class CanceledLoanState : ILoanState
{
    /// <inheritdoc cref="ILoanState"/>
    public void HandleTenant(Entities.Loan loan, LoanStatus newStatus)
    {
        // Tenant can do nothing now
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
    }

    /// <inheritdoc cref="ILoanState"/>
    public void HandleOwner(Entities.Loan loan, LoanStatus newStatus)
    {
        // Owner can do nothing now
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class ReturnedLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // tenant can do nothing now
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // Owner can do nothing now
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }
}
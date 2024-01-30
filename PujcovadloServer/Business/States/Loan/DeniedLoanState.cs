using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class DeniedLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        throw new ActionNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        throw new ActionNotAllowedException(
                            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class CanceledLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // Tenant can do nothing now
        throw new OperationNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // Owner can do nothing now
        throw new OperationNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }
}
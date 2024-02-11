using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.States.Loan;

public abstract class ALoanState : ILoanState
{
    /// <inheritdoc cref="ILoanState"/>
    public virtual void HandleTenant(Entities.Loan loan, LoanStatus newStatus)
    {
        // Nothing to be done here
        if (loan.Status == newStatus) return;

        HandleTenantImplementation(loan, newStatus);
    }

    /// <inheritdoc cref="ILoanState"/>
    public virtual void HandleOwner(Entities.Loan loan, LoanStatus newStatus)
    {
        // Nothing to be done here
        if (loan.Status == newStatus) return;
        HandleOwnerImplementation(loan, newStatus);
    }

    protected abstract void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus);

    protected abstract void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus);
}
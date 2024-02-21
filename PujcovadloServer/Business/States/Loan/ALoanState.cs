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

    /// <inheritdoc cref="ILoanState"/>
    public virtual bool CanUpdatePickupProtocol(Entities.Loan loan) => false;

    /// <inheritdoc cref="ILoanState"/>
    public virtual bool CanCreatePickupProtocol(Entities.Loan loan) => false;

    /// <inheritdoc cref="ILoanState"/>
    public virtual bool CanUpdateReturnProtocol(Entities.Loan loan) => false;

    /// <inheritdoc cref="ILoanState"/>
    public virtual bool CanCreateReturnProtocol(Entities.Loan loan) => false;

    /// <inheritdoc cref="ILoanState"/>
    public virtual bool CanCreateReview(Entities.Loan loan) => false;

    protected abstract void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus);

    protected abstract void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus);
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class ActiveLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // tenant can do nothing now
        throw new OperationNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Owner can prepare the loan for return or return the loan
            case LoanStatus.PreparedForReturn:
                PrepareForReturn(loan);
                break;
            case LoanStatus.Returned:
                loan.Status = newStatus;
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }

    private void PrepareForReturn(Entities.Loan loan)
    {
        // Check if the loan has a return protocol
        if (loan.ReturnProtocol == null)
            throw new OperationNotAllowedException("Return protocol does not exist.");

        // Change the loan status
        loan.Status = LoanStatus.PreparedForReturn;
    }

    /// <inheritdoc cref="ILoanState"/>
    public override bool CanUpdateReturnProtocol(Entities.Loan loan) => true;

    /// <inheritdoc cref="ILoanState"/>
    public override bool CanCreateReturnProtocol(Entities.Loan loan) => true;
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class PreparedForReturnLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Tenant can return or deny
            case LoanStatus.ReturnDenied:
                loan.Status = newStatus;
                break;
            case LoanStatus.Returned:
                ReturnedLoanState(loan);
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        // owner can do nothing now
        throw new OperationNotAllowedException(
            $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
    }

    private void ReturnedLoanState(Entities.Loan loan)
    {
        // Check if the loan has a return protocol
        if (loan.ReturnProtocol == null)
            throw new OperationNotAllowedException("Return protocol does not exist.");

        // Sign the return protocol
        loan.ReturnProtocol.ConfirmedAt = DateTime.Now;

        // Change the loan status
        loan.Status = LoanStatus.Returned;
    }
}
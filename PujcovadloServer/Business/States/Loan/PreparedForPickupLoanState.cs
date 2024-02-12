using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class PreparedForPickupLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Tenant can cancel, deny or pickup the loan (active)
            case LoanStatus.Cancelled:
            case LoanStatus.PickupDenied:
                loan.Status = newStatus;
                break;
            case LoanStatus.Active:
                MakeLoanActive(loan);
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as a tenant.");
        }
    }

    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Owner can cancel the loan (only when there is some problem)
            case LoanStatus.Cancelled:
                loan.Status = newStatus;
                break;
            default:
                throw new OperationNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }

    /// <summary>
    /// Realizes transition from PreparedForPickup status to Active status.
    /// </summary>
    /// <param name="loan">Loan to be updated.</param>
    /// <exception cref="OperationNotAllowedException">Thrown when there is no pickup protocol.</exception>
    private void MakeLoanActive(Entities.Loan loan)
    {
        // Check if pickup protocol exists (it should exist)
        if (loan.PickupProtocol == null)
            throw new OperationNotAllowedException("Cannot change loan status to active without pickup protocol.");

        // Update the status of the loan
        loan.Status = LoanStatus.Active;

        // Confirm pickup protocol
        loan.PickupProtocol.ConfirmedAt = DateTime.Now;
    }
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;

namespace PujcovadloServer.Business.States.Loan;

public class AcceptedLoanState : ALoanState
{
    /// <inheritdoc cref="ILoanState"/>
    protected override void HandleTenantImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
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
    protected override void HandleOwnerImplementation(Entities.Loan loan, LoanStatus newStatus)
    {
        switch (newStatus)
        {
            // Can cancel, activate or prepare for pickup
            case LoanStatus.Cancelled:
            case LoanStatus.Active:
                // Update the status
                loan.Status = newStatus;
                break;
            case LoanStatus.PreparedForPickup:
                // Pickup protocol is required for changing the status to PreparedForPickup
                if (loan.PickupProtocol == null)
                    throw new ActionNotAllowedException(
                        $"Cannot change loan status from {loan.Status} to {newStatus} as an owner without a pickup protocol.");

                // Update the status
                loan.Status = newStatus;
                break;
            default:
                throw new ActionNotAllowedException(
                    $"Cannot change loan status from {loan.Status} to {newStatus} as an owner.");
        }
    }
}
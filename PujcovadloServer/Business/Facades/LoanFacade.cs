using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class LoanFacade
{
    private readonly LoanService _loanService;
    private readonly PickupProtocolService _pickupProtocolService;

    public LoanFacade(LoanService loanService, PickupProtocolService pickupProtocolService)
    {
        _loanService = loanService;
        _pickupProtocolService = pickupProtocolService;
    }

    public async Task<Loan> GetLoan(int loanId)
    {
        var loan = await _loanService.Get(loanId);
        if (loan == null) throw new EntityNotFoundException("Loan not found");

        return loan;
    }

    public PickupProtocol GetPickupProtocol(Loan loan)
    {
        var protocol = loan.PickupProtocol;
        if (protocol == null) throw new EntityNotFoundException("Protocol not found");

        return protocol;
    }
    
    public async Task UpdatePickupProtocol(PickupProtocol protocol, PickupProtocolRequest request)
    {
        protocol.Description = request.Description;
        protocol.AcceptedRefundableDeposit = request.AcceptedRefundableDeposit;
        
        await _pickupProtocolService.Update(protocol);
    }
}
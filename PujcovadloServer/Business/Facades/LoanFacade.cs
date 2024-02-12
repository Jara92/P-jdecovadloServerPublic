using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Services;

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
}
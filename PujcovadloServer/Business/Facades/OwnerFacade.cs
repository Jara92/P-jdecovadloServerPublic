using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class OwnerFacade
{
    private readonly LoanService _loanService;
    private readonly IAuthenticateService _authenticateService;
    private readonly PujcovadloServerConfiguration _configuration;

    public OwnerFacade(LoanService loanService, IAuthenticateService authenticateService,
        PujcovadloServerConfiguration configuration)
    {
        _loanService = loanService;
        _authenticateService = authenticateService;
        _configuration = configuration;
    }

    /// <summary>
    /// Retruns paginated list of loans where the current user is the owner.
    /// </summary>
    /// <param name="filter">Filter to be used.</param>
    /// <returns></returns>
    public virtual async Task<PaginatedList<Loan>> GetMyLoans(LoanFilter filter)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // Get loans where the user is the tenant
        var loans = await _loanService.GetLoansByOwner(user, filter);

        return loans;
    }

    public virtual async Task UpdateMyLoan(Loan loan, LoanUpdateRequest request)
    {
        // Check if the status has been changed
        if (request.Status != null)
        {
            // get current state
            var state = _loanService.GetState(loan);

            // handle the request
            state.HandleOwner(loan, request.Status.Value);
        }

        /*
        TODO: Change loan parameters
        In the future it would be great to have the option to change date, note etc.
        But these changes are possible to be made only if the loan is in some specific statuses.
        */

        await _loanService.Update(loan);
    }
}
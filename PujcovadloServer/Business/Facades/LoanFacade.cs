using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class LoanFacade
{
    private readonly LoanService _loanService;
    private readonly OwnerFacade _ownerFacade;
    private readonly TenantFacade _tenantFacade;
    private readonly IAuthenticateService _authenticateService;

    public LoanFacade(LoanService loanService, OwnerFacade ownerFacade, TenantFacade tenantFacade,
        IAuthenticateService authenticateService)
    {
        _loanService = loanService;
        _ownerFacade = ownerFacade;
        _tenantFacade = tenantFacade;
        _authenticateService = authenticateService;
    }

    /// <summary>
    /// Returns paginated list of loans by given filter.
    /// </summary>
    /// <param name="filter">Filter to be used.</param>
    /// <returns>Paginated list of loans.</returns>
    public Task<PaginatedList<Loan>> GetLoans(LoanFilter filter)
    {
        var userId = _authenticateService.GetCurrentUserId();

        // If OwnerId is the same as userId, then the return all loans of current user where he is the owner
        if (filter.OwnerId == userId)
        {
            return _ownerFacade.GetMyLoans(filter);
        }

        // If TenantId is the same as userId, then the return all loans of current user where he is the tenant
        if (filter.TenantId == userId)
        {
            return _tenantFacade.GetMyLoans(filter);
        }

        // Otherwise return all loans of current user where he is either owner or tenant
        return _loanService.GetLoansByUserId(userId, filter);
    }

    /// <summary>
    /// Returns loan by given id.
    /// </summary>
    /// <param name="loanId">Id of the loan</param>
    /// <returns>Loan</returns>
    /// <exception cref="EntityNotFoundException">thrown when loan with given id does not exist.</exception>
    public async Task<Loan> GetLoan(int loanId)
    {
        var loan = await _loanService.Get(loanId);
        if (loan == null) throw new EntityNotFoundException("Loan not found");

        return loan;
    }

    /// <summary>
    /// Updates loan by given request.
    /// </summary>
    /// <param name="loan">Loan to be updated.</param>
    /// <param name="request">Request to update the loan.</param>
    /// <returns></returns>
    public Task UpdateLoan(Loan loan, LoanUpdateRequest request)
    {
        var userId = _authenticateService.GetCurrentUserId();

        // User is the owner
        if (loan.Item.Owner.Id == userId)
        {
            return _ownerFacade.UpdateMyLoan(loan, request);
        }

        // User is the tenant
        if (loan.Tenant.Id == userId)
        {
            return _tenantFacade.UpdateMyLoan(loan, request);
        }

        // User has no right to update the loan - this should not happen.
        return Task.CompletedTask;
    }
}
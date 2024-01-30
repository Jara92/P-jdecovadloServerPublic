using System.Security.Authentication;
using AutoMapper;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Services;
using PujcovadloServer.Business.Services.Interfaces;
using PujcovadloServer.Lib;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Business.Facades;

public class OwnerFacade
{
    private readonly LoanService _loanService;
    private readonly ItemService _itemService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;

    public OwnerFacade(LoanService loanService, ItemService itemService, IAuthenticateService authenticateService,
        IMapper mapper)
    {
        _loanService = loanService;
        _itemService = itemService;
        _authenticateService = authenticateService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<Loan>> GetMyLoans(LoanFilter filter)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new AuthenticationException();

        // Get loans where the user is the tenant
        var loans = await _loanService.GetLoansByOwner(user, filter);

        return loans;
    }

    public async Task<Loan> GetMyLoan(int id)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new AuthenticationException();

        // Get the loan
        var loan = await _loanService.Get(id);

        // Check that the loan exists
        if (loan == null) throw new EntityNotFoundException();

        // Check that the user is the tenant
        if (loan.Item.Owner.Id != user.Id) throw new UnauthorizedAccessException("You are not the owner of this loan.");

        return loan;
    }

    public async Task UpdateMyLoan(TenantLoanRequest request)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new AuthenticationException();
        
        // Get the loan
        var oldLoan = await GetMyLoan(request.Id);

        // Check if the status has been changed
        if (request.Status != null && request.Status != oldLoan.Status)
        {
            // get current state
            var state = _loanService.GetState(oldLoan);
            
            // handle the request
            state.HandleOwner(oldLoan, request.Status.Value);
        }

        /*
        TODO: Change loan parameters
        In the future it would be great to have the option to change date, note etc.
        But these changes are possible to be made only if the loan is in some specific statuses.
        */

        await _loanService.Update(oldLoan);
    }
}
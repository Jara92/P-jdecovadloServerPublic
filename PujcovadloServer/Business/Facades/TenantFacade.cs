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

public class TenantFacade
{
    private readonly LoanService _loanService;
    private readonly ItemService _itemService;
    private readonly IAuthenticateService _authenticateService;
    private readonly IMapper _mapper;

    public TenantFacade(LoanService loanService, ItemService itemService, IAuthenticateService authenticateService,
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
        var loans = await _loanService.GetLoansByTenant(user, filter);

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
        if (loan.Tenant.Id != user.Id) throw new UnauthorizedAccessException("You are not the tenant of this loan.");

        return loan;
    }


    public async Task<Loan> CreateLoan(TenantLoanRequest request)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();
        if (user == null) throw new AuthenticationException();
        
        // get the item
        var item = await _itemService.Get(request.Item.Id);
        if (item == null) throw new EntityNotFoundException($"Item with id {request.Item.Id} was not found.");

        // Map request to loan
        var newLoan = _mapper.Map<Loan>(request);

        // Set default status
        newLoan.Status = LoanStatus.Inquired;

        // Set the tenant as the current user
        newLoan.Tenant = user;
        
        // Set the item
        newLoan.Item = item;
        newLoan.PricePerDay = item.PricePerDay;
        newLoan.RefundableDeposit = item.RefundableDeposit;
        
        // Calculate the price - take dates only to get rid of time so we get pretty accurate days
        var days = (newLoan.To.Date - newLoan.From.Date).TotalDays;
        // Set days to one if To and From are the same
        if (days < 1) days = 1;
        
        // Set the days and expected price
        newLoan.Days = (int) Math.Ceiling(days);
        newLoan.ExpectedPrice = newLoan.Days * newLoan.PricePerDay;

        // create the loan
        await _loanService.Create(newLoan);

        return newLoan;
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
            state.HandleTenant(oldLoan, request.Status.Value);
        }

        /*
        TODO: Change loan parameters
        In the future it would be great to have the option to change date, note etc.
        But these changes are possible to be made only if the loan is in some specific statuses.
        */

        await _loanService.Update(oldLoan);
    }
}
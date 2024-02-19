using AutoMapper;
using PujcovadloServer.AuthorizationHandlers.Exceptions;
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

    public virtual async Task<PaginatedList<Loan>> GetMyLoans(LoanFilter filter)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // Get loans where the user is the tenant
        var loans = await _loanService.GetLoansByTenant(user, filter);

        return loans;
    }

    private void PreCreateCheck(Loan loan)
    {
        // Check if the item is public
        if (loan.Item.Status != ItemStatus.Public)
            throw new OperationNotAllowedException("You can't borrow an item that is not public.");

        // Check if the tenant is the owner
        if (loan.Item.Owner == loan.Tenant)
            throw new OperationNotAllowedException("You can't borrow your own item.");
    }


    public async Task<Loan> CreateLoan(LoanRequest request)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // get the item
        var item = await _itemService.Get(request.ItemId);
        if (item == null) throw new EntityNotFoundException($"Item with id {request.ItemId} was not found.");

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

        // Set the expected price and days
        newLoan.Days = GetLoanDays(newLoan);
        newLoan.ExpectedPrice = GetLoanExpectedPrice(newLoan);

        PreCreateCheck(newLoan);

        // create the loan
        await _loanService.Create(newLoan);

        return newLoan;
    }

    public int GetLoanDays(Loan loan)
    {
        // Calculate the price - take dates only to get rid of time so we get pretty accurate days
        var days = (loan.To.Date - loan.From.Date).TotalDays;
        // Set days to one if To and From are the same
        if (days < 1) days = 1;

        // Set the days and expected price
        return (int)Math.Ceiling(days);
    }

    public float GetLoanExpectedPrice(Loan loan)
    {
        // Check if the days are greater than 0
        if (loan.Days <= 0) throw new ArgumentException("Days must be greater than 0.");

        return loan.Days * loan.PricePerDay;
    }

    public virtual async Task UpdateMyLoan(Loan loan, LoanUpdateRequest request)
    {
        // Get current user
        var user = await _authenticateService.GetCurrentUser();

        // Check that the user is the tenant
        if (loan.Tenant.Id != user.Id) throw new ForbiddenAccessException("You are not the tenant of this loan.");

        // Check if the status has been changed
        if (request.Status != null)
        {
            // get current state
            var state = _loanService.GetState(loan);

            // handle the request
            state.HandleTenant(loan, request.Status.Value);
        }

        /*
        TODO: Change loan parameters
        In the future it would be great to have the option to change date, note etc.
        But these changes are possible to be made only if the loan is in some specific statuses.
        */

        await _loanService.Update(loan);
    }
}
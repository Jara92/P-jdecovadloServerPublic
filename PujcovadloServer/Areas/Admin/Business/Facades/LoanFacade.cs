using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class LoanFacade
{
    private readonly LoanService _loanService;
    private readonly ItemService _itemService;
    private readonly ApplicationUserService _userService;
    private readonly ApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;

    public LoanFacade(LoanService loanService, ItemService itemService, ApplicationUserService userService,
        ApplicationUserService applicationUserService, IMapper mapper)
    {
        _loanService = loanService;
        _itemService = itemService;
        _userService = userService;
        _applicationUserService = applicationUserService;
        _mapper = mapper;
    }

    public async Task<Loan> Get(int id)
    {
        var category = await _loanService.Get(id);
        if (category == null) throw new EntityNotFoundException();

        return category;
    }

    private async Task FillRequest(Loan loan, LoanRequest request)
    {
        loan.Status = request.Status;

        loan.From = request.From;
        loan.To = request.To;
        loan.Days = request.Days; // todo: update days?

        loan.PricePerDay = request.PricePerDay;
        loan.ExpectedPrice = request.ExpectedPrice;
        loan.RefundableDeposit = request.RefundableDeposit;
        loan.TenantNote = request.TenantNote;

        loan.CreatedAt = request.CreatedAt ?? DateTime.Now;
        // todo: actions

        // Update tenant
        var tenant = await _applicationUserService.Get(request.TenantId);
        if (tenant == null) throw new ArgumentException("Tenant not found");
        loan.TenantId = tenant.Id;

        // Update item if exists
        var item = await _itemService.Get(request.ItemId);
        if (item == null) throw new ArgumentException("Item not found");

        loan.ItemId = item.Id;
    }

    public async Task Update(Loan loan, LoanRequest request)
    {
        await FillRequest(loan, request);

        await _loanService.Update(loan);
    }

    public Task Delete(Loan loan)
    {
        return _loanService.Delete(loan);
    }

    public Task<IList<ApplicationUserOption>> GetUserOptions()
    {
        return _userService.GetAllAsOptions(new ApplicationUserFilter());
    }

    public Task<List<EntityOption>> GetLoanStatusOptions()
    {
        // todo translate
        return _loanService.GetLoanStatusOptions();
    }
}
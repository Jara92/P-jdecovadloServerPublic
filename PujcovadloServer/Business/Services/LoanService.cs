using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.Factories.State;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Objects;
using PujcovadloServer.Business.States.Loan;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services;

public class LoanService : ACrudService<Loan, ILoanRepository, LoanFilter>
{
    private readonly LoanStateFactory _loanStateFactory;

    public LoanService(ILoanRepository repository, LoanStateFactory loanStateFactory) : base(repository)
    {
        _loanStateFactory = loanStateFactory;
    }

    public virtual async Task<PaginatedList<Loan>> GetLoansByTenant(ApplicationUser user, LoanFilter filter)
    {
        filter.TenantId = user.Id;
        return await _repository.GetAll(filter);
    }

    public virtual ILoanState GetState(Loan loan)
    {
        return _loanStateFactory.GetState(loan.Status);
    }

    public async Task<PaginatedList<Loan>> GetLoansByOwner(ApplicationUser user, LoanFilter filter)
    {
        filter.OwnerId = user.Id;
        return await _repository.GetAll(filter);
    }

    public virtual async Task<int> GetRunningLoansCountByItem(Item item)
    {
        return await _repository.GetRunningLoansCountByItem(item);
    }

    public virtual async Task<PaginatedList<Loan>> GetLoansByUserId(string userId, LoanFilter filter)
    {
        return await _repository.GetAllByUserId(userId, filter);
    }

    public Task<List<EntityOption>> GetLoanStatusOptions()
    {
        var statuses = new List<EntityOption>();
        foreach (var i in Enum.GetValues(typeof(LoanStatus)))
        {
            statuses.Add(new EntityOption()
            {
                Id = (int)i,
                Name = i.ToString(),
            });
        }

        return Task.FromResult(statuses);
    }
}
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Services;

public class LoanService(ILoanRepository repository) : ACrudService<Loan, ILoanRepository, LoanFilter>(repository)
{
    public async Task<PaginatedList<Loan>> GetLoansByTenant(ApplicationUser user, LoanFilter filter)
    {
        filter.TenantId = user.Id;
        return await _repository.GetAll(filter);
    }
}
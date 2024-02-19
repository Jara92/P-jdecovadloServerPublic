using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;
using PujcovadloServer.Lib;

namespace PujcovadloServer.Business.Interfaces;

public interface ILoanRepository : ICrudRepository<Loan, LoanFilter>
{
    public Task<List<Loan>> GetRunningLoansByItem(Item item);

    public Task<int> GetRunningLoansCountByItem(Item item);
    Task<PaginatedList<Loan>> GetAllByUserId(string userId, LoanFilter filter);
}
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface ILoanRepository : ICrudRepository<Loan, LoanFilter>
{
    public Task<List<Loan>> GetRunningLoansByItem(Item item);
    
    public Task<int> GetRunningLoansCountByItem(Item item);
}
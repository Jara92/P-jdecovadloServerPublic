using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Filters;

namespace PujcovadloServer.Business.Interfaces;

public interface ILoanRepository : ICrudRepository<Loan, LoanFilter>
{
}
using PujcovadloServer.Business.Enums;
using PujcovadloServer.Business.States;
using PujcovadloServer.Business.States.Loan;

namespace PujcovadloServer.Business.Factories.State;

public class LoanStateFactory
{
    private Dictionary<LoanStatus, ILoanState> _stateByType;
    
    public LoanStateFactory()
    {
        _stateByType = new Dictionary<LoanStatus, ILoanState>()
        {
            {LoanStatus.Inquired, new InquiredLoanState()},
            {LoanStatus.Accepted, new AcceptedLoanState()},
            {LoanStatus.Denied, new DeniedLoanState()},
            {LoanStatus.Cancelled, new CanceledLoanState()},
            {LoanStatus.PreparedForPickup, new PreparedForPickupLoanState()},
            {LoanStatus.PickupDenied, new PickupDeniedLoanState()},
            {LoanStatus.Active, new ActiveLoanState()},
            {LoanStatus.PreparedForReturn, new PreparedForReturnLoanState()},
            {LoanStatus.ReturnDenied, new ReturnDeniedLoanState()},
            {LoanStatus.Returned, new ReturnedLoanState()},
        };
    }

    public ILoanState GetState(LoanStatus stateType) => 
        _stateByType[stateType];
}
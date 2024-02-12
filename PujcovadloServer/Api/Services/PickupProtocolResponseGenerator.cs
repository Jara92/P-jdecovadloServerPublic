using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for PickupProtocol entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class PickupProtocolResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;

    public PickupProtocolResponseGenerator(IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor,
        AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// generates PickupProtocolResponse with detailed information about the protocol.
    /// </summary>
    /// <param name="protocol">PickupProtocol to be converted to a response.</param>
    /// <returns>PickupProtocolResponse which represents the protocol</returns>
    public async Task<PickupProtocolResponse> GeneratePickupProtocolDetailResponse(PickupProtocol protocol)
    {
        var response = _mapper.Map<PickupProtocolResponse>(protocol);

        // Link to loan
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.GetLoan), "Loan",
                values: new { protocol.Loan.Id }), "LOAN", "GET"));

        // Add update link if user has permission
        if (await _authorizationService.CanPerformOperation(protocol,
                PickupProtocolAuthorizationHandler.Operations.Update))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(PickupProtocolController.UpdateProtocol),
                    "PickupProtocol", values: new { protocol.Id }), "UPDATE", "PUT"));
        }

        // todo: add images link

        // todo: add link for creating a new image?

        return response;
    }
}
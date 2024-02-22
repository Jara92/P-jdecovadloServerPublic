using AutoMapper;
using PujcovadloServer.Api.Controllers;
using PujcovadloServer.AuthorizationHandlers;
using PujcovadloServer.AuthorizationHandlers.ReturnProtocol;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Facades;
using PujcovadloServer.Responses;

namespace PujcovadloServer.Api.Services;

/// <summary>
/// This class generates responses for ReturnProtocol entity.
/// Generates pagination and HATEOAS links for the responses.
/// </summary>
public class ReturnProtocolResponseGenerator : ABaseResponseGenerator
{
    private readonly IMapper _mapper;
    private readonly ImageFacade _imageFacade;

    public ReturnProtocolResponseGenerator(ImageFacade imageFacade, IMapper mapper, LinkGenerator urlHelper,
        IHttpContextAccessor httpContextAccessor, AuthorizationService authorizationService) :
        base(httpContextAccessor, urlHelper, authorizationService)
    {
        _imageFacade = imageFacade;
        _mapper = mapper;
    }

    /// <summary>
    /// generates ReturnProtocolResponse with detailed information about the protocol.
    /// </summary>
    /// <param name="protocol">ReturnProtocol to be converted to a response.</param>
    /// <returns>ReturnProtocolResponse which represents the protocol</returns>
    public async Task<ReturnProtocolResponse> GenerateReturnProtocolDetailResponse(ReturnProtocol protocol)
    {
        var response = _mapper.Map<ReturnProtocolResponse>(protocol);

        // Link to loan
        response.Links.Add(new LinkResponse(
            _urlHelper.GetUriByAction(_httpContext, nameof(LoanController.GetLoan), "Loan",
                values: new { protocol.Loan.Id }), "LOAN", "GET"));

        // Add update link if user has permission
        if (await _authorizationService.CanPerformOperation(protocol,
                ReturnProtocolOperations.Update))
        {
            response.Links.Add(new LinkResponse(
                _urlHelper.GetUriByAction(_httpContext, nameof(ReturnProtocolController.UpdateProtocol),
                    "ReturnProtocol", values: new { protocol.Id }), "UPDATE", "PUT"));
        }

        // Images link
        // Image links
        for (var i = 0; i < protocol.Images.Count; i++)
        {
            var imageUrl = await _imageFacade.GetImagePath(protocol.Images[i]);
            response.Images[i].Links.Add(new LinkResponse(imageUrl, "DATA", "GET"));
        }

        // todo: add link for creating a new image?

        return response;
    }
}
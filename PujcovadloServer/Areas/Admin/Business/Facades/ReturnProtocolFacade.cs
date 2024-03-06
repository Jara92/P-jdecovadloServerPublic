using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class ReturnProtocolFacade
{
    private readonly ReturnProtocolService _protocolService;
    private readonly LoanService _loanService;
    private readonly IMapper _mapper;
    private readonly IFileStorage _storage;

    public ReturnProtocolFacade(ReturnProtocolService protocolService, LoanService loanService, IMapper mapper,
        IFileStorage storage)
    {
        _protocolService = protocolService;
        _loanService = loanService;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<ReturnProtocol> Get(int id)
    {
        var protocol = await _protocolService.Get(id);
        if (protocol == null) throw new EntityNotFoundException();

        foreach (var image in protocol.Images)
        {
            image.Url = await _storage.GetFilePublicUrl("images", image.Path);
        }

        return protocol;
    }

    private Task FillRequest(ReturnProtocol protocol, ReturnProtocolRequest request)
    {
        protocol.Description = request.Description;
        protocol.ReturnedRefundableDeposit = request.ReturnedRefundableDeposit;
        protocol.ConfirmedAt = request.ConfirmedAt;
        protocol.CreatedAt = request.CreatedAt;
        protocol.UpdatedAt = request.UpdatedAt;

        return Task.CompletedTask;
    }

    public async Task Update(ReturnProtocol protocol, ReturnProtocolRequest request)
    {
        await FillRequest(protocol, request);

        await _protocolService.Update(protocol);
    }
}
using AutoMapper;
using PujcovadloServer.Areas.Admin.Requests;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Exceptions;
using PujcovadloServer.Business.Interfaces;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Areas.Admin.Business.Facades;

public class PickupProtocolFacade
{
    private readonly PickupProtocolService _protocolService;
    private readonly LoanService _loanService;
    private readonly IMapper _mapper;
    private readonly IFileStorage _storage;

    public PickupProtocolFacade(PickupProtocolService protocolService, LoanService loanService, IMapper mapper,
        IFileStorage storage)
    {
        _protocolService = protocolService;
        _loanService = loanService;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<PickupProtocol> Get(int id)
    {
        var protocol = await _protocolService.Get(id);
        if (protocol == null) throw new EntityNotFoundException();

        foreach (var image in protocol.Images)
        {
            image.Url = await _storage.GetFilePublicUrl("images", image.Path);
        }

        return protocol;
    }

    private Task FillRequest(PickupProtocol protocol, PickupProtocolRequest request)
    {
        protocol.Description = request.Description;
        protocol.AcceptedRefundableDeposit = request.AcceptedRefundableDeposit;
        protocol.ConfirmedAt = request.ConfirmedAt;
        protocol.CreatedAt = request.CreatedAt;
        protocol.UpdatedAt = request.UpdatedAt;

        return Task.CompletedTask;
    }

    public async Task Update(PickupProtocol protocol, PickupProtocolRequest request)
    {
        await FillRequest(protocol, request);

        await _protocolService.Update(protocol);
    }
}
using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;

namespace InvoiceSystem.Wpf.Tests.Fakes;

public sealed class FakeMemberService : IMemberService
{
    public MemberDetailDto? MemberToReturn { get; set; }
    public Exception? GetByIdException { get; set; }

    public int? UpdatedId { get; private set; }
    public MemberUpdateRequest? UpdatedRequest { get; private set; }

    public int? DisabledId { get; private set; }

    public Task<MemberDetailDto?> GetByIdAsync(int id)
    {
        if (GetByIdException is not null)
            throw GetByIdException;

        return Task.FromResult(MemberToReturn);
    }

    public Task UpdateAsync(int id, MemberUpdateRequest request)
    {
        UpdatedId = id;
        UpdatedRequest = request;
        return Task.CompletedTask;
    }

    public Task DisableAsync(int id)
    {
        DisabledId = id;
        return Task.CompletedTask;
    }
}
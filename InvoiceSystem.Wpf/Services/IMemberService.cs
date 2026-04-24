using InvoiceSystem.Wpf.Models;

namespace InvoiceSystem.Wpf.Services;

public interface IMemberService
{
    Task<MemberDetailDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, MemberUpdateRequest request);
    Task DisableAsync(int id);
}
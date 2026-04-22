using InvoiceSystem.Wpf.Models;

namespace InvoiceSystem.Wpf.Services;

public interface IInvoiceService
{
    Task<CollectionSnapshotDto> GetCollectionSnapshotAsync(long invoiceId);
    Task<List<DunningLogDto>> GetCollectionLogsAsync(long invoiceId);
    Task<long> CreateCollectionLogAsync(long invoiceId, CreateDunningLogRequestDto request);

    Task<InvoiceDetailDto> GetAdminInvoiceDetailAsync(long invoiceId);
    Task<PdfDownloadResult> GetAdminInvoicePdfAsync(long invoiceId);

    Task<List<MemberOptionDto>> GetMemberOptionsAsync();
    Task<long> CreateAdminInvoiceAsync(InvoiceUpsertRequestDto request);
    Task UpdateAdminInvoiceAsync(long invoiceId, InvoiceUpsertRequestDto request);

    Task<List<InvoiceListItemDto>> SearchInvoicesAsync(InvoiceSearchRequest request);
}
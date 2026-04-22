using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceSystem.Wpf.Tests.Fakes;

public sealed class FakeInvoiceService : IInvoiceService
{
    // =========================
    // AdminCollectionViewModel 用
    // =========================
    public CollectionSnapshotDto? SnapshotToReturn { get; set; }
    public List<DunningLogDto> LogsToReturn { get; set; } = new();

    public Exception? ExceptionToThrowOnGetSnapshot { get; set; }
    public Exception? ExceptionToThrowOnGetLogs { get; set; }
    public Exception? ExceptionToThrowOnCreateLog { get; set; }

    public long LastInvoiceIdForSnapshot { get; private set; }
    public long LastInvoiceIdForLogs { get; private set; }
    public long LastInvoiceIdForCreate { get; private set; }

    public CreateDunningLogRequestDto? LastCreateRequest { get; private set; }

    // =========================
    // AdminInvoiceDetailViewModel 用
    // =========================
    public InvoiceDetailDto? AdminInvoiceDetailToReturn { get; set; }
    public PdfDownloadResult? AdminInvoicePdfToReturn { get; set; }

    public Exception? ExceptionToThrowOnGetAdminInvoiceDetail { get; set; }
    public Exception? ExceptionToThrowOnGetAdminInvoicePdf { get; set; }

    public long LastInvoiceIdForAdminDetail { get; private set; }
    public long LastInvoiceIdForAdminPdf { get; private set; }

    // =========================
    // AdminInvoiceEditViewModel 用
    // =========================
    public List<MemberOptionDto> MemberOptionsToReturn { get; set; } = new();

    public Exception? ExceptionToThrowOnGetMemberOptions { get; set; }
    public Exception? ExceptionToThrowOnCreateAdminInvoice { get; set; }
    public Exception? ExceptionToThrowOnUpdateAdminInvoice { get; set; }

    public int GetMemberOptionsCallCount { get; private set; }

    public InvoiceUpsertRequestDto? LastCreateAdminInvoiceRequest { get; private set; }
    public long LastUpdateAdminInvoiceId { get; private set; }
    public InvoiceUpsertRequestDto? LastUpdateAdminInvoiceRequest { get; private set; }

    // =========================
    // InvoiceListViewModel 用
    // =========================
    public List<InvoiceListItemDto> SearchInvoicesResultToReturn { get; set; } = new();
    public Exception? ExceptionToThrowOnSearchInvoices { get; set; }
    public InvoiceSearchRequest? LastSearchInvoicesRequest { get; private set; }

    public Task<CollectionSnapshotDto> GetCollectionSnapshotAsync(long invoiceId)
    {
        LastInvoiceIdForSnapshot = invoiceId;

        if (ExceptionToThrowOnGetSnapshot != null)
        {
            return Task.FromException<CollectionSnapshotDto>(ExceptionToThrowOnGetSnapshot);
        }

        return Task.FromResult(SnapshotToReturn ?? new CollectionSnapshotDto());
    }

    public Task<List<DunningLogDto>> GetCollectionLogsAsync(long invoiceId)
    {
        LastInvoiceIdForLogs = invoiceId;

        if (ExceptionToThrowOnGetLogs != null)
        {
            return Task.FromException<List<DunningLogDto>>(ExceptionToThrowOnGetLogs);
        }

        return Task.FromResult(LogsToReturn);
    }

    public Task<long> CreateCollectionLogAsync(long invoiceId, CreateDunningLogRequestDto request)
    {
        LastInvoiceIdForCreate = invoiceId;
        LastCreateRequest = request;

        if (ExceptionToThrowOnCreateLog != null)
        {
            return Task.FromException<long>(ExceptionToThrowOnCreateLog);
        }

        return Task.FromResult(1L);
    }

    public Task<InvoiceDetailDto> GetAdminInvoiceDetailAsync(long invoiceId)
    {
        LastInvoiceIdForAdminDetail = invoiceId;

        if (ExceptionToThrowOnGetAdminInvoiceDetail != null)
        {
            return Task.FromException<InvoiceDetailDto>(ExceptionToThrowOnGetAdminInvoiceDetail);
        }

        return Task.FromResult(AdminInvoiceDetailToReturn ?? new InvoiceDetailDto());
    }

    public Task<PdfDownloadResult> GetAdminInvoicePdfAsync(long invoiceId)
    {
        LastInvoiceIdForAdminPdf = invoiceId;

        if (ExceptionToThrowOnGetAdminInvoicePdf != null)
        {
            return Task.FromException<PdfDownloadResult>(ExceptionToThrowOnGetAdminInvoicePdf);
        }

        return Task.FromResult(AdminInvoicePdfToReturn ?? new PdfDownloadResult
        {
            Content = new byte[] { 1, 2, 3 },
            FileName = "test.pdf",
            ContentType = "application/pdf"
        });
    }

    public Task<List<MemberOptionDto>> GetMemberOptionsAsync()
    {
        GetMemberOptionsCallCount++;

        if (ExceptionToThrowOnGetMemberOptions != null)
        {
            return Task.FromException<List<MemberOptionDto>>(ExceptionToThrowOnGetMemberOptions);
        }

        return Task.FromResult(MemberOptionsToReturn);
    }

    public Task<long> CreateAdminInvoiceAsync(InvoiceUpsertRequestDto request)
    {
        LastCreateAdminInvoiceRequest = request;

        if (ExceptionToThrowOnCreateAdminInvoice != null)
        {
            return Task.FromException<long>(ExceptionToThrowOnCreateAdminInvoice);
        }

        return Task.FromResult(1L);
    }

    public Task UpdateAdminInvoiceAsync(long invoiceId, InvoiceUpsertRequestDto request)
    {
        LastUpdateAdminInvoiceId = invoiceId;
        LastUpdateAdminInvoiceRequest = request;

        if (ExceptionToThrowOnUpdateAdminInvoice != null)
        {
            return Task.FromException(ExceptionToThrowOnUpdateAdminInvoice);
        }

        return Task.CompletedTask;
    }


    public Task<List<InvoiceListItemDto>> SearchInvoicesAsync(InvoiceSearchRequest request)
    {
        LastSearchInvoicesRequest = request;

        if (ExceptionToThrowOnSearchInvoices != null)
        {
            return Task.FromException<List<InvoiceListItemDto>>(ExceptionToThrowOnSearchInvoices);
        }

        return Task.FromResult(SearchInvoicesResultToReturn);
    }
}
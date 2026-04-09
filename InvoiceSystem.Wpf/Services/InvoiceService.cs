using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using InvoiceSystem.Wpf.Models;

namespace InvoiceSystem.Wpf.Services;

public class InvoiceService
{
    private readonly HttpClient _httpClient;

    public InvoiceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AccountInvoiceListDto> GetMemberInvoicesAsync(
        int year,
        string month,
        string status,
        string q,
        int page,
        int pageSize = 10)
    {
        var requestUri =
            $"/api/members/me/invoices" +
            $"?year={year}" +
            $"&month={Uri.EscapeDataString(month ?? "all")}" +
            $"&status={Uri.EscapeDataString(status ?? "all")}" +
            $"&q={Uri.EscapeDataString(q ?? string.Empty)}" +
            $"&page={page}" +
            $"&pageSize={pageSize}";

        using var response = await _httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AccountInvoiceListDto>();

            if (result == null)
            {
                throw new Exception("請求書一覧応答の解析に失敗しました。");
            }

            return result;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var message = TryReadMessage(responseText);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new Exception("ログイン情報の有効期限が切れています。再度ログインしてください。");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new Exception("この請求書情報を参照する権限がありません。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception(message ?? "請求書一覧の取得条件が不正です。");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new Exception("請求書一覧データが見つかりませんでした。");
        }

        throw new Exception(message ?? "請求書一覧の取得に失敗しました。時間をおいて再度お試しください。");
    }

    public async Task<InvoiceDetailDto> GetMemberInvoiceDetailAsync(long invoiceId)
    {
        using var response = await _httpClient.GetAsync($"/api/members/me/invoices/{invoiceId}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<InvoiceDetailDto>();

            if (result == null)
            {
                throw new Exception("請求書詳細応答の解析に失敗しました。");
            }

            return result;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var message = TryReadMessage(responseText);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new Exception("ログイン情報の有効期限が切れています。再度ログインしてください。");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new Exception("この請求書を参照する権限がありません。");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new Exception("請求書が見つかりませんでした。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception(message ?? "請求書詳細の取得条件が不正です。");
        }

        throw new Exception(message ?? "請求書詳細の取得に失敗しました。時間をおいて再度お試しください。");
    }

    private static string? TryReadMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }
}
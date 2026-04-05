using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using InvoiceSystem.Wpf.Models;

namespace InvoiceSystem.Wpf.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null)
            {
                throw new Exception("ログイン応答の解析に失敗しました。");
            }

            return result;
        }

        var responseText = await response.Content.ReadAsStringAsync();

        string? message = TryReadMessage(responseText);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new Exception("メールアドレスまたはパスワードが正しくありません。");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new Exception("このアカウントは利用できません。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception(message ?? "不正なリクエストです。");
        }

        throw new Exception("ログインに失敗しました。時間をおいて再度お試しください。");
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("/auth/forgot-password", request);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        string? message = TryReadMessage(responseText);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception(message ?? "メール送信に失敗しました。メールアドレスをご確認ください。");
        }

        throw new Exception("再設定メールの送信に失敗しました。時間をおいて再度お試しください。");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("/auth/reset-password", request);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        string? message = TryReadMessage(responseText);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception(message ?? "パスワードの再設定に失敗しました。");
        }

        throw new Exception("パスワードの再設定に失敗しました。時間をおいて再度お試しください。");
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
using System;
using System.Threading.Tasks;
using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;

namespace InvoiceSystem.Wpf.Tests.Fakes;

public sealed class FakeAuthService : IAuthService
{
    public LoginRequest? LastRequest { get; private set; }

    public LoginResponse? ResponseToReturn { get; set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        LastRequest = request;

        if (ExceptionToThrow != null)
        {
            return Task.FromException<LoginResponse>(ExceptionToThrow);
        }

        return Task.FromResult(ResponseToReturn ?? new LoginResponse());
    }

    public void Logout()
    {
    }
}
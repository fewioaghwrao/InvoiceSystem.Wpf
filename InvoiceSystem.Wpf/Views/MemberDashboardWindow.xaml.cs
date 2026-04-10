using InvoiceSystem.Wpf.Services;
using System;
using System.Reflection;
using System.Windows;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberDashboardWindow : Window
{
    private readonly object? _currentUser;
    private readonly AuthService _authService;
    private readonly InvoiceService _invoiceService;
    private readonly AccountService _accountService;

    public MemberDashboardWindow(
        object? currentUser,
        AuthService authService,
    InvoiceService invoiceService,
    AccountService accountService)
    {
        InitializeComponent();

        _currentUser = currentUser;
        _authService = authService;
        _invoiceService = invoiceService;
        _accountService = accountService;

        LoadCurrentUser();
    }

    private void LoadCurrentUser()
    {
        var name = ReadProperty(_currentUser, "Name") ?? "会員ユーザー";
        var email = ReadProperty(_currentUser, "Email") ?? "メールアドレス未設定";
        var roleRaw = ReadProperty(_currentUser, "Role")
                   ?? ReadProperty(_currentUser, "RoleName")
                   ?? ReadProperty(_currentUser, "UserRole");

        var roleLabel = ToRoleLabel(roleRaw);

        NameText.Text = name;
        EmailText.Text = email;
        RoleText.Text = roleLabel;
        RoleBadgeText.Text = roleLabel;
        WelcomeText.Text = $"ようこそ、{name}さん";
        SubWelcomeText.Text = $"{email} でログイン中";
    }

    private static string? ReadProperty(object? target, string propertyName)
    {
        if (target == null) return null;

        var prop = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        var value = prop?.GetValue(target);
        return value?.ToString();
    }

    private static string ToRoleLabel(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "一般会員";

        var normalized = role.Trim().ToUpperInvariant();

        return normalized switch
        {
            "ADMIN" => "管理者",
            "MEMBER" => "一般会員",
            "USER" => "一般会員",
            "2" => "一般会員",
            "1" => "管理者",
            "9" => "退会",
            _ => role
        };
    }

    private void InvoicesButton_OnClick(object sender, RoutedEventArgs e)
    {
        var invoiceListWindow = new MemberInvoiceListWindow(_currentUser, _authService, _invoiceService, _accountService);
        invoiceListWindow.Show();
        Close();
    }

    private void UnpaidButton_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new MemberPaymentStatusWindow(_currentUser, _authService, _invoiceService,_accountService);
        window.Show();
        Close();
    }

    private void ProfileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new MemberProfileWindow(_currentUser, _authService, _invoiceService, _accountService);
        window.Show();
        Close();
    }

    private void BackToLoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "ログイン画面へ戻りますか？",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "ログアウトしますか？",
            "ログアウト確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        _authService.Logout();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }
}
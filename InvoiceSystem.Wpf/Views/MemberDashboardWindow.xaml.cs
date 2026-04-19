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
        var confirmed = ShowConfirmDialog(
            title: "ログイン画面へ戻る",
            message: "現在の画面を閉じて、ログイン画面へ戻りますか？",
            confirmText: "戻る");

        if (!confirmed)
            return;

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
    {
        var confirmed = ShowConfirmDialog(
            title: "ログアウト確認",
            message: "現在のセッションを終了してログアウトしますか？",
            confirmText: "ログアウト",
            visualType: ConfirmDialogWindow.DialogVisualType.DangerConfirm,
            subMessage: "ログアウトすると、再度ログインが必要になります。");

        if (!confirmed)
            return;

        _authService.Logout();

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private bool ShowConfirmDialog(
        string title,
        string message,
        string confirmText,
        ConfirmDialogWindow.DialogVisualType visualType = ConfirmDialogWindow.DialogVisualType.Default,
        string? subMessage = null)
    {
        var dialog = new ConfirmDialogWindow(
            title: title,
            message: message,
            confirmText: confirmText,
            cancelText: "キャンセル",
            visualType: visualType,
            subMessage: subMessage)
        {
            Owner = this
        };

        var result = dialog.ShowDialog();
        return result == true && dialog.IsConfirmed;
    }
}
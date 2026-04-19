using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberProfileWindow : Window
{
    private readonly object? _currentUser;
    private readonly AuthService _authService;
    private readonly InvoiceService _invoiceService;
    private readonly AccountService _accountService;

    private AccountProfileDto? _profile;
    private string _originalEmail = string.Empty;

    public MemberProfileWindow(
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

        Loaded += MemberProfileWindow_Loaded;
        EmailTextBox.TextChanged += EmailTextBox_TextChanged;
    }

    private async void MemberProfileWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            ToggleLoading(true);
            ErrorText.Text = string.Empty;

            var profile = await _accountService.GetMyProfileAsync();
            _profile = profile;
            _originalEmail = profile.Email;

            BindProfile(profile);
            UpdateEmailChangedNotice();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;

            MessageBox.Show(
                $"プロフィールの取得に失敗しました。\n\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void BindProfile(AccountProfileDto profile)
    {
        NameTextBox.Text = profile.Name;
        EmailTextBox.Text = profile.Email;
        PhoneTextBox.Text = profile.Phone ?? string.Empty;
        PostalCodeTextBox.Text = profile.PostalCode ?? string.Empty;
        AddressTextBox.Text = profile.Address ?? string.Empty;
    }

    private AccountProfileDto BuildProfileFromForm()
    {
        return new AccountProfileDto
        {
            Id = _profile?.Id ?? 0,
            Name = NameTextBox.Text?.Trim() ?? string.Empty,
            Email = EmailTextBox.Text?.Trim() ?? string.Empty,
            Phone = NormalizeNullable(PhoneTextBox.Text),
            PostalCode = NormalizeNullable(PostalCodeTextBox.Text),
            Address = NormalizeNullable(AddressTextBox.Text)
        };
    }

    private string[] Validate(AccountProfileDto profile)
    {
        var issues = new System.Collections.Generic.List<string>();

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            issues.Add("氏名は必須です。");
        }

        if (string.IsNullOrWhiteSpace(profile.Email))
        {
            issues.Add("メールアドレスは必須です。");
        }
        else if (!AccountService.IsEmailLike(profile.Email))
        {
            issues.Add("メールアドレスの形式が正しくありません。");
        }

        return issues.ToArray();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var profile = BuildProfileFromForm();
        var issues = Validate(profile);

        ErrorText.Text = string.Empty;

        if (issues.Length > 0)
        {
            ErrorText.Text = string.Join(Environment.NewLine, issues);
            return;
        }

        var emailChanged = !string.Equals(_originalEmail, profile.Email, StringComparison.Ordinal);

        var confirmMessage =
            "次の内容で保存します。よろしいですか？\n\n" +
            $"氏名：{profile.Name}\n" +
            $"メール：{profile.Email}\n" +
            $"電話：{profile.Phone ?? "—"}\n" +
            $"郵便番号：{profile.PostalCode ?? "—"}\n" +
            $"住所：{profile.Address ?? "—"}";

        if (emailChanged)
        {
            confirmMessage +=
                $"\n\nメールアドレスが変更されます。\n変更前：{_originalEmail}\n変更後：{profile.Email}\n\n※ Lite版のため、確認メールは送信されません。";
        }

        var confirmed = ShowConfirmDialog(
            title: "保存内容の確認",
            message: "入力内容を保存します。よろしいですか？",
            confirmText: "保存する",
            visualType: ConfirmDialogWindow.DialogVisualType.SaveConfirm,
            subMessage: confirmMessage);

        if (!confirmed)
        {
            return;
        }

        try
        {
            ToggleLoading(true);

            await _accountService.UpdateMyProfileAsync(profile);

            _profile = profile;
            _originalEmail = profile.Email;
            UpdateEmailChangedNotice();

            MessageBox.Show(
                "プロフィールを保存しました。",
                "保存完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;

            MessageBox.Show(
                $"プロフィールの保存に失敗しました。\n\n{ex.Message}",
                "保存エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private async void WithdrawButton_Click(object sender, RoutedEventArgs e)
    {
        var confirmed = ShowConfirmDialog(
            title: "退会確認",
            message: "本当に退会しますか？",
            confirmText: "退会する",
            visualType: ConfirmDialogWindow.DialogVisualType.DangerConfirm,
            subMessage: "退会後はログインできなくなります。この操作は取り消せません。");

        if (!confirmed)
        {
            return;
        }

        try
        {
            ToggleLoading(true);
            await _accountService.DeleteMyAccountAsync();

            MessageBox.Show(
                "退会処理が完了しました。",
                "退会完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;

            MessageBox.Show(
                $"退会処理に失敗しました。\n\n{ex.Message}",
                "退会エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var dashboard = new MemberDashboardWindow(_currentUser, _authService, _invoiceService, _accountService);

        dashboard.Show();
        Close();
    }

    private void EmailTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateEmailChangedNotice();
    }

    private void UpdateEmailChangedNotice()
    {
        var currentEmail = EmailTextBox.Text?.Trim() ?? string.Empty;
        var changed = !string.IsNullOrWhiteSpace(_originalEmail)
            && !string.Equals(_originalEmail, currentEmail, StringComparison.Ordinal);

        EmailChangedNotice.Visibility = changed ? Visibility.Visible : Visibility.Collapsed;
        EmailChangedText.Text = changed
            ? $"変更前：{_originalEmail}\n変更後：{currentEmail}"
            : string.Empty;
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingText.Text = isLoading ? "処理中..." : string.Empty;
        LoadingText.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

        SaveButton.IsEnabled = !isLoading;
        WithdrawButton.IsEnabled = !isLoading;
    }

    private static string? NormalizeNullable(string? value)
    {
        var v = value?.Trim();
        return string.IsNullOrWhiteSpace(v) ? null : v;
    }

    private bool ShowConfirmDialog(
    string title,
    string message,
    string confirmText,
    ConfirmDialogWindow.DialogVisualType visualType,
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
using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using InvoiceSystem.Wpf.ViewModels;
using System.Windows;

namespace InvoiceSystem.Wpf.Views;

public partial class AdminDashboardWindow : Window
{
    private readonly AdminDashboardViewModel _viewModel;
    private readonly AuthService _authService;
    private readonly MemberService _memberService;
    private readonly InvoiceService _invoiceService;

    public AdminDashboardWindow(
        CurrentUser? currentUser,
        AuthService authService,
        InvoiceService invoiceService,
        AccountService accountService,
        AdminService adminService,
        MemberService memberService)
    {
        InitializeComponent();

        _authService = authService;
        _invoiceService = invoiceService;
        _memberService = memberService;

        _viewModel = new AdminDashboardViewModel(adminService, currentUser);
        DataContext = _viewModel;

        Loaded += AdminDashboardWindow_Loaded;
    }

    private async void AdminDashboardWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private async void PreviousYearButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.MovePreviousYear();
        await _viewModel.LoadAsync();
    }

    private async void NextYearButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.MoveNextYear();
        await _viewModel.LoadAsync();
    }

    private async void ReloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
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

        try
        {
            _authService.Logout();
        }
        catch
        {
        }

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private void InvoicesButton_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new InvoiceListWindow(_invoiceService)
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private void MembersButton_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new MemberListWindow(_memberService)
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private void SalesButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "売上一覧画面への遷移は今後接続予定です。",
            "案内",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
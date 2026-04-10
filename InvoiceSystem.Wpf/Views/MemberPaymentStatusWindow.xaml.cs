using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberPaymentStatusWindow : Window
{
    private readonly object? _currentUser;
    private readonly AuthService _authService;
    private readonly InvoiceService _invoiceService;
    private readonly AccountService _AccountService;

    private const int PageSize = 50;

    public ObservableCollection<MemberPaymentStatusRowViewModel> Rows { get; } = new();

    public MemberPaymentStatusWindow(
        object? currentUser,
        AuthService authService,
        InvoiceService invoiceService,
        AccountService accountService)
    {
        InitializeComponent();
        DataContext = this;

        _currentUser = currentUser;
        _authService = authService;
        _invoiceService = invoiceService;


        InitializeUserHeader();

        Loaded += MemberPaymentStatusWindow_Loaded;
        _AccountService = accountService;
    }

    private async void MemberPaymentStatusWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private void InitializeUserHeader()
    {
        var name = ReadProperty(_currentUser, "Name") ?? "会員ユーザー";
        var email = ReadProperty(_currentUser, "Email") ?? "メールアドレス未設定";

        WelcomeText.Text = $"ようこそ、{name}さん";
        SubWelcomeText.Text = $"{email} の未払い状況を表示しています";
    }

    private async Task LoadAsync()
    {
        try
        {
            ToggleLoading(true);
            Rows.Clear();

            var year = DateTime.Now.Year;
            const string month = "all";
            const string q = "";
            const int page = 1;

            var unpaid = await _invoiceService.GetMemberInvoicesWithBalanceAsync(
                year,
                month,
                "unpaid",
                q,
                page,
                PageSize);

            var partial = await _invoiceService.GetMemberInvoicesWithBalanceAsync(
                year,
                month,
                "partial",
                q,
                page,
                PageSize);

            var mergedItems = (unpaid.Items ?? Enumerable.Empty<AccountInvoiceListItemDto>())
                .Concat(partial.Items ?? Enumerable.Empty<AccountInvoiceListItemDto>())
                .OrderBy(x => x.DueAt)
                .Take(PageSize)
                .ToList();

            foreach (var item in mergedItems)
            {
                Rows.Add(MemberPaymentStatusRowViewModel.FromDto(item));
            }

            BindSummary();
        }
        catch (Exception ex)
        {
            Rows.Clear();
            BindSummary();

            MessageBox.Show(
                $"入金確認情報の取得に失敗しました。\n\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void BindSummary()
    {
        var count = Rows.Count;
        var remainingTotal = Rows.Sum(x => Math.Max(0m, x.RemainingAmount));
        var overdueCount = Rows.Count(x => x.IsOverdue && x.RemainingAmount > 0);

        SummaryText.Text = $"{count:N0} 件";
        UnpaidCountText.Text = $"{count:N0} 件";
        RemainingTotalText.Text = $"{remainingTotal:N0} 円";
        OverdueCountText.Text = $"{overdueCount:N0} 件";
        OverdueBanner.Visibility = overdueCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var dashboard = new MemberDashboardWindow(_currentUser, _authService, _invoiceService,_AccountService);
        dashboard.Show();
        Close();
    }

    private void DetailButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not MemberPaymentStatusRowViewModel row)
            return;

        var detailWindow = new MemberInvoiceDetailWindow(
            row.Id,
            _currentUser,
            _authService,
            _invoiceService,
            _AccountService);

        detailWindow.Show();
        Close();
    }

    private async void PdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not MemberPaymentStatusRowViewModel row)
            return;

        try
        {
            ToggleLoading(true);

            var pdf = await _invoiceService.GetMemberInvoicePdfAsync(row.Id);

            if (!string.Equals(pdf.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("PDF形式ではないレスポンスを受信しました。");
            }

            var filePath = InvoiceService.SavePdfToTempFile(pdf);

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"PDFを開けませんでした。\n\n{ex.Message}",
                "PDF表示エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingText.Text = isLoading ? "読み込み中..." : string.Empty;
        LoadingText.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

        PaymentStatusDataGrid.IsEnabled = !isLoading;
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
}

public class MemberPaymentStatusRowViewModel
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime DueAt { get; set; }
    public decimal RemainingAmount { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }

    public string DueAtText => DueAt == default ? string.Empty : DueAt.ToString("yyyy/MM/dd");
    public string RemainingAmountText => $"{Math.Max(0m, RemainingAmount):N0} 円";

    public bool IsPaid =>
        !string.IsNullOrWhiteSpace(StatusName) &&
        StatusName.Contains("入金済");

    public Visibility OverdueBadgeVisibility =>
        IsOverdue && RemainingAmount > 0 && !IsPaid ? Visibility.Visible : Visibility.Collapsed;

    public string StatusForeground =>
        StatusName switch
        {
            var s when s.Contains("入金済") => "#86EFAC",
            var s when s.Contains("一部") => "#FCD34D",
            var s when s.Contains("未入金") => "#FDA4AF",
            _ => "#E2E8F0"
        };

    public string StatusBackground =>
        StatusName switch
        {
            var s when s.Contains("入金済") => "#0B2A1E",
            var s when s.Contains("一部") => "#3B2506",
            var s when s.Contains("未入金") => "#3A1118",
            _ => "#132032"
        };

    public string StatusBorderBrush =>
        StatusName switch
        {
            var s when s.Contains("入金済") => "#166534",
            var s when s.Contains("一部") => "#92400E",
            var s when s.Contains("未入金") => "#9F1239",
            _ => "#334155"
        };

    public static MemberPaymentStatusRowViewModel FromDto(AccountInvoiceListItemDto dto)
    {
        return new MemberPaymentStatusRowViewModel
        {
            Id = dto.Id,
            InvoiceNumber = dto.InvoiceNumber ?? string.Empty,
            DueAt = dto.DueAt,
            RemainingAmount = dto.RemainingAmount,
            StatusName = dto.StatusName ?? string.Empty,
            IsOverdue = dto.IsOverdue
        };
    }
}

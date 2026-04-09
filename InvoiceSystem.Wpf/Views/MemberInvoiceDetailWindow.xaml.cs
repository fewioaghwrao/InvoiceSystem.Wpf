using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberInvoiceDetailWindow : Window
{
    private readonly long _invoiceId;
    private readonly object? _currentUser;
    private readonly AuthService _authService;
    private readonly InvoiceService _invoiceService;

    private string? _pdfUrl;

    public MemberInvoiceDetailWindow(
        long invoiceId,
        object? currentUser,
        AuthService authService,
        InvoiceService invoiceService)
    {
        InitializeComponent();

        _invoiceId = invoiceId;
        _currentUser = currentUser;
        _authService = authService;
        _invoiceService = invoiceService;

        Loaded += MemberInvoiceDetailWindow_Loaded;
    }

    private async void MemberInvoiceDetailWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDetailAsync();
    }

    private async Task LoadDetailAsync()
    {
        try
        {
            ToggleLoading(true);
            ErrorText.Text = string.Empty;

            var detail = await _invoiceService.GetMemberInvoiceDetailAsync(_invoiceId);
            BindDetail(detail);
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;

            MessageBox.Show(
                $"請求書詳細の取得に失敗しました。\n\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void BindDetail(InvoiceDetailDto detail)
    {
        InvoiceNumberText.Text = detail.InvoiceNumber;
        StatusText.Text = detail.StatusName;
        InvoiceDateText.Text = FormatDate(detail.InvoiceDate);
        DueDateText.Text = FormatDate(detail.DueDate);
        TotalAmountText.Text = FormatMoney(detail.TotalAmount);
        PaidAmountText.Text = FormatMoney(detail.PaidAmount);
        RemainingAmountText.Text = FormatMoney(detail.RemainingAmount);
        RemarksText.Text = string.IsNullOrWhiteSpace(detail.Remarks) ? "備考はありません。" : detail.Remarks;

        _pdfUrl = !string.IsNullOrWhiteSpace(detail.PdfUrl)
            ? detail.PdfUrl
            : detail.PdfPath;

        var isOverdue =
            detail.DueDate.Date < DateTime.Today &&
            detail.RemainingAmount > 0;

        OverdueBadge.Visibility = isOverdue ? Visibility.Visible : Visibility.Collapsed;

        ApplyStatusStyle(detail.StatusName);
        PdfButton.IsEnabled = !string.IsNullOrWhiteSpace(_pdfUrl);
    }

    private void ApplyStatusStyle(string statusName)
    {
        if (statusName.Contains("入金済"))
        {
            StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B2A1E"));
            StatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EFAC"));
            return;
        }

        if (statusName.Contains("一部"))
        {
            StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2506"));
            StatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D"));
            return;
        }

        if (statusName.Contains("未入金"))
        {
            StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A1118"));
            StatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9F1239"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDA4AF"));
            return;
        }

        StatusBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#132032"));
        StatusBadge.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
        StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new MemberInvoiceListWindow(_currentUser, _authService, _invoiceService);
        window.Show();
        Close();
    }

    private void PdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_pdfUrl))
        {
            MessageBox.Show(
                "PDFのURLまたはパスが設定されていません。",
                "PDF表示",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _pdfUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"PDFを開けませんでした。\n{ex.Message}",
                "PDF表示エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingText.Text = isLoading ? "読み込み中..." : string.Empty;
        LoadingText.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string FormatDate(DateTime value)
    {
        if (value == default)
            return "---";

        return value.ToString("yyyy/MM/dd");
    }

    private static string FormatMoney(decimal value)
    {
        return $"{value:N0} 円";
    }
}
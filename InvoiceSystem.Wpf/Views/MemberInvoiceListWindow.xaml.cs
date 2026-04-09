using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberInvoiceListWindow : Window
{
    private readonly object? _currentUser;
    private readonly AuthService _authService;
    private readonly InvoiceService _invoiceService;

    private const int DefaultPageSize = 10;

    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _selectedYear;
    private string _selectedMonth = "all";
    private string _selectedStatus = "all";
    private string _keyword = string.Empty;

    public ObservableCollection<MemberInvoiceRowViewModel> InvoiceRows { get; } = new();

    public MemberInvoiceListWindow(object? currentUser, AuthService authService, InvoiceService invoiceService)
    {
        InitializeComponent();
        DataContext = this;

        _currentUser = currentUser;
        _authService = authService;
        _invoiceService = invoiceService;

        _selectedYear = DateTime.Now.Year;

        InitializeUserHeader();
        InitializeFilters();

        Loaded += MemberInvoiceListWindow_Loaded;
    }

    private async void MemberInvoiceListWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadInvoicesAsync(resetPage: true);
    }

    private void InitializeUserHeader()
    {
        var name = ReadProperty(_currentUser, "Name") ?? "会員ユーザー";
        var email = ReadProperty(_currentUser, "Email") ?? "メールアドレス未設定";

        WelcomeText.Text = $"ようこそ、{name}さん";
        SubWelcomeText.Text = $"{email} の請求書を表示しています";
    }

    private void InitializeFilters()
    {
        var now = DateTime.Now.Year;

        YearComboBox.Items.Clear();
        for (int year = now + 1; year >= now - 5; year--)
        {
            YearComboBox.Items.Add(year);
        }
        YearComboBox.SelectedItem = _selectedYear;

        MonthComboBox.Items.Clear();
        MonthComboBox.Items.Add(new ComboBoxItem { Content = "全て", Tag = "all" });
        for (int month = 1; month <= 12; month++)
        {
            MonthComboBox.Items.Add(new ComboBoxItem
            {
                Content = $"{month}月",
                Tag = month.ToString()
            });
        }
        MonthComboBox.SelectedIndex = 0;

        StatusComboBox.Items.Clear();
        StatusComboBox.Items.Add(new ComboBoxItem { Content = "全て", Tag = "all" });
        StatusComboBox.Items.Add(new ComboBoxItem { Content = "未入金", Tag = "unpaid" });
        StatusComboBox.Items.Add(new ComboBoxItem { Content = "一部入金", Tag = "partial" });
        StatusComboBox.Items.Add(new ComboBoxItem { Content = "入金済み", Tag = "paid" });
        StatusComboBox.SelectedIndex = 0;

        KeywordTextBox.Text = string.Empty;
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvoicesAsync(resetPage: true);
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInvoicesAsync(resetPage: false);
    }

    private async void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedYear = DateTime.Now.Year;
        _selectedMonth = "all";
        _selectedStatus = "all";
        _keyword = string.Empty;
        _currentPage = 1;

        YearComboBox.SelectedItem = _selectedYear;
        MonthComboBox.SelectedIndex = 0;
        StatusComboBox.SelectedIndex = 0;
        KeywordTextBox.Text = string.Empty;

        await LoadInvoicesAsync(resetPage: true);
    }

    private async void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage <= 1)
            return;

        _currentPage--;
        await LoadInvoicesAsync(resetPage: false);
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage >= _totalPages)
            return;

        _currentPage++;
        await LoadInvoicesAsync(resetPage: false);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var dashboard = new MemberDashboardWindow(_currentUser, _authService, _invoiceService);
        dashboard.Show();
        Close();
    }

    private void DetailButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not MemberInvoiceRowViewModel row)
            return;

        var detailWindow = new MemberInvoiceDetailWindow(
            row.Id,
            _currentUser,
            _authService,
            _invoiceService);

        detailWindow.Show();
        Close();
    }

    private void PdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not MemberInvoiceRowViewModel row)
            return;

        if (string.IsNullOrWhiteSpace(row.PdfUrl))
        {
            MessageBox.Show(
                "PDFのURLまたはパスがまだ設定されていません。\n\n" +
                "DTOに PdfUrl または PdfPath を追加すると、このボタンから直接開けます。",
                "PDF表示",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = row.PdfUrl,
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

    private async Task LoadInvoicesAsync(bool resetPage)
    {
        try
        {
            ToggleLoading(true);

            ReadFilters();

            if (resetPage)
            {
                _currentPage = 1;
            }

            var result = await _invoiceService.GetMemberInvoicesAsync(
                _selectedYear,
                _selectedMonth,
                _selectedStatus,
                _keyword,
                _currentPage,
                DefaultPageSize);

            BindResult(result);
        }
        catch (Exception ex)
        {
            InvoiceRows.Clear();
            SummaryText.Text = "0 件";
            PageInfoText.Text = "1 / 1";
            OverdueBanner.Visibility = Visibility.Collapsed;

            MessageBox.Show(
                $"請求書一覧の取得に失敗しました。\n\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private void ReadFilters()
    {
        if (YearComboBox.SelectedItem is int year)
        {
            _selectedYear = year;
        }

        _selectedMonth = GetSelectedTag(MonthComboBox, "all");
        _selectedStatus = GetSelectedTag(StatusComboBox, "all");
        _keyword = KeywordTextBox.Text?.Trim() ?? string.Empty;
    }

    private void BindResult(AccountInvoiceListDto? result)
    {
        InvoiceRows.Clear();

        if (result == null)
        {
            SummaryText.Text = "0 件";
            PageInfoText.Text = "1 / 1";
            OverdueBanner.Visibility = Visibility.Collapsed;
            return;
        }

        if (result.AvailableYears is { Count: > 0 })
        {
            var currentSelection = _selectedYear;

            YearComboBox.Items.Clear();
            foreach (var y in result.AvailableYears.OrderByDescending(x => x))
            {
                YearComboBox.Items.Add(y);
            }

            if (YearComboBox.Items.Contains(currentSelection))
            {
                YearComboBox.SelectedItem = currentSelection;
            }
            else
            {
                YearComboBox.SelectedIndex = 0;
            }
        }

        if (result.Items != null)
        {
            foreach (var item in result.Items)
            {
                InvoiceRows.Add(MemberInvoiceRowViewModel.FromDto(item));
            }
        }

        var totalCount = result.TotalCount;
        _totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)Math.Max(1, result.PageSize)));
        _currentPage = Math.Max(1, result.Page);

        SummaryText.Text = $"{totalCount:N0} 件";
        PageInfoText.Text = $"{_currentPage} / {_totalPages}";
        OverdueBanner.Visibility = InvoiceRows.Any(x => x.IsOverdue && !x.IsPaid) ? Visibility.Visible : Visibility.Collapsed;
        NextButton.IsEnabled = _currentPage < _totalPages;
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingText.Text = isLoading ? "読み込み中..." : string.Empty;
        LoadingText.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

        SearchButton.IsEnabled = !isLoading;
        ClearButton.IsEnabled = !isLoading;
        YearComboBox.IsEnabled = !isLoading;
        MonthComboBox.IsEnabled = !isLoading;
        StatusComboBox.IsEnabled = !isLoading;
        KeywordTextBox.IsEnabled = !isLoading;
        InvoicesDataGrid.IsEnabled = !isLoading;
    }

    private static string GetSelectedTag(ComboBox comboBox, string fallback)
    {
        if (comboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Tag?.ToString() ?? fallback;
        }

        return fallback;
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

public class MemberInvoiceRowViewModel
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime DueAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public string? PdfUrl { get; set; }

    public string IssuedAtText => FormatDate(IssuedAt);
    public string DueAtText => FormatDate(DueAt);
    public string TotalAmountText => $"{TotalAmount:N0} 円";

    public bool IsPaid =>
        !string.IsNullOrWhiteSpace(StatusName) &&
        StatusName.Contains("入金済");

    public Visibility OverdueBadgeVisibility =>
        IsOverdue && !IsPaid ? Visibility.Visible : Visibility.Collapsed;

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

    public static MemberInvoiceRowViewModel FromDto(AccountInvoiceListItemDto dto)
    {
        return new MemberInvoiceRowViewModel
        {
            Id = dto.Id,
            InvoiceNumber = dto.InvoiceNumber ?? string.Empty,
            IssuedAt = dto.IssuedAt,
            DueAt = dto.DueAt,
            TotalAmount = dto.TotalAmount,
            StatusName = dto.StatusName ?? string.Empty,
            IsOverdue = dto.IsOverdue,
            PdfUrl = TryReadPdfUrl(dto)
        };
    }

    private static string FormatDate(DateTime value)
    {
        if (value == default)
            return string.Empty;

        return value.ToString("yyyy/MM/dd");
    }

    private static string? TryReadPdfUrl(AccountInvoiceListItemDto dto)
    {
        var type = dto.GetType();

        var pdfUrlProp = type.GetProperty("PdfUrl");
        var pdfPathProp = type.GetProperty("PdfPath");

        var pdfUrl = pdfUrlProp?.GetValue(dto)?.ToString();
        if (!string.IsNullOrWhiteSpace(pdfUrl))
            return pdfUrl;

        var pdfPath = pdfPathProp?.GetValue(dto)?.ToString();
        return string.IsNullOrWhiteSpace(pdfPath) ? null : pdfPath;
    }
}
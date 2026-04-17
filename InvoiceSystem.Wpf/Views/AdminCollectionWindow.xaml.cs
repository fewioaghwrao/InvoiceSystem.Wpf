using InvoiceSystem.Wpf.Services;
using InvoiceSystem.Wpf.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace InvoiceSystem.Wpf.Views;

public partial class AdminCollectionWindow : Window
{
    private readonly AdminCollectionViewModel _viewModel;

    public AdminCollectionWindow(InvoiceService invoiceService, long invoiceId)
    {
        InitializeComponent();

        _viewModel = new AdminCollectionViewModel(invoiceService, invoiceId);
        DataContext = _viewModel;

        Loaded += AdminCollectionWindow_Loaded;
    }

    private async void AdminCollectionWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadSafelyAsync();
    }

    private async void ReloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadSafelyAsync();
    }

    private void CopyBodyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.BodyPreview))
        {
            MessageBox.Show(
                "コピー対象の本文がありません。",
                "本文コピー",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            Clipboard.SetText(_viewModel.BodyPreview);

            MessageBox.Show(
                "本文をコピーしました。",
                "本文コピー",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"本文のコピーに失敗しました。{Environment.NewLine}{ex.Message}",
                "コピーエラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void RecordButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.RemainingAmount <= 0)
        {
            MessageBox.Show(
                "未回収残額が0円のため、催促記録は不要です。",
                "催促記録",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var message =
            "催促を実施済みとして記録します。よろしいですか？" +
            Environment.NewLine + Environment.NewLine +
            $"請求書：{_viewModel.InvoiceNumber}" + Environment.NewLine +
            $"チャネル：{_viewModel.SelectedChannelLabel}" + Environment.NewLine +
            $"トーン：{_viewModel.SelectedToneLabel}" + Environment.NewLine +
            $"次回アクション日：{_viewModel.NextActionDateTextForDialog}" + Environment.NewLine +
            $"未回収残額：{_viewModel.RemainingText}";

        var result = MessageBox.Show(
            message,
            "催促記録確認",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.OK)
        {
            return;
        }

        var success = await _viewModel.RecordAsync();
        if (!success)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(_viewModel.ErrorMessage)
                    ? "催促履歴の登録に失敗しました。"
                    : _viewModel.ErrorMessage,
                "催促記録エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(
            "催促履歴に記録しました。",
            "催促記録",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async Task LoadSafelyAsync()
    {
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"催促画面の読み込みに失敗しました。{Environment.NewLine}{ex.Message}",
                "読込エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

using InvoiceSystem.Wpf.Models;
using InvoiceSystem.Wpf.Services;
using InvoiceSystem.Wpf.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace InvoiceSystem.Wpf.Views;

public partial class PaymentDetailWindow : Window
{
    private readonly PaymentDetailViewModel _viewModel;

    public PaymentDetailWindow(
        long paymentId,
        PaymentService paymentService,
        InvoiceService invoiceService)
    {
        InitializeComponent();

        _viewModel = new PaymentDetailViewModel(paymentId, paymentService, invoiceService);
        DataContext = _viewModel;

        Loaded += PaymentDetailWindow_Loaded;
    }

    private async void PaymentDetailWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private async void ReloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AddRowButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AddRow();
    }

    private void RemoveRowButton_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is PaymentAllocationEditRow row)
        {
            _viewModel.RemoveRow(row);
        }
    }

    private void ClearInvoiceButton_OnClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is PaymentAllocationEditRow row)
        {
            _viewModel.ClearRowInvoice(row);
        }
    }

    private async void SearchCandidatesButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.SearchCandidatesAsync();
    }

    private void ApplyCandidateButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedAllocationRow is null)
        {
            MessageBox.Show(
                "先に左側の割当行を選択してください。",
                "選択確認",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (_viewModel.SelectedCandidate is null)
        {
            MessageBox.Show(
                "右側の候補一覧から請求書を選択してください。",
                "選択確認",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        _viewModel.ApplyCandidateToSelectedRow();
    }

    private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var validation = _viewModel.Validate();
            if (!string.IsNullOrWhiteSpace(validation))
            {
                MessageBox.Show(
                    validation,
                    "入力エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var ok = MessageBox.Show(
                "割当内容を保存します。よろしいですか？",
                "割当保存確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (ok != MessageBoxResult.Yes)
                return;

            await _viewModel.SaveAsync();

            MessageBox.Show(
                "保存しました。",
                "完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "保存エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
using System.Windows;
using InvoiceSystem.Wpf.Services;
using InvoiceSystem.Wpf.ViewModels;

namespace InvoiceSystem.Wpf.Views;

public partial class MemberDetailWindow : Window
{
    private readonly MemberDetailViewModel _viewModel;

    public MemberDetailWindow(MemberService memberService, int memberId)
    {
        InitializeComponent();

        _viewModel = new MemberDetailViewModel(memberService, memberId, this);
        DataContext = _viewModel;

        Loaded += MemberDetailWindow_Loaded;
    }

    private async void MemberDetailWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}

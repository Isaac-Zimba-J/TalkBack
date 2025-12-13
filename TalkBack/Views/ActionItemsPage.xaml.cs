using TalkBack.ViewModels;

namespace TalkBack.Views;

public partial class ActionItemsPage : ContentPage
{
    private readonly ActionItemsPageViewModel _viewModel;

    public ActionItemsPage(ActionItemsPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadActionItemsAsync();
    }
}

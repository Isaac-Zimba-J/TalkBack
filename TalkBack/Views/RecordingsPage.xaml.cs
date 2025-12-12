using TalkBack.ViewModels;

namespace TalkBack.Views;

public partial class RecordingsPage : ContentPage
{
	private readonly RecordingsPageViewModel _viewModel;
	public RecordingsPage(RecordingsPageViewModel viewModel)
	{

		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;

	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadRecordingsAsync();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.OnDisappearing();
	}
}
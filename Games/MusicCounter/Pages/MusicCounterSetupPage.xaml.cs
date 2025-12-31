namespace fApp.Games.MusicCounter;

public partial class MusicCounterSetupPage : ContentPage
{
    public MusicCounterSetupPage(MusicCounterSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (MusicCounterSetupViewModel)BindingContext;
        vm.LoadCommand.Execute(null);
    }
}

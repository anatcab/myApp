namespace fApp;

public partial class GamesListPage : ContentPage
{
    public GamesListPage(GamesListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

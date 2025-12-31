namespace fApp;

public partial class App : Application
{
    public App(GamesListPage gamesListPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(gamesListPage);
    }
}

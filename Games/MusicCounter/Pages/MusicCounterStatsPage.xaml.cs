namespace fApp.Games.MusicCounter;

public partial class MusicCounterStatsPage : ContentPage
{
    public MusicCounterStatsPage(MusicCounterStatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

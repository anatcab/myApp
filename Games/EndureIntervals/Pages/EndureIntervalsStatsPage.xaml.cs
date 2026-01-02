namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsStatsPage : ContentPage
{
    public EndureIntervalsStatsPage(EndureIntervalsStatsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

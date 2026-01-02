using fApp.Games.EndureIntervals;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsGamePage : ContentPage
{
    public EndureIntervalsGamePage(EndureIntervalsGameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

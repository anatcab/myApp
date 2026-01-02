using fApp.Games.EndureIntervals;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsSetupPage : ContentPage
{
    public EndureIntervalsSetupPage(EndureIntervalsSetupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

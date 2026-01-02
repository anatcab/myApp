namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsGamePage : ContentPage
{
    public EndureIntervalsGamePage(EndureIntervalsGameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        SizeChanged += (_, __) =>
        {
            if (BindingContext is not EndureIntervalsGameViewModel m) return;

            var paddingTopBottom = 18.0 * 2.0;
            var rowSpacing = 14.0 * 2.0;

            var headerApprox = 120.0 + 12.0;
            var bottomButtonApprox = 86.0;

            var available = Height - paddingTopBottom - rowSpacing - headerApprox - bottomButtonApprox;
            if (available <= 0) return;

            var perCircle = available / (m.IsTimer2Visible ? 2.0 : 1.0);
            var circle = Math.Max(180.0, Math.Min(320.0, perCircle - 16.0));

            m.TimerCircleSize = circle;
        };
    }
}

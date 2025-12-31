namespace fApp.Games.MusicCounter;

public partial class MusicCounterGamePage : ContentPage
{
    public MusicCounterGamePage(MusicCounterGameViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

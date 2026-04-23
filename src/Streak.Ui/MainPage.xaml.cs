namespace Streak.Ui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        StartupTiming.Mark("main-page-constructor-start");

        InitializeComponent();

        StartupTiming.Mark("main-page-constructor-completed");
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace UnoReng;

public sealed partial class MainPage : Page
{
    private readonly MainViewModel ViewModel = new();
    public MainPage()
    {
        DataContext = ViewModel;
        this.InitializeComponent();

//#if WINDOWS10_0_17763_0_OR_GREATER
//        ElevatedView2.Background = new SolidColorBrush();
//#endif

        ImageControl.Loaded += async (sender, e) =>
        {
            await ViewModel.GetColors(ImageControl);
        };
    }
}

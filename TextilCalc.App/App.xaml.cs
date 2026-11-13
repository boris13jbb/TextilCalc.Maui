using Microsoft.Extensions.DependencyInjection;

namespace TextilCalc.App;

public partial class App : Application
{
    private readonly AppShell _appShell;

    public App(IServiceProvider services)
    {
        InitializeComponent();

        // Las páginas usan recursos globales; deben resolverse después de cargar App.xaml.
        _appShell = services.GetRequiredService<AppShell>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_appShell)
        {
            Title = "TextilCalc"
        };

        // MinimumWidth/Height solo existen de forma confiable en escritorio.
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            window.MinimumWidth = 390;
            window.MinimumHeight = 650;
        }

        return window;
    }
}
using CommunityToolkit.Mvvm.Input;

namespace TextilCalc.App.ViewModels;

public sealed partial class HomeViewModel
{
    [RelayCommand]
    private static Task OpenGramaturaAsync() => Shell.Current.GoToAsync(nameof(Views.GramaturaPage));

    [RelayCommand]
    private static Task OpenMetrajeAsync() => Shell.Current.GoToAsync(nameof(Views.MetrajePage));

    [RelayCommand]
    private static Task OpenTelasAsync() => Shell.Current.GoToAsync(nameof(Views.TelasPage));

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

    [RelayCommand]
    private static void Exit()
    {
        var application = Application.Current;
        var window = application?.Windows.FirstOrDefault();
        if (application is not null && window is not null)
        {
            application.CloseWindow(window);
        }
    }
}

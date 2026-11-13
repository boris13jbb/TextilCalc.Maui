using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Services;

namespace TextilCalc.App.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    private readonly IUserDialogService _dialogs;

    public HomeViewModel(IUserDialogService dialogs)
    {
        _dialogs = dialogs;
        AppVersion = AppInfo.Current.VersionString;
        RefreshThemeLabel();
    }

    /// <summary>Versión real de la app (ApplicationDisplayVersion / empaquetado).</summary>
    public string AppVersion { get; }

    /// <summary>Etiqueta del botón de tema según el tema efectivo actual.</summary>
    [ObservableProperty]
    public partial string ThemeToggleLabel { get; set; } = "Tema";

    [RelayCommand]
    private static Task OpenGramaturaAsync() => Shell.Current.GoToAsync(nameof(Views.GramaturaPage));

    [RelayCommand]
    private static Task OpenMetrajeAsync() => Shell.Current.GoToAsync(nameof(Views.MetrajePage));

    [RelayCommand]
    private static Task OpenTelasAsync() => Shell.Current.GoToAsync(nameof(Views.TelasPage));

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));

    /// <summary>Alterna claro/oscuro de forma real mediante UserAppTheme.</summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var effective = app.RequestedTheme;
        app.UserAppTheme = effective == AppTheme.Dark
            ? AppTheme.Light
            : AppTheme.Dark;

        RefreshThemeLabel();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            "Salir de TextilCalc",
            "¿Deseas cerrar la aplicación?",
            "Salir",
            "Cancelar");
        if (!confirmed)
        {
            return;
        }

        var application = Application.Current;
        var window = application?.Windows.FirstOrDefault();
        if (application is not null && window is not null)
        {
            application.CloseWindow(window);
        }
    }

    private void RefreshThemeLabel()
    {
        var effective = Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
        ThemeToggleLabel = effective == AppTheme.Dark ? "Claro" : "Oscuro";
    }
}

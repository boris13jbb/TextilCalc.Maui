using System.Windows.Input;

namespace TextilCalc.App.Behaviors;

/// <summary>
/// Ejecuta un comando al pulsar Enter en un Entry.
/// En Windows, Entry.Completed no siempre se dispara; se captura KeyDown del TextBox nativo.
/// </summary>
public sealed class EnterKeyCommandBehavior : Behavior<Entry>
{
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EnterKeyCommandBehavior));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EnterKeyCommandBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private Entry? _entry;
#if WINDOWS
    private Microsoft.UI.Xaml.Controls.TextBox? _textBox;
#endif

    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        _entry = bindable;
        bindable.Completed += OnCompleted;
        bindable.HandlerChanged += OnHandlerChanged;
        bindable.Loaded += OnLoaded;
        bindable.BindingContextChanged += OnBindingContextChanged;
        BindingContext = bindable.BindingContext;
        AttachPlatformHandlers(bindable);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        DetachPlatformHandlers();
        bindable.Completed -= OnCompleted;
        bindable.HandlerChanged -= OnHandlerChanged;
        bindable.Loaded -= OnLoaded;
        bindable.BindingContextChanged -= OnBindingContextChanged;
        _entry = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (_entry is not null)
        {
            BindingContext = _entry.BindingContext;
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_entry is not null)
        {
            AttachPlatformHandlers(_entry);
        }
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        if (_entry is null)
        {
            return;
        }

        DetachPlatformHandlers();
        AttachPlatformHandlers(_entry);
    }

    private void OnCompleted(object? sender, EventArgs e) => ExecuteCommand();

    private void ExecuteCommand()
    {
        // Asegura que el texto actual quede reflejado en el binding antes de calcular.
        _entry?.Unfocus();

        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void AttachPlatformHandlers(Entry entry)
    {
#if WINDOWS
        if (entry.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            return;
        }

        if (ReferenceEquals(_textBox, textBox))
        {
            return;
        }

        DetachPlatformHandlers();
        _textBox = textBox;
        _textBox.KeyDown += OnWindowsKeyDown;
#endif
    }

    private void DetachPlatformHandlers()
    {
#if WINDOWS
        if (_textBox is not null)
        {
            _textBox.KeyDown -= OnWindowsKeyDown;
            _textBox = null;
        }
#endif
    }

#if WINDOWS
    private void OnWindowsKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        e.Handled = true;

        // Sincroniza el texto del TextBox nativo con el Entry de MAUI antes de calcular.
        if (_entry is not null && sender is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            _entry.Text = textBox.Text;
        }

        MainThread.BeginInvokeOnMainThread(ExecuteCommand);
    }
#endif
}

using Avalonia;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr.Error;

public class ErrorDialogViewModel {
    public string Message { get; private set; }

    public string StackTrace { get; private set; }

    public ICommand OnCopy { get; }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ErrorDialogViewModel(string message, string stackTrace) {
        Message = message;
        StackTrace = stackTrace;

        OnCopy = ReactiveCommand.CreateFromTask<string>(Copy);

        Close = ReactiveCommand.Create(() => Unit.Default);
    }

    public ErrorDialogViewModel(Exception error) : this(error.Message, error.StackTrace ?? "") {

    }

    private async Task Copy(string message) {
        if (Application.Current is null || Application.Current.Clipboard is null)
            return;

        await Application.Current.Clipboard.SetTextAsync(message);
    }
}

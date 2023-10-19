using Avalonia;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace Zephyr.Error;

public class ErrorDialogViewModel {
    public string Message { get; private set; }

    public string StackTrace { get; private set; }

    public ICommand OnCopy { get; }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public string ErrorMessage { get; set; } = "The operation failed.  Please try it again.";

    public ErrorDialogViewModel(string message, string stackTrace) {
        Message = message;
        StackTrace = stackTrace;
        
        OnCopy = ReactiveCommand.CreateFromTask<string>(Copy);

        Close = ReactiveCommand.Create(() => Unit.Default);

        ErrorMapItem[] errorMap = new ErrorMapItem[] {
            new() {
                Regex = new("navigating to \".+\", waiting until \"load\""),
                Message =
                    "Connection to the iTime page failed.  Please check your internet connection and try again.",
            }
        };

        foreach (var error in errorMap) {
            if (error.Regex.Match(Message).Success) {
                ErrorMessage = error.Message;
                break;
            }
        }
    }

    public ErrorDialogViewModel(Exception error) : this(error.Message, error.StackTrace ?? "") {

    }

    private async Task Copy(string message) {
        if (Application.Current is null || Application.Current.Clipboard is null)
            return;

        await Application.Current.Clipboard.SetTextAsync(message);
    }

    private struct ErrorMapItem {
        public Regex Regex { get; set; }

        public string Message { get; set; }
    }
}

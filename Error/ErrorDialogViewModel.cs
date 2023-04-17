using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;

namespace Zephyr.Error;

public class ErrorDialogViewModel {
    private readonly string _message;
    private readonly string _stackTrace;

    public ICommand Email { get; }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ErrorDialogViewModel(string message, string stackTrace) {
        _message = message;
        _stackTrace = stackTrace;

        Email = ReactiveCommand.Create(EmailDetails);

        Close = ReactiveCommand.Create(() => Unit.Default);
    }

    public ErrorDialogViewModel(Exception error) : this(error.Message, error.StackTrace ?? "") {

    }

    private void EmailDetails() {
        // this is a new line
        const string N = "%0D%0A";

        const string BASE_EMAIL =
            "mailto:mathew.cooper@dbbroadcast.co.uk" +
            "?subject=Zephyr&body=" +
            $"Hi,{N}{N}An error has occured with Zephyr.  The actions I took to produce this error is the follwing:{N}" +
            $"<Put your actions here>{N}{N}";

        var email = BASE_EMAIL + $"ErrorMessage:{N}{_message}{N}{N}StackTrace:{N}{_stackTrace}";

        Process.Start(new ProcessStartInfo(email) { UseShellExecute = true } );
    }
}

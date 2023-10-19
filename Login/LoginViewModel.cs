using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr.Login;

public class LoginViewModel : ReactiveObject, IRoutableViewModel {
    private readonly Model _model;
    private readonly Action _triggerSave;

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = "LOGIN";

    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "???";

    public string Username { get { return _model.Username; } set { _model.Username = value; } }

    public string Password { get { return _model.Password; } set { _model.Password = value; } }

    [Reactive]
    public string ErrorMessage { get; set; } = string.Empty;

    public ICommand OnLogin { get; }

    public LoginViewModel(IScreen screen, Model model, Action triggerSave) {
        HostScreen = screen;
        _model = model;
        _triggerSave = triggerSave;

        OnLogin = ReactiveCommand.CreateFromTask(Login);
    }

    private async Task Login() {
        await _model.RunTask(
            async (page) => {
                HostScreen.Router.Navigate.Execute(new Main.MainViewModel(HostScreen, _model, _triggerSave));

                await Task.CompletedTask;

                _triggerSave();
            },
            (error) => {
                if (error is not Model.LoginException) {
                    ErrorMessage = "Login errored.";
                    return false;
                }

                ErrorMessage = error.Message;

                return true;
            }
        );
    }
}


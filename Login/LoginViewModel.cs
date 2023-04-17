using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr.Login;

public class LoginViewModel : ReactiveObject, IRoutableViewModel {
    private readonly Model _model;

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = "LOGIN";

    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "???";

    public string Username { get { return _model.Username; } set { _model.Username = value; } }

    public string Password { get { return _model.Password; } set { _model.Password = value; } }

    [Reactive]
    public string ErrorMessage { get; set; } = string.Empty;

    public ICommand OnLogin { get; }

    public LoginViewModel(IScreen screen, Model model) {
        HostScreen = screen;
        _model = model;

        OnLogin = ReactiveCommand.CreateFromTask(Login);
    }

    private async Task Login() {
        await _model.RunTask(
            async (page) => {
                HostScreen.Router.Navigate.Execute(new Main.MainViewModel(HostScreen, _model));

                await Task.CompletedTask;
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


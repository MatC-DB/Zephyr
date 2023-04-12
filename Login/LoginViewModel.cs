using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr.Login;

public class LoginViewModel : ReactiveObject, IRoutableViewModel {
    private readonly MainWindowViewModel _mainWindowViewModel;

    public IScreen HostScreen { get { return _mainWindowViewModel; } }

    public string UrlPathSegment { get; } = "LOGIN";

    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "???";

    public string Username { get { return _mainWindowViewModel.Username; } set { _mainWindowViewModel.Username = value; } }

    public string Password { get { return _mainWindowViewModel.Password; } set { _mainWindowViewModel.Password = value; } }

    [Reactive]
    public string ErrorMessage { get; set; } = string.Empty;

    public ICommand OnLogin { get; }

    public LoginViewModel(MainWindowViewModel screen) {
        _mainWindowViewModel = screen;

        OnLogin = ReactiveCommand.CreateFromTask(Login);
    }

    private async Task Login() {
        await _mainWindowViewModel.RunTask(
            async (page) => {
                _mainWindowViewModel.Router.Navigate.Execute(new Main.MainViewModel(_mainWindowViewModel));

                await Task.CompletedTask;
            },
            (error) => {
                if (error is not LoginModel.LoginException)
                    return false;

                ErrorMessage = error.Message;

                return true;
            }
        );
    }
}


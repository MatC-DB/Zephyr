using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr;

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
        try {
            var page = await _mainWindowViewModel.GetLockedPage();

            await LoginModel.Login(page, _mainWindowViewModel.Username, _mainWindowViewModel.Password);

            _mainWindowViewModel.Router.Navigate.Execute(new MainViewModel(_mainWindowViewModel));
        }
        catch (LoginModel.LoginException e) {
            ErrorMessage = e.Message;
        }
        finally {
            _mainWindowViewModel.ReleasePage();
        }
    }
}


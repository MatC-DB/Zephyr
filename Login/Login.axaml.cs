using Avalonia.ReactiveUI;

namespace Zephyr.Login;

public partial class Login : ReactiveUserControl<LoginViewModel> {
    public Login() {
        InitializeComponent();
    }
}
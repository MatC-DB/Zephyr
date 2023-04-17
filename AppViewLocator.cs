using ReactiveUI;
using System;

namespace Zephyr;

// For routing
internal class AppViewLocator : IViewLocator {
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) {
        return viewModel switch {
            Login.LoginViewModel context => new Login.Login { DataContext = context },
            Main.MainViewModel context => new Main.Main { DataContext = context },
            _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
        };
    }
}

using ReactiveUI;
using System;

namespace Zephyr {
    internal class AppViewLocator : IViewLocator {
        public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) {
            return viewModel switch {
                LoginViewModel context => new Login { DataContext = context },
                MainViewModel context => new Main { DataContext = context },
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };
        }
    }
}

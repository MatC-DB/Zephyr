using ReactiveUI;
using System;

namespace Zephyr
{
    internal class AppViewLocator : IViewLocator
    {
        public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
        {
            return viewModel switch
            {
                ContentViewModel context => new Content { DataContext = context },
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
            };
        }
    }
}

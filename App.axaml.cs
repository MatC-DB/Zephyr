using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Zephyr;

public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
#pragma warning disable CS8604 // Possible null reference argument.
        var suspension = new AutoSuspendHelper(ApplicationLifetime);
#pragma warning restore CS8604 // Possible null reference argument.
        RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new AkavacheSuspensionDriver());
        suspension.OnFrameworkInitializationCompleted();

        var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();
        new MainWindow(state).Show();

        base.OnFrameworkInitializationCompleted();
    }
}
using Avalonia;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using System;
using System.Threading.Tasks;
using Zephyr.Error;
using ReactiveMarbles.ObservableEvents;

namespace Zephyr;

class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
         => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseReactiveUI()
            .UsePlatformDetect()
            .LogToTrace()
            .WithIcons(container => container.Register<MaterialDesignIconProvider>());
}

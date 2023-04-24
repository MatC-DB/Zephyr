using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Playwright;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Zephyr.AddJob;
using Zephyr.Job;
using Zephyr.Settings;

namespace Zephyr.Main;

public partial class Main : ReactiveUserControl<MainViewModel> {
    public Main() {
        InitializeComponent();

        this.Events().LayoutUpdated.Subscribe(e => {
            ViewModel!.IsViewportSmall = Bounds.Width < 400;
        });

        var now = DateTime.Now;
        var offset = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);

        this.WhenActivated(d => {
            d(ViewModel!.ShowAddJobDialog.RegisterHandler(DoShowDialogAsync));

            d(ViewModel!.ShowSettingsDialog.RegisterHandler(DoShowSettingsDialogAsync));

            // make sure to fire if before the offset
            if (now < offset)
                Task.Run(ViewModel!.GetStatus);
        });

        Observable.Timer(new DateTimeOffset(offset), TimeSpan.FromDays(1)).Subscribe(
            e => Dispatcher.UIThread.Post(() => Task.Run(ViewModel!.GetStatus), DispatcherPriority.Background));
    }

    private async Task DoShowDialogAsync(InteractionContext<AddJobDialogViewModel, JobControlViewModel?> interaction) {
        JobControlViewModel? result = null;

        var window = (Window) this.GetVisualRoot();

        if (window is not null) {
            AddJobDialog dialog = new() {
                DataContext = interaction.Input
            };

            result = await dialog.ShowDialog<JobControlViewModel?>(window);
        }

        interaction.SetOutput(result);
    }

    private async Task DoShowSettingsDialogAsync(InteractionContext<SettingsViewModel, SettingsModel.Settings> interaction) {
        var window = (Window)this.GetVisualRoot();

        if (window is not null) {
            SettingsDialog dialog = new() {
                DataContext = interaction.Input
            };

            interaction.SetOutput(await dialog.ShowDialog<SettingsModel.Settings>(window));
        }
    }
}

public static class MainBindings {
    public static Model.Clocking ClockIn = Model.Clocking.In;
    public static Model.Clocking ClockOut = Model.Clocking.Out;

    public static Model.WorkAreas Office = Model.WorkAreas.Office;
    public static Model.WorkAreas Wfm = Model.WorkAreas.Wfm;

}
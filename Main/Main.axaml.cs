using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Zephyr.AddJob;
using Zephyr.Interface;
using Zephyr.Job;

namespace Zephyr.Main;

public partial class Main : ReactiveUserControl<MainViewModel> {
    public Main() {
        InitializeComponent();

        var now = DateTime.Now;
        var offset = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);

        this.WhenActivated(d => {
            d(ViewModel!.ShowAddJobDialog.RegisterHandler(DoShowDialogAsync));

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
}

public static class MainBindings {
    public const Model.Clocking ClockIn = Model.Clocking.In;
    public const Model.Clocking ClockOut = Model.Clocking.Out;

    public const Model.WorkAreas Office = Model.WorkAreas.Office;
    public const Model.WorkAreas Wfm = Model.WorkAreas.Wfm;

}
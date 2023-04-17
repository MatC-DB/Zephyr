using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Zephyr.AddJob;
using Zephyr.Job;

namespace Zephyr.Main;

public partial class Main : ReactiveUserControl<MainViewModel> {
    public Main() {
        InitializeComponent();

        this.Events().LayoutUpdated.Subscribe(e => {
            ViewModel!.IsViewportSmall = Bounds.Width < 400;
        });

        var now = DateTime.Now;
        var offset = new DateTime(now.Year, now.Month, now.Day, 4, 0, 0);

        // make sure to fire if before the offset
        this.WhenActivated(d => {
            d(ViewModel!.ShowAddJobDialog.RegisterHandler(DoShowDialogAsync));

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
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using System;
using Zephyr.Error;
using Avalonia.Threading;
using System.Reactive;
using System.Reactive.Concurrency;
using Avalonia.Controls;

namespace Zephyr;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel): this() {
        Position = viewModel.Position;

        ViewModel = viewModel;

        RxApp.MainThreadScheduler.Schedule(async () => await ViewModel.OnLoad());

        this.WhenActivated(d => d(ViewModel.ErrorDialog.RegisterHandler(ShowErrorDialogAsync)));

        this.Events().PositionChanged
            .Select((e) => e.Point).InvokeCommand(this, x => x.ViewModel!.SetPosition);
    }

    private async Task ShowErrorDialogAsync(InteractionContext<Exception, Unit> interaction) {
        ErrorDialogViewModel viewModel = new(interaction.Input);

        var result = await Dispatcher.UIThread.InvokeAsync(async () => {
            ErrorDialog dialog = new() {
                DataContext = viewModel
            };

            return await dialog.ShowDialog<Unit>(this);
        });

        interaction.SetOutput(result);
    }
}

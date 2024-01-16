using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using System;
using Zephyr.Error;
using Avalonia.Threading;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Zephyr;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel): this() {
        ViewModel = viewModel;

        RxApp.MainThreadScheduler.Schedule(ViewModel.OnLoad);

        this.WhenActivated(d => d(ViewModel.ErrorDialog.RegisterHandler(ShowErrorDialogAsync)));
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

using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using Zephyr.Main.AddJob;
using Zephyr.Main.Job;

namespace Zephyr;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.ShowAddJobDialog.RegisterHandler(DoShowDialogAsync)));
    }

    public MainWindow(MainWindowViewModel viewModel) : this() {
        Position = viewModel.Position;
        DataContext = viewModel;
    }

    protected void OnPositionChanged(object sender, PixelPointEventArgs e) {
        if (WindowState == WindowState.Normal && ViewModel != null)
            ViewModel.Position = e.Point;
    }

    private async Task DoShowDialogAsync(InteractionContext<AddJobDialogViewModel, JobControlViewModel?> interaction) {
        var dialog = new AddJobDialog {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<JobControlViewModel?>(this);
        interaction.SetOutput(result);
    }
}

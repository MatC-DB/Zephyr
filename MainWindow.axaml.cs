using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Zephyr;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this() {
        Position = viewModel.Position;
        DataContext = viewModel;
    }

    protected void OnPositionChanged(object sender, PixelPointEventArgs e) {
        if (WindowState == WindowState.Normal && DataContext is MainWindowViewModel viewModel)
            viewModel.Position = e.Point;
    }
}

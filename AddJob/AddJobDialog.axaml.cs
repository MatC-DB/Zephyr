using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace Zephyr.AddJob;

public partial class AddJobDialog : ReactiveWindow<AddJobDialogViewModel> {
    public AddJobDialog() {
        InitializeComponent();

        this.WhenActivated(d => {
            d(ViewModel!.AddJob.Subscribe(Close));
            d(ViewModel!.CancelAddJob.Subscribe(Close));

            Task.Run(ViewModel!.GetJobOptions);
        });

        this.WhenAnyValue(x => x.ViewModel!.IsAtJob).Subscribe(isAtJob => {
            var textBoxName = isAtJob ? "JobSearchBox" : "SequenceSearchBox";

            this.FindControl<TextBox>(textBoxName)?.Focus();
        });
    }

    public void OnJobDoubleClick(object sender, RoutedEventArgs e) {
        ViewModel!.OnNext.Execute(Unit.Default);
    }

    public void OnSequenceDoubleClick(object sender, RoutedEventArgs e) {
        Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(async () => {
            Close(await ViewModel!.OnAddJob());
        }));
    }

    public void OnJobAttached(object? sender, VisualTreeAttachmentEventArgs e) {
        if (sender is not null && sender is TextBox textBox) {
            textBox.Focus();
        }
    }
}
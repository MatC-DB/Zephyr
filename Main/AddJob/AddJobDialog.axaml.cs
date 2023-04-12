using Avalonia.ReactiveUI;
using ReactiveUI;
using System;

namespace Zephyr.Main.AddJob;

public partial class AddJobDialog : ReactiveWindow<AddJobDialogViewModel> {
    public AddJobDialog()
    {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel!.AddJob.Subscribe(Close)));
        this.WhenActivated(d => d(ViewModel!.CancelAddJob.Subscribe(Close)));
    }
}
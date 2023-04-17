using Avalonia.ReactiveUI;
using ReactiveUI;
using System;

namespace Zephyr.Error;

public partial class ErrorDialog : ReactiveWindow<ErrorDialogViewModel>
{
    public ErrorDialog()
    {
        InitializeComponent();

        this.WhenActivated(d => d(ViewModel!.Close.Subscribe(x => Close(x))));
    }
}
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;

namespace Zephyr.Settings;

public partial class SettingsDialog : ReactiveWindow<SettingsViewModel> {
    public SettingsDialog()
    {
        InitializeComponent();

        this.WhenActivated(d => {
            d(ViewModel!.OnClose.Subscribe(x => Close(x)));
        });
    }
}
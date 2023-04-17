using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using Avalonia.Threading;

namespace Zephyr.Job;

public partial class JobControl : ReactiveUserControl<JobControlViewModel> {
    public JobControl() {
        InitializeComponent();

        this.WhenActivated(d =>
            d(this.FindAncestorOfType<Main.Main>()
                .WhenAnyValue(m => m.ViewModel!.CurrentJobSequenceValue)
                .Subscribe(currentJobSequenceValue => {
                    Dispatcher.UIThread.Post(() => {
                        ViewModel!.IsActive = currentJobSequenceValue == ViewModel!.SequenceValue;
                    });
                }))
        );
    }
}
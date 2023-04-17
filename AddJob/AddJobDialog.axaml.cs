using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace Zephyr.AddJob;

public partial class AddJobDialog : ReactiveWindow<AddJobDialogViewModel> {
    public AddJobDialog()
    {
        InitializeComponent();

        this.WhenActivated(d => {
            d(ViewModel!.AddJob.Subscribe(Close));
            d(ViewModel!.CancelAddJob.Subscribe(Close));

            Task.Run(ViewModel!.GetJobOptions);
        });
    }
}
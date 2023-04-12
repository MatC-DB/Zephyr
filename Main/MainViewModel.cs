using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zephyr.Main.AddJob;

namespace Zephyr.Main;

public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private readonly MainWindowViewModel _mainWindowViewModel;

    public IScreen HostScreen { get { return _mainWindowViewModel; } }

    public string UrlPathSegment { get; } = "CONTENT";

    [Reactive]
    public bool IsClockedIn { get; private set; } = false;

    [Reactive]
    public bool IsClockedOut { get; private set; } = false;

    public ICommand OnClockIn { get; }

    public ICommand OnClockOut { get; }

    public ICommand OnAddJob { get; }

    public MainViewModel(MainWindowViewModel screen) {
        _mainWindowViewModel = screen;

        OnClockIn = ReactiveCommand.CreateFromTask(ClockIn);
        OnClockOut = ReactiveCommand.CreateFromTask(ClockOut);

        OnAddJob = ReactiveCommand.CreateFromTask(async () => {
            await _mainWindowViewModel.RunTask(async (page) => {
                var addJobDialog = new AddJobDialogViewModel(page);

                var result = await _mainWindowViewModel.ShowAddJobDialog.Handle(addJobDialog);
            });
        });

        Task.Run(GetStatus);
    }

    private void SetStatus(bool status) {
        IsClockedIn = status;
        IsClockedOut = !status;
    }

    private async Task GetStatus() {
        await _mainWindowViewModel.RunTask(async (page) => {
            SetStatus(await MainModel.GetStatus(page));
        });
    }

    private async Task ClockIn() {
        await Clock(MainModel.Clocking.In);
    }

    private async Task ClockOut() {
        await Clock(MainModel.Clocking.Out);
    }

    private async Task Clock(MainModel.Clocking type) {
        await _mainWindowViewModel.RunTask(async (page) => {
            await MainModel.Clock(page, type);
            SetStatus(type == MainModel.Clocking.In);

            // could get status here but would require some sort of check to see if the iTime backend has updated
        });
    }
}


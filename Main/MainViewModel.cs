using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zephyr;
public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private const string STATUS_LOADING = "Loading...";
    private readonly MainWindowViewModel _mainWindowViewModel;

    public IScreen HostScreen { get { return _mainWindowViewModel; } }

    public string UrlPathSegment { get; } = "CONTENT";

    [Reactive]
    public string Status { get; set; } = STATUS_LOADING;

    public ICommand OnClockIn { get; }

    public ICommand OnClockOut { get; }

    public MainViewModel(MainWindowViewModel screen) {
        _mainWindowViewModel = screen;

        OnClockIn = ReactiveCommand.CreateFromTask(ClockIn);
        OnClockOut = ReactiveCommand.CreateFromTask(ClockOut);

        Task.Run(GetStatus);
    }

    private async Task Login(IPage page) {
        await LoginModel.Login(page, _mainWindowViewModel.Username, _mainWindowViewModel.Password);
    }

    private void SetStatus(string message) {
        Status = message;
    }

    private async Task RunTask(Func<IPage, Task> task) {
        try {
            var page = await _mainWindowViewModel.GetLockedPage();

            await Login(page);

            await task(page);
        }
        catch {
            // TODO: do something better here
        }
        finally {
            _mainWindowViewModel.ReleasePage();
        }
    }

    private async Task GetStatus() {
        await RunTask(async (page) => await MainModel.GetStatus(page, SetStatus));
    }

    private async Task ClockIn() {
        await Clock(MainModel.Clocking.In);
    }

    private async Task ClockOut() {
        await Clock(MainModel.Clocking.Out);
    }

    private async Task Clock(MainModel.Clocking type) {
        await RunTask(async (page) => await MainModel.Clock(page, SetStatus, type));
    }
}


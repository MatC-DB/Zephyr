using Microsoft.Playwright;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Collections.ObjectModel;
using Zephyr.Job;
using MethodTimer;

namespace Zephyr;

public partial class Model {
    public const string BASE_URL = "https://time.dbbroadcast.co.uk:442/";

    private readonly Func<Exception, IObservable<Unit>> _errorDialogHandler;
    private readonly Action _incrementRunningProcesses;
    private readonly Action _decrementRunningProcesses;

    // don't use any of these directly
    private IPlaywright? _playWright;
    private IBrowser? _browser;
    private IPage? _page;

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public ObservableCollection<JobControlViewModel> Jobs { get; set; } = new();

    public Model(
        Func<Exception, IObservable<Unit>> errorDialogHandler,
        Action incrementRunningProcesses,
        Action decrementRunningProcesses
    ) {
        _errorDialogHandler = errorDialogHandler;
        _incrementRunningProcesses = incrementRunningProcesses;
        _decrementRunningProcesses = decrementRunningProcesses;
    }

    ~Model() {
        _playWright?.Dispose();
        _semaphoreSlim.Dispose();
    }

    [Time]
    public async Task OnLoad() {
        _incrementRunningProcesses();

        await GetPage();

        _decrementRunningProcesses();
    }

    private async Task<IPage> GetPage() {
        _playWright ??= await Playwright.CreateAsync();

#if DEBUG && false
        _browser ??= await _playWright.Chromium.LaunchAsync(new() {
            Headless = false,
            SlowMo = 50,
        });
#else
        _browser ??= await _playWright.Chromium.LaunchAsync();
#endif

        _page ??= await _browser.NewPageAsync();

        return _page;
    }

    private async Task<IPage> GetLockedPage() {
        var page = await GetPage();

        await _semaphoreSlim.WaitAsync();

        return page;
    }

    private void ReleasePage() {
        _semaphoreSlim.Release();
    }

    public async Task RunTask(Func<IPage, Task> task, Func<Exception, bool>? tryHandleError = null) {
        try {
            _incrementRunningProcesses();

            var page = await GetLockedPage();

            await Login(page);

            await task(page);
        }
        catch (Exception e) {
            if (tryHandleError == null || !tryHandleError.Invoke(e)) {
                await _errorDialogHandler(e);
            }
        }
        finally {
            ReleasePage();

            _decrementRunningProcesses();
        }
    }
}

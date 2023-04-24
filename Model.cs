using Microsoft.Playwright;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Collections.ObjectModel;
using Zephyr.Job;
using MethodTimer;
using Zephyr.Settings;
using System.Diagnostics;
using System.Text.Json;
using ReactiveMarbles.ObservableEvents;

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

    public SettingsModel.Settings Settings { get; set; } = new() { ShowWorkAreas = true };

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
        if (_browser is not null)
            Task.Run(_browser.CloseAsync).Wait();
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

        if(_page is null) {
            _page = await _browser.NewPageAsync();

#if DEBUG
            _page.Events().Request.Subscribe(r => {
                var request = r.Url[BASE_URL.Length..];

                if (request.StartsWith("Scripts/") || request.StartsWith("Content/"))
                    return;

                if(string.IsNullOrWhiteSpace(request)) {
                    request = "{ROOT}";
                }

                Debug.WriteLine($"[ZEPHYR] Request: \"{request}\"");
            });
#endif
        }

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
#if DEBUG
                Debug.WriteLine($"[ZEPHYR] {e.Message}\n{e.StackTrace}");
#endif
                await _errorDialogHandler(e);
            }
        }
        finally {
            ReleasePage();

            _decrementRunningProcesses();
        }
    }

    internal static async Task<IResponse> GetResponse(IPage page, Func<IPage, Task> action, string path, bool useStartsWith = false) {
        var usedPath = BASE_URL + path;

        var request = await page.RunAndWaitForRequestAsync(async () => {
            await action(page);
        }, (request) => {
            if (useStartsWith)
                return request.Url.StartsWith(usedPath);

            return request.Url.Equals(usedPath);
        });

        // ensure server receives it
        if (request.Failure is not null) {
            throw new ResponseException("Request failed.");
        }

        var response = await request.ResponseAsync();

        if (response is null || !response.Ok) {
            throw new ResponseException("Response failed.");
        }

        return response;
    }

    internal static async Task<JsonElement> GetResponseJson(IPage page, Func<IPage, Task> action, string path) {
        var response = await GetResponse(page, action, path);

        var potentialJson = await response.JsonAsync();

        if (!potentialJson.HasValue) {
            throw new ResponseException("JSON failed.");
        }

        return potentialJson.Value;
    }

    public class ResponseException : Exception {
        public ResponseException(string message) : base(message) { }
    }
}

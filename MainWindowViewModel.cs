using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Zephyr {
    [DataContract]
    public partial class MainWindowViewModel : ReactiveObject, IScreen {
        public const string BASE_URL = "https://time.dbbroadcast.co.uk:442/";

        private IPlaywright? _playWright;
        private IBrowser? _browser;
        private IPage? _page;

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private readonly object _processesRunningLock = new();
        private int _processesRunning = 0;

        public RoutingState Router { get; } = new();

        [Reactive]
        public bool IsLoading { get; private set; } = false;

        [Reactive, DataMember]
        public string Username { get; set; } = string.Empty;

        [Reactive, DataMember]
        public string Password { get; set; } = string.Empty;


        public MainWindowViewModel() {
            Router.Navigate.Execute(new LoginViewModel(this));

            _ = GetBrowser();
        }

        ~MainWindowViewModel() {
            _playWright?.Dispose();
            _semaphoreSlim.Dispose();
        }

        private void IncrementProcessesRunning() {
            lock (_processesRunningLock) {
                ++_processesRunning;
                IsLoading = _processesRunning > 0;
            }
        }

        private void DecrementProcessesRunning() {
            lock (_processesRunningLock) {
                --_processesRunning;
                IsLoading = _processesRunning > 0;
            }
        }

        public async Task<IPage> GetLockedPage() {
            IncrementProcessesRunning();

            while (_page is null)
                await Task.Delay(25);

            await _semaphoreSlim.WaitAsync();

            return _page;
        }

        public void ReleasePage() {
            _semaphoreSlim.Release();

            DecrementProcessesRunning();
        }

        private async Task GetBrowser() {
            IncrementProcessesRunning();

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

            DecrementProcessesRunning();
        }
    }
}

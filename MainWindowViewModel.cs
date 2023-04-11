using System;
using System.Diagnostics;
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

        private readonly Task<IPage> _getPage;

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

            _getPage = GetPage();

            Task.Run(async () => await _getPage);
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

            var page = await _getPage;

            await _semaphoreSlim.WaitAsync();

            return page;
        }

        public void ReleasePage() {
            _semaphoreSlim.Release();

            DecrementProcessesRunning();
        }

        private async Task<IPage> GetPage() {
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

            var page = await _browser.NewPageAsync();

            Debug.WriteLine("[ZEPHYR]: Got page");

            DecrementProcessesRunning();

            return page;
        }
    }
}

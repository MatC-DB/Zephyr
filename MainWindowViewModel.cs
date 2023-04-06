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

        private Mutex _runningProcessesMutex = new();
        private int _processesRunning = 0;

        public RoutingState Router { get; } = new();

        [Reactive]
        public bool IsLoading { get; private set; } = false;

        [Reactive, DataMember]
        public string Username { get; set; } = string.Empty;

        [Reactive, DataMember]
        public string Password { get; set; } = string.Empty;

        public IBrowser? Browser { get; private set; } = null;

        public MainWindowViewModel() {
            Router.Navigate.Execute(new LoginViewModel(this));

            _ = GetBrowser();
        }

        ~MainWindowViewModel() {
            _runningProcessesMutex.Dispose();
            _playWright?.Dispose();
        }

        public void IncrementProcessesRunning() {
            _runningProcessesMutex.WaitOne();
            ++_processesRunning;
            IsLoading = _processesRunning > 0;
            _runningProcessesMutex.ReleaseMutex();
        }

        public void DecrementProcessesRunning() {
            _runningProcessesMutex.WaitOne();
            --_processesRunning;
            IsLoading = _processesRunning > 0;
            _runningProcessesMutex.ReleaseMutex();
        }

        private async Task GetBrowser() {
            IncrementProcessesRunning();

            _playWright ??= await Playwright.CreateAsync();

#if DEBUG
            Browser ??= await _playWright.Chromium.LaunchAsync(new() {
                Headless = false,
                SlowMo = 50,
            });
#else
            Browser ??= await _playWright.Chromium.LaunchAsync();
#endif

            DecrementProcessesRunning();
        }
    }
}

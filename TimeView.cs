using Microsoft.Playwright;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Zephyr {
    internal class TimeView {
        private const string BASE_URL = "https://time.dbbroadcast.co.uk:442/";

        private readonly MainWindowViewModel _viewModel;

        private int _processesRunning = 0;

        private bool _isConnecting = false;
        private bool _isConnected = false;

        private int ProcessesRunning {
            get { return _processesRunning; }
            set {
                _processesRunning = value;
                _viewModel.IsLoading = _processesRunning > 0;
            }
        }

        private IPlaywright? _playWright;
        private IBrowser? _browser;
        private IPage? _page;

        public TimeView(MainWindowViewModel viewModel) {
            _viewModel = viewModel;
        }

        ~TimeView() {
            _browser?.CloseAsync();
            _playWright?.Dispose();
        }

        public async Task Connect() {
            _isConnecting = true;
            _viewModel.ErrorMessage = string.Empty;

            try {
                ProcessesRunning++;

                _playWright ??= await Playwright.CreateAsync();

#if DEBUG
                _browser ??= await _playWright.Chromium.LaunchAsync(new() {
                    Headless = false,
                    SlowMo = 50,
                });
#else
                _browser ??= await _playWright.Chromium.LaunchAsync();
#endif

                Debug.WriteLine("[Zephyr] Got browser");
                _page ??= await _browser.NewPageAsync();
                await _page.GotoAsync(BASE_URL + "Login");
                Debug.WriteLine("[Zephyr] Got page");

                _isConnected = true;
            }
            catch {
                _viewModel.ErrorMessage = "Connection failed.";
            }
            finally {
                ProcessesRunning--;

                _isConnecting = false;
            }
        }

        public async Task WaitForConnection() {
            if (_isConnected)
                return;

            if (_isConnecting) {
                while (!_isConnected)
                    await Task.Delay(25); // lol
            }
            else {
                await Connect();
            }
        }

        public async Task<bool> TryLogin() {
            await WaitForConnection();

            _viewModel.ErrorMessage = string.Empty;

            try {
                ProcessesRunning++;

                await _page.GotoAsync(BASE_URL + "Login");

                await _page.GetByLabel("Email address or username").FillAsync(_viewModel.Username);
                await _page.GetByLabel("Password").FillAsync(_viewModel.Password);

                await _page.GetByText("Sign In").ClickAsync();

                Debug.WriteLine("[Zephyr] Signed in as: " + _viewModel.Username);

                var result = _page.Url == BASE_URL;

                if (!result)
                    _viewModel.ErrorMessage = "Incorrect login details.";

                ProcessesRunning--;
                return result;
            }
            catch {
                _viewModel.ErrorMessage = "Login failed.";

                ProcessesRunning--;
                return false;
            }
        }

        public async Task Snapshot() {
            if (_page == null)
                return;

            var dateTime = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture).Replace("-", "").Replace(":", "");

            await _page.ScreenshotAsync(new() {
                Path = "screenshot" + dateTime + ".png"
            });

            Debug.WriteLine("[Zephyr] Screenshot got");
        }

    }
}

using Microsoft.Playwright;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Zephyr
{
    internal class TimeView
    {
        private const string BASE_URL = "https://time.dbbroadcast.co.uk:442/";

        private readonly MainWindowViewModel _viewModel;

        private int _processesRunning = 0;

        private int ProcessesRunning
        {
            get { return _processesRunning; }
            set
            {
                _processesRunning = value;
                _viewModel.IsLoading = _processesRunning > 0;
            }
        }

        private IPlaywright? _playWright;
        private IBrowser? _browser;
        private IPage? _page;

        public TimeView(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        ~TimeView()
        {
            _browser?.CloseAsync();
            _playWright?.Dispose();
        }

        public async Task Connect()
        {
            _viewModel.ErrorMessage = string.Empty;

            try
            {
                ProcessesRunning++;

                _playWright ??= await Playwright.CreateAsync();

#if DEBUG
                _browser ??= await _playWright.Chromium.LaunchAsync(new()
                {
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

                _viewModel.IsConnected = true;
            }
            catch
            {
                _viewModel.ErrorMessage = "Connection failed.";
                _viewModel.IsConnected = false;
            } finally
            {
                ProcessesRunning--;
            }
        }

        public async Task<bool> TryLogin()
        {
            if (!_viewModel.IsConnected) return false;
            if (_page == null) throw new InvalidOperationException();

            _viewModel.ErrorMessage = string.Empty;

            try
            {
                await _page.GotoAsync(BASE_URL + "Login");

                await _page.GetByLabel("Email address or username").FillAsync(_viewModel.Username);
                await _page.GetByLabel("Password").FillAsync(_viewModel.Password);

                await _page.GetByText("Sign In").ClickAsync();

                Debug.WriteLine("[Zephyr] Signed in as: " + _viewModel.Username);

                var result = _page.Url == BASE_URL;

                if (!result) _viewModel.ErrorMessage = "Incorrect login details.";

                return result;
            }
            catch
            {
                _viewModel.ErrorMessage = "Login failed.";
                Debug.WriteLine("[Zephyr] Login failed");

                return false;
            }
        }

        public async Task Snapshot()
        {
            if (_page == null) return;

            var dateTime = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture).Replace("-", "").Replace(":", "");

            await _page.ScreenshotAsync(new()
            {
                Path = "screenshot" + dateTime + ".png"
            });

            Debug.WriteLine("[Zephyr] Screenshot got");
        }

    }
}

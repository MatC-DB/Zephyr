using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Zephyr
{
    [DataContract]
    public partial class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();

        [Reactive, DataMember]
        public string Username { get; set; } = string.Empty;

        [Reactive]
        public string Password { get; set; } = string.Empty;

        public ICommand OnLogin { get; }

        public MainWindowViewModel()
        {
            OnLogin = ReactiveCommand.Create(Login);
        }
        private void Login()
        {
            Debug.WriteLine(Username + "," + Password);

            Router.Navigate.Execute(new ContentViewModel(this));
        }

        private async Task Test()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://playwright.dev/dotnet");
            await page.ScreenshotAsync(new()
            {
                Path = "screenshot.png"
            });
        }
    }
}

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Zephyr
{
    [DataContract]
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();

        #region Window
        public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "DEBUG";

        [DataMember]
        public double Width { get; set; } = 800;

        [DataMember]
        public double Height { get; set; } = 450;

        public WindowState State { get; set; } = WindowState.Normal;

        [DataMember]
        public bool IsFullScreen
        {
            get => State == WindowState.Maximized;
            set
            {
                if (value && State == WindowState.Normal) State = WindowState.Maximized;
            }
        }

        public Avalonia.PixelPoint Position { get; set; } = new Avalonia.PixelPoint(50, 50);

        [DataMember]
        public int Top { get { return Position.Y; } set { Position = new Avalonia.PixelPoint(Position.X, value); } }

        [DataMember]
        public int Left { get { return Position.X; } set { Position = new Avalonia.PixelPoint(value, Position.Y); } }

        #endregion

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

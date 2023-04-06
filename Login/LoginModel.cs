using Microsoft.Playwright;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zephyr; 

internal class LoginModel {

    public static async Task<Union2<IPage, string>> Login(MainWindowViewModel viewModel) {
        while (viewModel.Browser is null)
            await Task.Delay(25);

        IPage? page = null;
        try {
            page = await viewModel.Browser.NewPageAsync();
            await page.GotoAsync(MainWindowViewModel.BASE_URL + "Login");


            await page.GetByLabel("Email address or username").FillAsync(viewModel.Username);
            await page.GetByLabel("Password").FillAsync(viewModel.Password);

            await page.GetByText("Sign In").ClickAsync();

            var result = page.Url == MainWindowViewModel.BASE_URL;

            if (result) {
                Debug.WriteLine("[Zephyr] Signed in as: " + viewModel.Username);
                viewModel.Router.Navigate.Execute(new MainViewModel(viewModel));

                return new Union2<IPage, string>(page);
            }
            else {
                await page.CloseAsync();
                return new Union2<IPage, string>("Login details are wrong.");
            }
        }
        catch {
            if (page is not null)
                await page.CloseAsync();

            return new Union2<IPage, string>("Login failed.");
        }
    }
}

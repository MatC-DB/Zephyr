using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Zephyr;

internal class LoginModel {
    public static async Task Login(IPage page, string username, string password) {
        try {
            await page.GotoAsync(MainWindowViewModel.BASE_URL + "Login");


            await page.GetByLabel("Email address or username").FillAsync(username);
            await page.GetByLabel("Password").FillAsync(password);

            await page.GetByText("Sign In").ClickAsync();
        }
        catch {
            throw new LoginException("Login failed.");
        }

        if (page.Url != MainWindowViewModel.BASE_URL) {
            throw new LoginException("Login details are wrong.");
        }
    }

    public class LoginException : Exception {
        public LoginException(string message) : base(message) { }
    }
}

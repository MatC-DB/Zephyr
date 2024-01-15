using MethodTimer;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Zephyr.Interface;

public partial class Model {
    [Time]
    public async Task Login(IPage page) {
        await page.GotoAsync(BASE_URL + "Login");

        await page.GetByLabel("Email address or username").FillAsync(Username);
        await page.GetByLabel("Password").FillAsync(Password);

        await page.GetByText("Sign In").ClickAsync();

        if (page.Url != BASE_URL) {
            throw new LoginException("Login details are wrong.");
        }
    }

    public class LoginException : Exception {
        public LoginException(string message) : base(message) { }
    }
}

using MethodTimer;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zephyr;

public partial class Model {
    public static async Task GetClockingPanel(IPage page) {
        await page.GotoAsync(BASE_URL);

        // click the header button
        await GetResponse(page, async (page) => {
            await page.EvaluateAsync(@"() => document.getElementsByName(""ClockModuleDisplay"")[0].click();");
        }, "Clocking/ClockingModule");
    }

    public enum Clocking {
        In,
        Out
    }

    [Time]
    public static async Task Clock(IPage page, Clocking type) {
        var isClockingIn = type == Clocking.In;

        await GetClockingPanel(page);

        // click the appropriate button
        var json = await GetResponseJson(page, async (page) => {
            await page.GetByText(isClockingIn ? "Clock In" : "Clock Out").ClickAsync();
        }, "Clocking/ClockAction");

        var responseValue = json.GetProperty("WorkingTasksResponse").GetString();

        if (responseValue is null || !responseValue.EndsWith("successfully")) {
#if DEBUG
            Debug.WriteLine($"[ZEPHYR] Json at error: {json}");
#endif

            throw new ResponseException("Response value failed");
        }
    }

    [Time]
    public static async Task EnsureClockedIn(IPage page) {
        if (!await GetClockingStatus(page)) {
            await Clock(page, Clocking.In);

            // even though the response still comes back good
            // we need to make sure the backend actually catches us
            for (int retry = 0; retry < 20; ++retry) { 
                await Task.Delay(25);

                if (await GetClockingStatus(page))
                    break;
            }
        }
    }
}

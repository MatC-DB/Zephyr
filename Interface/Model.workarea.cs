using MethodTimer;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zephyr.Interface;

public partial class Model {
    public enum WorkAreas {
        Office,
        Wfm
    }

    [Time]
    public static async Task SetWorkArea(IPage page, WorkAreas type) {
        var isInOffice = type == WorkAreas.Office;

        await GetClockingPanel(page);

        await page.Locator("#ActivityChange").SelectOptionAsync(
            new SelectOptionValue() { Label = isInOffice ? "Normal Working Hours" : "Working From Home" });

        var json = await GetResponseJson(page, async (page) => {
            await page.GetByText("Change Activity").ClickAsync();
        }, "Clocking/TaskChange");

        var responseValue = json.GetProperty("WorkingTasksResponse").GetString();

        if (responseValue is null || !responseValue.EndsWith("successfully")) {
#if DEBUG
            Debug.WriteLine($"[ZEPHYR] Json at error: {json}");
#endif

            throw new ResponseException("Response value failed");
        }
    }
}


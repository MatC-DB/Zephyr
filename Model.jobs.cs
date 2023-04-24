using MethodTimer;
using Microsoft.Playwright;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zephyr;

public partial class Model {
    public static async Task OpenJobsPanel(IPage page) {
        await page.GotoAsync(BASE_URL);

        await GetResponse(page, async (page) => {
            await page.EvaluateAsync(@"() => document.getElementsByName(""JobCostingModuleDisplay"")[0].click();");
        }, "JobCosting/JobCostingModule");
    }

    // Note you have to call OpenJobsPanel first
    [Time]
    public static async Task SelectJob(IPage page, string job, string sequence) {
        await GetResponse(page, async (page) => {
            await SelectOption(page, "JobChange", job);
        }, "JobCosting/GetSequences", true);

        await SelectOption(page, "SequenceChange", sequence);

        var json = await GetResponseJson(page, async (page) => {
            await page.GetByRole(AriaRole.Button, new() { Name = "Start Job" }).ClickAsync();
        }, "JobCosting/StartJob");

        var responseValue = json.GetProperty("Message").GetString();

        if (responseValue is null || !responseValue.EndsWith("successfully")) {
#if DEBUG
            Debug.WriteLine($"[ZEPHYR] Json at error: {json}");
#endif

            throw new ResponseException("Response value failed");
        }
    }

    public static async Task SelectOption(IPage page, string id, string label) {
        await page.Locator("#" + id).SelectOptionAsync(new SelectOptionValue() { Label = label.Trim() });
    }

    public class JobException : Exception {
        public JobException(string message) : base($"Job selection {message} failed.") { }
    }
}


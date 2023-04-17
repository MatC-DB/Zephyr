using MethodTimer;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Zephyr;

public partial class Model {
    public static async Task OpenJobsPanel(IPage page) {
        await page.GotoAsync(BASE_URL);

        await page.EvaluateAsync(@"() => document.getElementsByName(""JobCostingModuleDisplay"")[0].click();");
    }

    // Note you have to call OpenJobsPanel first
    [Time]
    public static async Task SelectJob(IPage page, string job, string sequence) {
        await SelectOption(page, "JobChange", job);
        await SelectOption(page, "SequenceChange", sequence);

        await page.GetByRole(AriaRole.Button, new() { Name = "Start Job" }).ClickAsync();
    }

    public static async Task SelectOption(IPage page, string id, string label) {
        await page.Locator("#" + id).SelectOptionAsync(new SelectOptionValue() { Label = label.Trim() });
    }
}


using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Zephyr;
internal class MainModel {

    internal static async Task GetStatus(IPage page, Action<string> statusSetter) {
        await page.GotoAsync(MainWindowViewModel.BASE_URL + "Information");

        var evaluateResult = await page.EvaluateAsync(@"() => {
            const resultsChildren = document.getElementById(""Result"").children;
            const clockingRecords = resultsChildren[resultsChildren.length - 1];

            if(clockingRecords.tagName !== ""DIV"") return {Job: ""???"", IsIn: false};

            const tableRows = [...clockingRecords.getElementsByTagName(""tr"")].slice(1);
            const tableData = tableRows.map((row) => [...row.getElementsByTagName(""td"")].map((col) => col.innerText))
            const transform = tableData.map(([clock, action, job]) => ({clock, action, job}));

            const date = (""0"" + (new Date()).getDate()).slice(-2);
            const transformFiltered = transform.filter(({clock}) => clock.startsWith(date));

            const lastJobIndex = transformFiltered.findLastIndex(row => !!row.job);

            const details = {Job: transformFiltered[lastJobIndex].job, IsIn: false};

            const lastIn = transformFiltered.findLastIndex(row => row.action === ""IN"");

            if(lastIn < 0) return details;

            const lastOut = transformFiltered.findLastIndex(row => row.action === ""OUT"");

            details.IsIn = lastOut < 0 ? true : (lastIn > lastOut);
            return details;
        }");

        bool isIn = evaluateResult.Value.GetProperty("IsIn").GetBoolean();

        if (!isIn) {
            statusSetter("Clocked out");
            return;
        }

        string job = evaluateResult.Value.GetProperty("Job").GetRawText();

        statusSetter($"Clocked in to job: {job}");
    }

    internal enum Clocking {
        In,
        Out
    }

    internal static async Task Clock(IPage page, Action<string> statusSetter, Clocking type) {
        var isClockingIn = type == Clocking.In;

        await page.GotoAsync(MainWindowViewModel.BASE_URL);

        await page.EvaluateAsync(@"() => document.getElementsByName(""ClockModuleDisplay"")[0].click();");

        await page.GetByText(isClockingIn ? "Clock In" : "Clock Out").ClickAsync();

        statusSetter(isClockingIn ? "Clocked in" : "Clocked out");

        // could get status here but would require some sort of check to see if the iTime backend has updated
    }
}


using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Zephyr.Main;
internal class MainModel {

    internal static async Task<bool> GetStatus(IPage page) {
        await page.GotoAsync(MainWindowViewModel.BASE_URL + "Information");

        return await page.EvaluateAsync<bool>(@"() => {
            const resultsChildren = document.getElementById(""Result"").children;
            const clockingRecords = resultsChildren[resultsChildren.length - 1];

            if(clockingRecords.tagName !== ""DIV"") return false;

            const date = (""0"" + (new Date()).getDate()).slice(-2);

            const actionsForToday = 
                [...clockingRecords.getElementsByTagName(""tr"")].slice(1)
                .map((row) => [...row.getElementsByTagName(""td"")])
                .filter(([clock]) => clock.innerText.startsWith(date))
                .map(([, action]) => action.innerText);

            if(!actionsForToday.length) return false;

            const lastIn = actionsForToday.findLastIndex(action => action === ""IN"");

            if(lastIn < 0) return false;

            const lastOut = actionsForToday.findLastIndex(action => action === ""OUT"");

            if(lastOut < 0) return true;

            return lastIn > lastOut;
        }");
    }

    internal enum Clocking {
        In,
        Out
    }

    internal static async Task Clock(IPage page, Clocking type) {
        var isClockingIn = type == Clocking.In;

        await page.GotoAsync(MainWindowViewModel.BASE_URL);

        await page.EvaluateAsync(@"() => document.getElementsByName(""ClockModuleDisplay"")[0].click();");

        await page.GetByText(isClockingIn ? "Clock In" : "Clock Out").ClickAsync();

        // TODO: Could check for if its successful using on page text
    }
}


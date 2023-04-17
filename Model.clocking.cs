using MethodTimer;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Zephyr;

public partial class Model {
    public enum Clocking {
        In,
        Out
    }

    [Time]
    public static async Task Clock(IPage page, Clocking type) {
        var isClockingIn = type == Clocking.In;

        await page.GotoAsync(BASE_URL);

        await page.EvaluateAsync(@"() => document.getElementsByName(""ClockModuleDisplay"")[0].click();");

        await page.GetByText(isClockingIn ? "Clock In" : "Clock Out").ClickAsync();

        // TODO: Could check for if its successful using on page text
    }

    private const string GET_TODAYS_EVENTS = @"() => {
        const resultsChildren = document.getElementById(""Result"").children;
        const clockingRecords = resultsChildren[resultsChildren.length - 1];

        if(clockingRecords.tagName !== ""DIV"") return false;

        const date = (""0"" + (new Date()).getDate()).slice(-2);

        const todaysEvents = 
            [...clockingRecords.getElementsByTagName(""tr"")].slice(1)
            .map((row) => [...row.getElementsByTagName(""td"")])
            .filter(([clock]) => clock.innerText.startsWith(date));

        return todaysEvents;
    }";

    private const string GET_CLOCKING = @"(todaysEvents) => {
        const actionsForToday = todaysEvents.map(([, action,]) => action.innerText);

        if(!actionsForToday.length) return false;

        const lastIn = actionsForToday.findLastIndex(action => action === ""IN"");

        if(lastIn < 0) return false;

        const lastOut = actionsForToday.findLastIndex(action => action === ""OUT"");

        if(lastOut < 0) return true;

        return lastIn > lastOut;
    }";

    private const string GET_SEQUENCE = @"(todaysEvents) => {
        if(!todaysEvents.length) return """";

        const lastItem = todaysEvents[todaysEvents.length - 1];
        
        return lastItem[3].innerText;
    }";

    public struct Status {
        public bool IsClockedIn;
        public string Sequence;
    }

    [Time]
    public static async Task<Status> GetStatus(IPage page) {
        await page.GotoAsync(BASE_URL + "Information");

        var todaysEvents = await page.EvaluateHandleAsync(GET_TODAYS_EVENTS);

        var isClockedIn = await page.EvaluateAsync<bool>(GET_CLOCKING, todaysEvents);

        var sequence = await page.EvaluateAsync<string>(GET_SEQUENCE, todaysEvents);

        return new Status() {
            IsClockedIn = isClockedIn,
            Sequence = sequence
        };
    }
}

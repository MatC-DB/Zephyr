using MethodTimer;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Zephyr.Interface;

public partial class Model {
    private const string GET_TODAYS_EVENTS = @"() => {
        const resultsChildren = document.getElementById(""Result"").children;
        const clockingRecords = resultsChildren[resultsChildren.length - 1];

        if(clockingRecords.tagName !== ""DIV"") return [];

        const date = (""0"" + (new Date()).getDate()).slice(-2);

        const todaysEvents = 
            [...clockingRecords.getElementsByTagName(""tr"")].slice(1)
            .map((row) => [...row.getElementsByTagName(""td"")])
            .filter(([clock]) => clock.innerText.startsWith(date));

        return todaysEvents;
    }";

    private static async Task<IJSHandle?> GetTodaysEvents(IPage page) {
        await page.GotoAsync(BASE_URL + "Information");

        return await page.EvaluateHandleAsync(GET_TODAYS_EVENTS);
    }

    private const string GET_CLOCKING = @"(todaysEvents) => {
        if(!todaysEvents.length) return false;

        const actionsForToday = todaysEvents.map(([, action,]) => action.innerText);

        const lastIn = actionsForToday.findLastIndex(action => action === ""IN"");

        if(lastIn < 0) return false;

        const lastOut = actionsForToday.findLastIndex(action => action === ""OUT"");

        if(lastOut < 0) return true;

        return lastIn > lastOut;
    }";

    [Time]
    public static async Task<bool> GetClockingStatus(IPage page) {
        var todaysEvents = await GetTodaysEvents(page);

        return await page.EvaluateAsync<bool>(GET_CLOCKING, todaysEvents);
    }

    private const string GET_WORK_FROM_HOME = @"(todaysEvents) => {
        if(!todaysEvents.length) return false;

        const actionsForToday = todaysEvents.map(([, action,]) => action.innerText);

        const wfm = actionsForToday.findLastIndex(action => action === ""Clock to: Working From Home"");

        if(wfm < 0) return false;

        const lastOut = actionsForToday.findLastIndex(action => action === ""OUT"");

        if(wfm < lastOut) return false;

        const office = actionsForToday.findLastIndex(action => action === ""Clock to: Normal Working Hours"");

        if(office < 0) return true;

        return wfm > office;
    }";

    private const string GET_SEQUENCE = @"(todaysEvents) => {
        if(!todaysEvents.length) return """";

        return todaysEvents.map(([,,,sequence]) => sequence.innerText).findLast(s => !!s);
    }";

    public struct Status {
        public bool IsClockedIn;
        public bool IsWfm;
        public string Sequence;
    }

    [Time]
    public static async Task<Status> GetStatus(IPage page) {
        var todaysEvents = await GetTodaysEvents(page);

        var isClockedIn = await page.EvaluateAsync<bool>(GET_CLOCKING, todaysEvents);

        var isWfm = await page.EvaluateAsync<bool>(GET_WORK_FROM_HOME, todaysEvents);

        var sequence = await page.EvaluateAsync<string>(GET_SEQUENCE, todaysEvents);

        return new Status() {
            IsClockedIn = isClockedIn,
            IsWfm = isWfm,
            Sequence = sequence
        };
    }
}


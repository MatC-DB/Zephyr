using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Threading.Tasks;

namespace Zephyr;
public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private readonly MainWindowViewModel _mainWindowViewModel;

    public IScreen HostScreen { get { return _mainWindowViewModel; } }

    public string UrlPathSegment { get; } = "CONTENT";

    [Reactive]
    public string Status { get; set; } = "Loading";

    public MainViewModel(MainWindowViewModel screen) {
        _mainWindowViewModel = screen;

        _ = GetStatus();
    }

    private async Task Login(IPage page) {
        await LoginModel.Login(page, _mainWindowViewModel.Username, _mainWindowViewModel.Password);
    }

    private async Task GetStatus() {
        var page = await _mainWindowViewModel.GetLockedPage();

        try {
            await Login(page);

            await page.GotoAsync("https://time.dbbroadcast.co.uk:442/Information");

            var evaluateResult = await page.EvaluateAsync(@"() => {
                const resultsChildren = document.getElementById(""Result"").children;
                const clockingRecords = resultsChildren[resultsChildren.length - 1];

                if(clockingRecords.tagName !== ""DIV"") return {Job: ""???"", IsIn: false};

                const tableRows = [...clockingRecords.getElementsByTagName(""tr"")].slice(1);
                const tableData = tableRows.map((row) => [...row.getElementsByTagName(""td"")].map((col) => col.innerText))
                const transform = tableData.map(([clock, action, job]) => ({clock, action, job}));

                const date = (""0"" + (new Date()).getDate()).slice(-2);
                const transformFiltered = transform.filter(({clock}) => clock.startsWith(date));

                const lastItem = transformFiltered[transformFiltered.length - 1] ?? {job: ""???""}

                const details = {Job: lastItem.job, IsIn: false};

                const lastIn = transformFiltered.findLastIndex(row => row.action === ""IN"");

                if(lastIn < 0) return details;

                const lastOut = transformFiltered.findLastIndex(row => row.action === ""OUT"");

                details.IsIn = lastOut < 0 ? true : (lastIn > lastOut);
                return details;
            }");

            StatusData data = new() {
                Job = evaluateResult.Value.GetProperty("Job").GetRawText(),
                IsIn = evaluateResult.Value.GetProperty("IsIn").GetBoolean()
            };

            Status = data.ToString();
        } catch (Exception err) {
            Status = err.Message;
        } finally {
            _mainWindowViewModel.ReleasePage();
        }
    }

    private struct StatusData {
        public string Job;
        public bool IsIn;

        public override string ToString() => $"Job: {Job}; IsIn: {IsIn}";
    }
}


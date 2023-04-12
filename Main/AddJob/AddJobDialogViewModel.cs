using DynamicData;
using DynamicData.Binding;
using Microsoft.Playwright;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zephyr.Main.Job;

namespace Zephyr.Main.AddJob;

public class AddJobDialogViewModel : ReactiveObject {
    private readonly IPage _page;

    [Reactive]
    public bool IsAtJob { get; set; } = true;

    public ICommand OnNext { get; }
    public ICommand OnPrevious { get; }

    [Reactive]
    public string JobSearchText { get; set; } = "";

    private readonly SourceList<string> _jobSearchSourceCache = new();
    private readonly ReadOnlyObservableCollection<string> _jobSearchResults;

    public ReadOnlyObservableCollection<string> JobSearchResults => _jobSearchResults;

    [Reactive]
    public string? SelectedJob { get; set; } = null;

    private readonly ObservableAsPropertyHelper<bool> _isNextEnabled;
    public bool IsNextEnabled => _isNextEnabled.Value;

    [Reactive]
    public string SequenceSearchText { get; set; } = "";

    private readonly SourceList<string> _sequenceSearchSourceCache = new();
    private readonly ReadOnlyObservableCollection<string> _sequenceSearchResults;

    public ReadOnlyObservableCollection<string> SequenceSearchResults => _sequenceSearchResults;

    [Reactive]
    public string? SelectedSequence { get; set; } = null;

    private readonly ObservableAsPropertyHelper<bool> _isOkEnabled;
    public bool IsOkEnabled => _isOkEnabled.Value;

    public bool IsOk { get; private set; }

    public ReactiveCommand<Unit, JobControlViewModel?> AddJob { get; }
    public ReactiveCommand<Unit, JobControlViewModel?> CancelAddJob { get; }

    public AddJobDialogViewModel(IPage page) {
        _page = page;

        BuildFilteredData(ref _jobSearchSourceCache, vm => vm.JobSearchText, out _jobSearchResults);

        this.WhenValueChanged(vm => vm.SelectedJob)
            .Select(job => job != null)
            .ToProperty(this, vm => vm.IsNextEnabled, out _isNextEnabled);

        OnNext = ReactiveCommand.CreateFromTask(Next);

        OnPrevious = ReactiveCommand.Create(() => IsAtJob = true);

        BuildFilteredData(ref _sequenceSearchSourceCache, vm => vm.SequenceSearchText, out _sequenceSearchResults);

        this.WhenValueChanged(vm => vm.SelectedSequence)
            .Select(sequence => sequence != null)
            .ToProperty(this, vm => vm.IsOkEnabled, out _isOkEnabled);

        AddJob = ReactiveCommand.Create(() => {
            if (SelectedJob == null || SelectedSequence == null)
                return null;

            return new JobControlViewModel() {
                Job = SelectedJob,
                Sequence = SelectedSequence
            };
        });

        CancelAddJob = ReactiveCommand.Create<JobControlViewModel?>(() => { 
            return null; 
        });

        Task.Run(GetJobOptions);
    }

    private void BuildFilteredData(
        ref SourceList<string> sourceList,
        System.Linq.Expressions.Expression<Func<AddJobDialogViewModel, string>> filterPropertyAccessor,
        out ReadOnlyObservableCollection<string> output
    ) {
        var filter =
            this.WhenValueChanged(filterPropertyAccessor)
                .Throttle(TimeSpan.FromMilliseconds(250), RxApp.TaskpoolScheduler)
                .Select<string?, Func<string, bool>>(filter =>
                    (string item) => string.IsNullOrEmpty(filter) || item.Contains(filter, StringComparison.InvariantCultureIgnoreCase)
                );

        sourceList.Connect()
            .Filter(filter)
            .Sort(SortExpressionComparer<string>.Ascending(a => a))
            .Bind(out output)
            .Subscribe();
    }

    private async Task GetJobOptions() {
        await _page.GotoAsync(MainWindowViewModel.BASE_URL);

        await _page.EvaluateAsync(@"() => document.getElementsByName(""JobCostingModuleDisplay"")[0].click();");

        var options = await _page.EvaluateAsync<string[]>(
            @"() => [...document.getElementById(""JobChange"").getElementsByTagName(""option"")].slice(1).map(o => o.innerText);"
        );

        _jobSearchSourceCache.Clear();
        _jobSearchSourceCache.AddRange(options);
    }

    private async Task Next() {
        if (SelectedJob is null)
            throw new InvalidOperationException("Selected job must not be null");

        await _page.Locator("#JobChange").SelectOptionAsync(new SelectOptionValue() { Label = SelectedJob });

        int retries = 0;
        const int MAX_RETIES = 20;

        string[] options;
        while (true) {
            options = await _page.EvaluateAsync<string[]>(
                @"() => [...document.getElementById(""SequenceChange"").getElementsByTagName(""option"")].slice(1).map(o => o.innerText);"
            );

            if (options.Length > 0)
                break;

            // silently fail if we can't get options
            if (++retries > MAX_RETIES) {
                _sequenceSearchSourceCache.Clear();

                IsAtJob = false;
                return;
            }

            await Task.Delay(25);
        }

        _sequenceSearchSourceCache.Clear();
        _sequenceSearchSourceCache.AddRange(options);

        IsAtJob = false;
    }
}


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
using Zephyr.Job;

namespace Zephyr.AddJob;

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

        CancelAddJob = ReactiveCommand.Create<JobControlViewModel?>(() => {
            return null;
        });

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

        AddJob = ReactiveCommand.CreateFromTask<JobControlViewModel?>(async() => {
            if (SelectedJob == null || SelectedSequence == null)
                throw new InvalidOperationException("Can't add job without having selected job and sequence");

            var value = await _page.EvaluateAsync<string>(
                "(sequence) => ([...document.getElementById(\"SequenceChange\").getElementsByTagName(\"option\")]" +
                ".slice(1).find(o => o.innerText === sequence)).value;", SelectedSequence);

            return new JobControlViewModel(SelectedJob, SelectedSequence, value);
        });
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
                    (string item) => 
                        string.IsNullOrEmpty(filter) 
                        || item.Contains(filter, StringComparison.InvariantCultureIgnoreCase)
                );

        sourceList.Connect()
            .Filter(filter)
            .Sort(SortExpressionComparer<string>.Ascending(a => a))
            .Bind(out output)
            .Subscribe();
    }

    public async Task GetJobOptions() {
        await Model.OpenJobsPanel(_page);

        var options = await _page.EvaluateAsync<string[]>(
            "() => [...document.getElementById(\"JobChange\").getElementsByTagName(\"option\")].slice(1).map(o => o.innerText);"
        );

        _jobSearchSourceCache.Clear();
        _jobSearchSourceCache.AddRange(options);
    }

    private async Task Next() {
        if (SelectedJob is null)
            throw new InvalidOperationException("Next called before job selection");

        var options = await GetSequenceOptions();

        _sequenceSearchSourceCache.Clear();

        if (options is not null) 
            _sequenceSearchSourceCache.AddRange(options);
        
        IsAtJob = false;
    }

    private async Task<string[]?> GetSequenceOptions() {
        await Model.SelectOption(_page, "JobChange", SelectedJob!);

        for (int retryCount = 0; retryCount < 20; retryCount++) {
            var options = await _page.EvaluateAsync<string[]>(
                "() => [...document.getElementById(\"SequenceChange\").getElementsByTagName(\"option\")].slice(1).map(o => o.innerText);");

            if (options is not null && options.Length > 0) {
                return options;
            }

            await Task.Delay(25);
        }

        return null;
    }
}


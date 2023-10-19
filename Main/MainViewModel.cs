using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Zephyr.AddJob;
using Zephyr.Job;
using Zephyr.Settings;

namespace Zephyr.Main;

public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private readonly Model _model;
    private readonly Action _triggerSave;

    #region RoutingParams
    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = "CONTENT";
    #endregion

    #region ClockingAndSettingParams
    private Model.Clocking? _clocking = null;

    public bool IsClockedIn { get { return _clocking == Model.Clocking.In; } }

    public bool IsClockedOut { get { return _clocking == Model.Clocking.Out; } }

    public ReactiveCommand<Model.Clocking, Unit> OnClock { get; }

    public ICommand OnRefresh { get; }

    public Interaction<SettingsViewModel, SettingsModel.Settings> ShowSettingsDialog { get; }

    public ICommand OnOpenSettings { get; }
    #endregion

    #region WorkAreaParams
    private Model.WorkAreas? _workArea = null;

    public bool IsOfficeSelected { get { return _workArea == Model.WorkAreas.Office; } }

    public bool IsWfhSelected { get { return _workArea == Model.WorkAreas.Wfm; } }

    public ReactiveCommand<Model.WorkAreas, Unit> OnWorkAreaSelected { get; }

    public bool ShowWorkAreas { get { return _model.Settings.ShowWorkAreas; } }
    #endregion

    #region JobParams
    [Reactive]
    public string CurrentJobSequenceValue { get; private set; } = string.Empty;

    [Reactive]
    public bool IsViewportSmall { get; set; }

    public ICommand OnAddJob { get; }

    public ObservableCollection<JobControlViewModel> Jobs { get { return _model.Jobs; } set { _model.Jobs = value; } }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobDelete { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobSelect { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobMoveUp { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobMoveDown { get; }

    public Interaction<AddJobDialogViewModel, JobControlViewModel?> ShowAddJobDialog { get; }
    #endregion

    public MainViewModel(IScreen screen, Model model, Action triggerSave) {
        _model = model;
        _triggerSave = triggerSave;

        // routing
        HostScreen = screen;

        // clocking
        OnClock = ReactiveCommand.CreateFromTask<Model.Clocking>(Clock);

        // work areas
        OnRefresh = ReactiveCommand.CreateFromTask(GetStatus);

        OnWorkAreaSelected = ReactiveCommand.CreateFromTask<Model.WorkAreas>(SetWorkArea);

        ShowSettingsDialog = new Interaction<SettingsViewModel, SettingsModel.Settings>();

        OnOpenSettings = ReactiveCommand.CreateFromTask(OpenSettings);

        // jobs
        ShowAddJobDialog = new Interaction<AddJobDialogViewModel, JobControlViewModel?>();

        OnAddJob = ReactiveCommand.CreateFromTask(AddJob);

        OnJobDelete = ReactiveCommand.Create<JobControlViewModel>((model) => {
            Jobs.Remove(model);

            _triggerSave();
        });

        OnJobSelect = ReactiveCommand.CreateFromTask<JobControlViewModel>(SelectJob);

        OnJobMoveUp = ReactiveCommand.Create<JobControlViewModel>((model) => MoveJob(model, 1));
        OnJobMoveDown = ReactiveCommand.Create<JobControlViewModel>((model) => MoveJob(model, -1));
    }

    private void SetWorkAreaStatus(Model.WorkAreas? status) {
        _workArea = status;

        this.RaisePropertyChanged(nameof(IsOfficeSelected));
        this.RaisePropertyChanged(nameof(IsWfhSelected));
    }

    private void SetClockingStatus(Model.Clocking? status) {
        _clocking = status;

        this.RaisePropertyChanged(nameof(IsClockedIn));
        this.RaisePropertyChanged(nameof(IsClockedOut));

        if (status == Model.Clocking.Out) {
            CurrentJobSequenceValue = string.Empty;
            this.RaisePropertyChanged(nameof(CurrentJobSequenceValue));
        }
    }

    public async Task GetStatus() {
        await _model.RunTask(async (page) => {
            var status = await Model.GetStatus(page);
            SetClockingStatus(status.IsClockedIn ? Model.Clocking.In : Model.Clocking.Out);
            SetWorkAreaStatus(status.IsWfm ? Model.WorkAreas.Wfm : Model.WorkAreas.Office);
            CurrentJobSequenceValue = status.Sequence;
        });
    }

    private async Task Clock(Model.Clocking type) {
        await _model.RunTask(async (page) => {
            await Model.Clock(page, type);

            SetClockingStatus(type);
        });
    }

    private async Task SetWorkArea(Model.WorkAreas area) {
        await _model.RunTask(async (page) => {
            await Model.EnsureClockedIn(page);

            SetClockingStatus(Model.Clocking.In);

            await Model.SetWorkArea(page, area);

            SetWorkAreaStatus(area);
        });
    }

    private async Task OpenSettings() {
        var settingsDialog = new SettingsViewModel(_model.Settings);

        var result = await ShowSettingsDialog.Handle(settingsDialog);

        _model.Settings = result;

        this.RaisePropertyChanged(nameof(ShowWorkAreas));
    }

    private async Task AddJob() {
        await _model.RunTask(async (page) => {
            var addJobDialog = new AddJobDialogViewModel(page);

            var result = await ShowAddJobDialog.Handle(addJobDialog);

            if (result is not null)
                Jobs.Add(result);

            _triggerSave();
        });
    }

    private void MoveJob(JobControlViewModel model, int moveAmount) {
        int index = Jobs.IndexOf(model);
        int newIndex = Math.Clamp(index + moveAmount, 0, Jobs.Count - 1);
        Jobs.Move(index, newIndex);

        _triggerSave();
    }

    private async Task SelectJob(JobControlViewModel model) {
        await _model.RunTask(async (page) => {
            await Model.EnsureClockedIn(page);

            SetClockingStatus(Model.Clocking.In);

            await Model.OpenJobsPanel(page);

            await Model.SelectJob(page, model.Job, model.Sequence);

            CurrentJobSequenceValue = model.SequenceValue;
        });
    }
}


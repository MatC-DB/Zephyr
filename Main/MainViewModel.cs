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

namespace Zephyr.Main;

public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private readonly Model _model;

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = "CONTENT";

    [Reactive]
    public bool IsClockedIn { get; private set; } = false;

    [Reactive]
    public bool IsClockedOut { get; private set; } = false;

    [Reactive]
    public string CurrentJobSequenceValue { get; private set; } = string.Empty;

    public ICommand OnClockIn { get; }

    public ICommand OnClockOut { get; }

    [Reactive]
    public bool IsViewportSmall { get; set; }

    public ICommand OnAddJob { get; }

    public ObservableCollection<JobControlViewModel> Jobs { get { return _model.Jobs; } set { _model.Jobs = value; } }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobDelete { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobSelect { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobMoveUp { get; }

    public ReactiveCommand<JobControlViewModel, Unit> OnJobMoveDown { get; }

    public Interaction<AddJobDialogViewModel, JobControlViewModel?> ShowAddJobDialog { get; }

    public MainViewModel(IScreen screen, Model model) {
        _model = model;

        HostScreen = screen;

        OnClockIn = ReactiveCommand.CreateFromTask(ClockIn);
        OnClockOut = ReactiveCommand.CreateFromTask(ClockOut);

        ShowAddJobDialog = new Interaction<AddJobDialogViewModel, JobControlViewModel?>();

        OnAddJob = ReactiveCommand.CreateFromTask(async () => {
            await _model.RunTask(async (page) => {
                var addJobDialog = new AddJobDialogViewModel(page);

                var result = await ShowAddJobDialog.Handle(addJobDialog);

                if (result is not null)
                    Jobs.Add(result);
            });
        });

        OnJobDelete = ReactiveCommand.Create<JobControlViewModel>((model) => Jobs.Remove(model));

        OnJobSelect = ReactiveCommand.CreateFromTask<JobControlViewModel>(async (model) => {
            await _model.RunTask(async (page) => {
                if (!(await Model.GetStatus(page)).IsClockedIn) {
                    await Model.Clock(page, Model.Clocking.In);

                    SetClockingStatus(true);

                    for (int retry = 0; retry < 20; retry++) {
                        if ((await Model.GetStatus(page)).IsClockedIn)
                            break;

                        await Task.Delay(25);
                    }
                }

                await Model.OpenJobsPanel(page);

                await Model.SelectJob(page, model.Job, model.Sequence);

                CurrentJobSequenceValue = model.SequenceValue;
            });
        });

        OnJobMoveUp = ReactiveCommand.Create<JobControlViewModel>((model) => JobMove(model, 1));
        OnJobMoveDown = ReactiveCommand.Create<JobControlViewModel>((model) => JobMove(model, -1));
    }

    private void JobMove(JobControlViewModel model, int moveAmount) {
        int index = Jobs.IndexOf(model);
        int newIndex = Math.Clamp(index + moveAmount, 0, Jobs.Count - 1);
        Jobs.Move(index, newIndex);
    }

    private void SetClockingStatus(bool status) {
        IsClockedIn = status;
        IsClockedOut = !status;
    }

    public async Task GetStatus() {
        await _model.RunTask(async (page) => {
            var status = await Model.GetStatus(page);
            SetClockingStatus(status.IsClockedIn);
            CurrentJobSequenceValue = status.Sequence;
        });
    }

    private async Task ClockIn() {
        await Clock(Model.Clocking.In);
    }

    private async Task ClockOut() {
        await Clock(Model.Clocking.Out);
    }

    private async Task Clock(Model.Clocking type) {
        await _model.RunTask(async (page) => {
            await Model.Clock(page, type);
            SetClockingStatus(type == Model.Clocking.In);
        });
    }
}


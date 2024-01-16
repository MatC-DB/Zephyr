using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Zephyr.Job;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Zephyr.Interface;

namespace Zephyr;

[DataContract]
public partial class MainWindowViewModel : ReactiveObject, IScreen {
    private readonly Model _model;

    private readonly object _processesRunningLock = new();
    private int _processesRunning = 0;

    public RoutingState Router { get; } = new();

    [Reactive]
    public bool IsLoading { get; private set; } = false;

    public Interaction<Exception, Unit> ErrorDialog { get; }

    #region SavedData
    [DataMember]
    public string Username { 
        get { return _model.Username; }
        set { _model.Username = value; } 
    }

    [DataMember]
    public string Password { 
        get { return _model.Password; } 
        set { _model.Password = value; }
    }

    [DataMember]
    public ObservableCollection<JobControlViewModel> Jobs { 
        get { return _model.Jobs; } 
        set { _model.Jobs = value; } 
    }

    [DataMember]
    public bool IsAutoLoginEnabled { 
        get { return _model.IsAutoLoginEnabled; } 
        set { _model.IsAutoLoginEnabled = value; } 
    }
    #endregion

    [Reactive]
    public bool IsFocused { get; set; } = true;

    private readonly Action _loginOnLoad;

    public MainWindowViewModel() {

        ErrorDialog = new Interaction<Exception, Unit>();

        _model = new(ErrorDialog.Handle, IncrementProcessesRunning, DecrementProcessesRunning);

        Task.Run(_model.InitializePage);

        var loginViewModel = new Login.LoginViewModel(this, _model, TriggerSave);
        _loginOnLoad = loginViewModel.OnLoad;

        Router.Navigate.Execute(loginViewModel);
    }

    public void OnLoad() {
        _loginOnLoad();
    }

    private void IsLoadingCheck() {
        IsLoading = _processesRunning > 0;
    }

    private void IncrementProcessesRunning() {
        lock (_processesRunningLock) {
            ++_processesRunning;
            IsLoadingCheck();
        }
    }

    private void DecrementProcessesRunning() {
        lock (_processesRunningLock) {
            --_processesRunning;
            IsLoadingCheck();
        }
    }

    private void TriggerSave() {
        AkavacheSuspensionDriver suspensionDriver = new();
        suspensionDriver.SaveState(this).Subscribe();
    }
}

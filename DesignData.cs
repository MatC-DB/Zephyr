using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Zephyr.AddJob;
using Zephyr.Error;
using Zephyr.Job;
using Zephyr.Login;
using Zephyr.Main;
using Zephyr.Interface;

namespace Zephyr;

// DON'T USE ANY OF THIS OUTSIDE OF DESIGN TIME
internal class DesignData {
    private const string USERNAME = "example.user@dbbroadcast.co.uk";
    private const string PASSWORD = "password";

    private static readonly ObservableCollection<JobControlViewModel> _jobs =
        new() {
            new JobControlViewModel("dB1234 - Zephyr Development", "ENG02 - Design Engineering", "000123"),
            new JobControlViewModel("dB2345 - Apis Development", "ENG01 - Engineering", "000234"),
            new JobControlViewModel("dB3456 - FIESTA Development", "WIR03 - Engineering", "000345"),
        };

    private static readonly Model _model =
        new((e) => Observable.FromAsync(async () => await Task.Run(() => Unit.Default)), () => { }, () => { }) {
            Username = USERNAME,
            Password = PASSWORD,
            Jobs = _jobs
        };

    public static MainWindowViewModel MainWindowViewModel =>
        new() {
            Username = USERNAME,
            Password = PASSWORD,
            Jobs = _jobs
        };

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public static AddJobDialogViewModel AddJobDialogViewModel => new(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    public static ErrorDialogViewModel ErrorDialogViewModel => new("ERROR MESSAGE", "STACK TRACE");

    public static JobControlViewModel JobControlViewModel => _jobs[0];

    public static MainViewModel MainViewModel => new(MainWindowViewModel, _model, () => { });

    public static LoginViewModel LoginViewModel => new(MainWindowViewModel, _model, () => { });
}

using ReactiveUI;

namespace Zephyr;
public class MainViewModel : ReactiveObject, IRoutableViewModel {
    private readonly MainWindowViewModel _mainWindowViewModel;

    public IScreen HostScreen { get { return _mainWindowViewModel; } }

    public string UrlPathSegment { get; } = "CONTENT";

    public MainViewModel(MainWindowViewModel screen) {
        _mainWindowViewModel = screen;
    }
}


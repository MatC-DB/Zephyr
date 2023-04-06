using ReactiveUI;

namespace Zephyr {
    public class ContentViewModel : ReactiveObject, IRoutableViewModel {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public IScreen HostScreen { get { return _mainWindowViewModel; } }

        public string UrlPathSegment { get; } = "CONTENT";

        public ContentViewModel(MainWindowViewModel screen) {
            _mainWindowViewModel = screen;
        }
    }
}

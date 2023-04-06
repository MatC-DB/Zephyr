using System.Diagnostics;
using System.Reactive;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tmds.DBus;

namespace Zephyr
{
    [DataContract]
    public partial class MainWindowViewModel : ReactiveObject, IScreen
    {
        private readonly TimeView _timeView;

        public RoutingState Router { get; } = new RoutingState();

        [Reactive, DataMember]
        public string Username { get; set; } = string.Empty;

        [Reactive, DataMember]
        public string Password { get; set; } = string.Empty;

        public ICommand OnLogin { get; }

        [Reactive]
        public bool IsLoading { get; set; } = false;

        [Reactive]
        public bool IsConnected { get; set; } = false;

        [Reactive]
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSignedIn { get; private set; } = false;

        public MainWindowViewModel()
        {
            OnLogin = ReactiveCommand.CreateFromTask(Login);

            _timeView = new TimeView(this);
        }

        public async Task Connect()
        {
            await _timeView.Connect();
        }

        private async Task Login()
        {
            SetIsLoggedIn(await _timeView.TryLogin());
        }

        private void SetIsLoggedIn(bool isLoggedIn)
        {
            IsSignedIn = isLoggedIn;

            var isCurrentlyIn = Router.CurrentViewModel is ContentViewModel;

            if (isLoggedIn)
            {
                if(!isCurrentlyIn) Router.Navigate.Execute(new ContentViewModel(this));
            }
            else
            {
                if(isCurrentlyIn) Router.NavigateBack.Execute();
            }
        }
    }
}

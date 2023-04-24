using ReactiveUI;
using System.Reactive;

namespace Zephyr.Settings;

public class SettingsViewModel : ReactiveObject {
    public bool ShowWorkAreas { get; set; }

    public ReactiveCommand<Unit, SettingsModel.Settings> OnClose { get; }

    public SettingsViewModel(SettingsModel.Settings settings) {
        ShowWorkAreas = settings.ShowWorkAreas;

        OnClose = ReactiveCommand.Create(() => {
            SettingsModel.Settings settings = new() { 
                ShowWorkAreas = ShowWorkAreas 
            }; 

            return settings;
        });
    }
}
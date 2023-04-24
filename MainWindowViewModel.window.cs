using Avalonia.Controls;
using ReactiveUI;
using System.Runtime.Serialization;

namespace Zephyr;

public partial class MainWindowViewModel : ReactiveObject, IScreen {
    [DataMember]
    public double Width { get; set; } = 350;

    [DataMember]
    public double Height { get; set; } = 450;

    public WindowState State { get; set; } = WindowState.Normal;

    [DataMember]
    public bool IsFullScreen {
        get => State == WindowState.Maximized;
        set {
            if (value && State == WindowState.Normal)
                State = WindowState.Maximized;
        }
    }
}

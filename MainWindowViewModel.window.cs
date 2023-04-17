using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System.Runtime.Serialization;
using System.Windows.Input;

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

    public PixelPoint Position { get; set; } = new PixelPoint(50, 50);

    [DataMember]
    public int Top { get { return Position.Y; } set { Position = new PixelPoint(Position.X, value); } }

    [DataMember]
    public int Left { get { return Position.X; } set { Position = new PixelPoint(value, Position.Y); } }

    public ICommand SetPosition { get; }

    private void HandlePosition(PixelPoint position) {
        if (State != WindowState.Normal)
            return;

        Position = position;
    }
}

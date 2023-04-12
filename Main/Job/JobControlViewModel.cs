using ReactiveUI;

namespace Zephyr.Main.Job;

public class JobControlViewModel : ReactiveObject {
    public string Job { get; set; } = "";

    public string Sequence { get; set; } = "";
}


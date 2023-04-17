using Avalonia.Controls.Templates;
using Avalonia.Controls;

namespace Zephyr.Job;

internal class JobControlViewLocator : IDataTemplate {
    public IControl Build(object data) {
        return new JobControl() { DataContext = data };
    }

    public bool Match(object data) {
        return data is JobControlViewModel;
    }
}


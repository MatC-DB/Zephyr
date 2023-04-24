using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Zephyr.Job;

[DataContract]
public class JobControlViewModel : ReactiveObject {
    [DataMember]
    public string Job { get; set; }

    [DataMember]
    public string Sequence { get; set; }

    [DataMember]
    public string SequenceValue { get; set; } = string.Empty;

    [Reactive]
    public bool IsActive { get; set; }

    public JobControlViewModel(string job, string sequence, string sequenceValue) {
        Job = job;
        Sequence = sequence;
        SequenceValue = sequenceValue;
    }
}


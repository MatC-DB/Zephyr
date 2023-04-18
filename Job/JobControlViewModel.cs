using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Zephyr.Job;

[DataContract]
public class JobControlViewModel : ReactiveObject {
    private string _job = string.Empty;

    [DataMember]
    public string Job { 
        get { return _job; }
        set { 
            _job = value;

            Regex regex = new("(?>dB(\\d{4})(?> - | -| )(.*))|.+", RegexOptions.IgnoreCase);

            var result = regex.Matches(value);

            if (result is null || result.Count == 0 || result[0].Groups.Count != 3) {
                JobCode = string.Empty;
                JobName = value;
                return;
            }

            JobCode = result[0].Groups[1].Value;
            JobName = result[0].Groups[2].Value;
        }
    }
    public string JobCode { get; private set; } = string.Empty;

    public string JobName { get; private set; } = string.Empty;

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


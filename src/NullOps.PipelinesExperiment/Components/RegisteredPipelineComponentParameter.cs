using System.Text.RegularExpressions;

namespace NullOps.PipelinesExperiment.Components;

public class RegisteredPipelineComponentParameter
{
    public string Name { get; set; }
    
    public Regex? Expression { get; set; }
    
    public bool Required { get; set; }
}

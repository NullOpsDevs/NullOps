using System.Text.RegularExpressions;

namespace NullOps.PipelinesExperimentV3.Components;

public class RegisteredPipelineComponentParameter
{
    public string Name { get; set; }
    
    public Regex? Expression { get; set; }
    
    public bool Required { get; set; }
}

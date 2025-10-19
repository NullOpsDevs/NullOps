using NullOps.PipelinesExperiment.Validation;

namespace NullOps.PipelinesExperiment;

public class PipelineForest
{
    public PipelineNode? PipelineStart { get; set; }
    public PipelineNode? PipelineSucceeded { get; set; }
    public PipelineNode? PipelineFailed { get; set; }
    public PipelineNode? PipelineCancelled { get; set; }
    
    public bool HasWarnings => ValidationEntries.Any(x => !x.IsError);
    public bool HasErrors => ValidationEntries.Any(x => x.IsError);
    public bool HasValidationProblems => ValidationEntries.Length != 0;
    
    public required PipelineValidationEntry[] ValidationEntries { get; set; }

    public static PipelineForest FromSingleError(string? stepId, PipelinePart part,
        PipelineValidationEntryMessage message)
    {
        return new PipelineForest
        {
            ValidationEntries =
            [
                new PipelineValidationEntry
                {
                    StepId = stepId,
                    Part = part,
                    Message = message,
                    IsError = true
                }
            ]
        };
    }
}

namespace NullOps.PipelinesExperimentV3.Validation;

public interface IPipelineValidationReport
{
    bool HasWarnings { get; }
    bool HasErrors { get; }
    bool HasValidationProblems { get; }
    
    void AddWarning(Guid? stepId, PipelineValidationEntryMessage message);
    void AddError(Guid? stepId, PipelineValidationEntryMessage message);
}

public class PipelineValidationResult : IPipelineValidationReport
{
    public Pipeline? Pipeline { get; set; }

    private readonly List<PipelineValidationEntry> validationEntries = [];
    public IReadOnlyList<PipelineValidationEntry> ValidationEntries => validationEntries;

    /// <inheritdoc />
    public bool HasWarnings => validationEntries.Any(x => !x.IsError);

    /// <inheritdoc />
    public bool HasErrors => validationEntries.Any(x => x.IsError);

    /// <inheritdoc />
    public bool HasValidationProblems => validationEntries.Count != 0;

    public void AddWarning(Guid? stepId, PipelineValidationEntryMessage message)
    {
        validationEntries.Add(new PipelineValidationEntry
        {
            StepId = stepId,
            Message = message,
            IsError = false
        });
    }

    public void AddError(Guid? stepId, PipelineValidationEntryMessage message)
    {
        validationEntries.Add(new PipelineValidationEntry
        {
            StepId = stepId,
            Message = message,
            IsError = true
        });
    }
}

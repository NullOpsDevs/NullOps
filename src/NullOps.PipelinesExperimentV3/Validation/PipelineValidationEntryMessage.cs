namespace NullOps.PipelinesExperimentV3.Validation;

public enum PipelineValidationEntryMessage
{
    StepIdsAreNotUnique,
    UnknownComponentId,
    DependencyReferencesNonExistentStep,
    CyclicDependencyDetected,
    DanglingStep,
    RootStepMustBeUnique,
    RootStepMustNotHaveDependencies,
    MissingParameters
}
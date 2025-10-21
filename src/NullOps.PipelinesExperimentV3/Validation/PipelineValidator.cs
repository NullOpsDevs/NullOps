using System.Collections.Frozen;
using NullOps.PipelinesExperimentV3.Components;
using NullOps.PipelinesExperimentV3.Configuration;

namespace NullOps.PipelinesExperimentV3.Validation;

public static class PipelineValidator
{
    private record PipelineValidationContext(
        IPipelineValidationReport Report,
        ComponentRegistry ComponentRegistry,
        PipelineConfiguration PipelineConfiguration,
        List<Guid> DanglingSteps);
    
    public static PipelineValidationResult Validate(
        ComponentRegistry registeredPipelineComponents,
        PipelineConfiguration pipelineConfiguration)
    {
        var result = new PipelineValidationResult();
        var context = new PipelineValidationContext(result, registeredPipelineComponents, pipelineConfiguration, []);
        
        // =================
        // 1. Validating pipeline steps
        // =================
        
        // 1.1. Validating that all step IDs are unique
        if (!Check_that_all_step_ids_are_unique(context))
            return result;
        
        // 1.2. Validating that each step refers to known component
        if (!Check_that_all_steps_refer_to_known_components(context))
            return result;

        // 1.3. Validating that all dependencies reference existing steps
        if (!Check_that_all_dependencies_reference_existing_steps(context))
            return result;

        // 1.4. Validating that there is only one root step
        if (!Check_for_single_root_step(context))
            return result;

        // 1.5. Validating that there is no cycle in the pipeline
        if (!Check_that_there_is_no_cycle_in_the_pipeline(context))
            return result;

        // 1.6. Validating that there are no dangling branches
        Check_for_dangling_branches(context);
        
        return result;
    }
    
    private static bool Check_that_all_step_ids_are_unique(PipelineValidationContext context)
    {
        var allSteps = context.PipelineConfiguration.Steps;
        
        var allStepsCount = allSteps.Length;
        var uniqueStepsCount = allSteps.Select(x => x.StepId).Distinct().Count();
        var allUnique = allStepsCount == uniqueStepsCount;

        if (allUnique)
            return true;
        
        context.Report.AddError(null, PipelineValidationEntryMessage.StepIdsAreNotUnique);
        return false;
    }
    
    private static bool Check_that_all_steps_refer_to_known_components(PipelineValidationContext context)
    {
        var steps = context.PipelineConfiguration.Steps;

        foreach (var step in steps)
        {
            if (!context.ComponentRegistry.IsComponentRegistered(step.ComponentId))
                context.Report.AddError(step.StepId, PipelineValidationEntryMessage.UnknownComponentId);
        }

        return !context.Report.HasWarnings;
    }

    private static bool Check_that_all_dependencies_reference_existing_steps(PipelineValidationContext context)
    {
        var steps = context.PipelineConfiguration.Steps;
        var allStepIds = new HashSet<Guid>(steps.Select(s => s.StepId));

        foreach (var step in steps)
        {
            foreach (var dependency in step.Dependencies)
            {
                if (!allStepIds.Contains(dependency))
                {
                    context.Report.AddError(step.StepId, PipelineValidationEntryMessage.DependencyReferencesNonExistentStep);
                }
            }
        }

        return !context.Report.HasErrors;
    }

    private static bool Check_for_single_root_step(PipelineValidationContext context)
    {
        var steps = context.PipelineConfiguration.Steps;
        var registry = context.ComponentRegistry;

        var rootNodes = steps.Where(step => registry.IsSystemEvent(step.ComponentId)).ToArray();
        var rootNodesCount = rootNodes.Length;

        if (rootNodesCount > 1)
        {
            foreach (var step in rootNodes) 
                context.Report.AddError(step.StepId, PipelineValidationEntryMessage.RootStepMustBeUnique);
        }
        
        var rootNodesWithDependencies = rootNodes.Where(step => step.Dependencies.Length > 0).ToArray();

        if (rootNodesWithDependencies.Length != 0)
        {
            foreach (var step in rootNodesWithDependencies)
                context.Report.AddError(step.StepId, PipelineValidationEntryMessage.RootStepMustNotHaveDependencies);
        }

        return !context.Report.HasErrors;
    }
    
    private static bool Check_that_there_is_no_cycle_in_the_pipeline(PipelineValidationContext context)
    {
        var nodes = context.PipelineConfiguration.Steps;
        
        var nextNodesForNode = nodes
            .SelectMany(x => x.Dependencies, (node, dependency) => new { node, dependency })
            .GroupBy(x => x.dependency)
            .ToFrozenDictionary(x => x.Key, x => x.Select(y => y.node).ToArray());

        var visited = new HashSet<Guid>();
        var knownSafePaths = new HashSet<Guid>();

        foreach (var startNode in nodes)
        {
            if (knownSafePaths.Contains(startNode.StepId))
                continue;

            var stack = new Stack<(PipelineStepConfiguration Node, bool IsReturning)>();
            stack.Push((startNode, false));

            while (stack.Count > 0)
            {
                var (currentNode, isReturning) = stack.Pop();

                if (isReturning)
                {
                    visited.Remove(currentNode.StepId);
                    knownSafePaths.Add(currentNode.StepId);
                    continue;
                }

                if (knownSafePaths.Contains(currentNode.StepId))
                    continue;

                if (visited.Contains(currentNode.StepId))
                {
                    context.Report.AddError(currentNode.StepId, PipelineValidationEntryMessage.CyclicDependencyDetected);
                    return false;
                }

                var nextNodes = nextNodesForNode.GetValueOrDefault(currentNode.StepId, []);

                visited.Add(currentNode.StepId);

                stack.Push((currentNode, true));

                for (var i = nextNodes.Length - 1; i >= 0; i--)
                {
                    stack.Push((nextNodes[i], false));
                }
            }
        }

        return true;
    }
    
    private static void Check_for_dangling_branches(PipelineValidationContext context)
    {
        var danglingSteps = FindDanglingSteps(context.ComponentRegistry, context.PipelineConfiguration.Steps);

        if (danglingSteps.Length == 0)
            return;

        foreach (var danglingStep in danglingSteps)
        {
            context.Report.AddWarning(danglingStep, PipelineValidationEntryMessage.DanglingStep);
            context.DanglingSteps.Add(danglingStep);
        }
    }

    private static Guid[] FindDanglingSteps(ComponentRegistry registry, PipelineStepConfiguration[] steps)
    {
        var stepList = steps.ToList();
        var allStepIds = new HashSet<Guid>(stepList.Select(s => s.StepId));

        var rootSteps = stepList
            .Where(s => registry.IsSystemEvent(s.ComponentId))
            .Select(s => s.StepId)
            .ToList();

        if (rootSteps.Count == 0)
            return allStepIds.ToArray();

        var dependents = new Dictionary<Guid, List<Guid>>();
        
        foreach (var step in stepList)
        {
            foreach (var dependency in step.Dependencies)
            {
                if (!dependents.ContainsKey(dependency))
                    dependents[dependency] = [];
                
                dependents[dependency].Add(step.StepId);
            }
        }

        var reachable = new HashSet<Guid>();
        var queue = new Queue<Guid>(rootSteps);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!reachable.Add(current))
                continue;

            if (dependents.TryGetValue(current, out var deps))
            {
                foreach (var dependent in deps)
                {
                    if (!reachable.Contains(dependent))
                        queue.Enqueue(dependent);
                }
            }
        }

        return allStepIds.Except(reachable).ToArray();
    }
}
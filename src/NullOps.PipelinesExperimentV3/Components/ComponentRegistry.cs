namespace NullOps.PipelinesExperimentV3.Components;

public class ComponentRegistry
{
    private readonly Dictionary<Guid, RegisteredPipelineComponent> components = new();
    private readonly Dictionary<string, RegisteredPipelineComponent> componentsByName = new();
    private readonly HashSet<Guid> systemEventIds = [];

    public IEnumerable<RegisteredPipelineComponent> GetComponents() => components.Values;

    public RegisteredPipelineComponent? GetComponent(Guid componentId) => components.GetValueOrDefault(componentId);
    
    public RegisteredPipelineComponent? GetComponent(string componentName) => componentsByName.GetValueOrDefault(componentName);
    
    public bool AddComponent(RegisteredPipelineComponent component)
    {
        var byIdExists = components.ContainsKey(component.ComponentId);
        var byNameExists = componentsByName.ContainsKey(component.Name);
        
        if(byIdExists || byNameExists)
            return false;
        
        components.Add(component.ComponentId, component);
        componentsByName.Add(component.Name, component);
        
        if(component.IsSystemEvent)
            systemEventIds.Add(component.ComponentId);
        
        return true;
    }
    
    public bool IsComponentRegistered(Guid componentId) => components.ContainsKey(componentId);
    
    public bool IsSystemEvent(Guid componentId) => systemEventIds.Contains(componentId);
}
using System.Diagnostics.CodeAnalysis;
using NullOps.PipelinesExperimentV2.Extensions;
using NullOps.PipelinesExperimentV2.Models;

namespace NullOps.PipelinesExperimentV2.Builders;

public static class PipelineBuilder
{
    public static bool TryBuild(List<PipelineNode> nodes, [NotNullWhen(true)] out Dictionary<Event, Component[]>? pipelines)
    {
        try
        {
            var events = nodes.Where(x => x is Event).OfType<Event>().ToArray();
            var components = nodes.Where(x => x is Component).OfType<Component>().ToArray();
            var services = nodes.Where(x => x is Service).OfType<Service>().ToArray();
            
            SetupFollowers(components);
            
            if (components.Any(x => !x.IsValid()))
                throw new Exception("Invalid configure pipeline components");
            
            pipelines = events.Select(x => (x, GetRouteForEvent(components, x))).ToDictionary();    
        }
        catch (Exception e)
        {
            pipelines = null;
            return false;
        }
        
        return true;
    }
    

    public static void SetupFollowers(Component[] components)
    {
        var dict = components.ToDictionary(x => x.Id, x => x);
        
        foreach (var component in components)
        {
            var subscriptions = component.SubscriptionIds;
            foreach (var subscription in subscriptions)
            {
                if (subscription == Constants.Start.Id)
                    continue;
                
                if (subscription == Constants.Failed.Id)
                    continue;
                
                if (subscription == Constants.Cancelled.Id)
                    continue;
                
                if (!dict.TryGetValue(subscription, out var nextNode))
                    throw new Exception("Invalid subscription");
                
                if (nextNode is not Component nextComponent)
                    throw new Exception("Invalid type subscription");
                
                nextComponent.FollowerIds.Add(component.Id);
            }
        }
    }
    
    public static Component[] GetRouteForEvent(Component[] components, Event eventNode)
    {
        var startIds = new [] { eventNode.Id };
        var road = new HashSet<string>();
        
        var ids = startIds;
        
        // Find all roads
        while (true)
        {
            var nextComponents = components.Where(x => x.SubscriptionIds.Any(c => ids.Contains(c))).ToArray();
            if (nextComponents.Length == 0)
                break;
            
            // Save points where you already were
            foreach (var component in nextComponents)
                if (!road.Add(component.Id))
                    throw new Exception("Invalid route");
            
            ids = nextComponents.Select(x => x.Id).ToArray();
        }
        
        return components.Where(x => road.Contains(x.Id)).ToArray();
    }
}
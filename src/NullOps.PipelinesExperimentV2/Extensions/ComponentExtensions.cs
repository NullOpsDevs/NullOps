using NullOps.PipelinesExperimentV2.Models;

namespace NullOps.PipelinesExperimentV2.Extensions;

public static class ComponentExtensions
{
    public static bool IsValid(this Component component)
    {
        if (component.SubscriptionIds.Count == 0)
            return false;
        
        if (component.FollowerIds.Contains(component.Id))
            return false;
        
        if (component.FollowerIds.Contains(component.Id))
            return false;
        
        if (component.SubscriptionIds.Any(subscription => component.FollowerIds.Contains(subscription)))
            return false;
        
        return true;
    }
}
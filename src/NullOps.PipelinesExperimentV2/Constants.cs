using NullOps.PipelinesExperimentV2.Models;

namespace NullOps.PipelinesExperimentV2;

public class Constants
{
    public static readonly Event Start = new ()
    {
        Name = "Start",
        Id = "Start event id",
        Description = "Entry point pipeline"
    };
    
    public static readonly Event Cancelled = new ()
    {
        Name = "Cancelled",
        Id = "Cancelled event id",
        Description = "Entry point when pipeline is cancelled"
    };
    
    public static readonly Event Failed = new ()
    {
        Name = "Failed",
        Id = "Failed event id",
        Description = "Entry point when pipeline is failed"
    };
}
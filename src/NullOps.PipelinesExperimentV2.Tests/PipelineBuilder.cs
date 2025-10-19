using NullOps.PipelinesExperimentV2.Builders;
using NullOps.PipelinesExperimentV2.Models;

namespace NullOps.PipelinesExperimentV2.Tests;

public class PipelineBuilderTests
{
    [Test]
    public void BuildingPipeline_WhenValidConfiguration_OneEventStart()
    {
        // Start--1--*--2--4
        //           |
        //           *--3--5--*--6--*--8
        //                    |     |
        //                    *--7--*
        
        var nodes = new List<PipelineNode>
        {
            Constants.Start,
            new Component() { Name = "1", Id = "1", SubscriptionIds = [Constants.Start.Id], },
            new Component() { Name = "2", Id = "2", SubscriptionIds = ["1"] },
            new Component() { Name = "3", Id = "3", SubscriptionIds = ["1"] },
            new Component() { Name = "4", Id = "4", SubscriptionIds = ["2"] },
            new Component() { Name = "5", Id = "5", SubscriptionIds = ["3"] },
            new Component() { Name = "6", Id = "6", SubscriptionIds = ["5"] },
            new Component() { Name = "7", Id = "7", SubscriptionIds = ["5"] },
            new Component() { Name = "8", Id = "8", SubscriptionIds = ["6", "7"] },
        };
        
        var result = PipelineBuilder.TryBuild(nodes, out var pipelines);
        
        Assert.That(result, Is.True);
        Assert.That(pipelines, Is.Not.Null);
        Assert.That(pipelines, Has.Count.EqualTo(1));
        
        var (eventNode, pipeline) = pipelines.Single();
        
        Assert.That(eventNode.Id, Is.EqualTo(Constants.Start.Id));
        Assert.That(pipeline, Is.Not.Null);
        Assert.That(pipeline, Has.Length.EqualTo(8));
    }
    
    [Test]
    public void BuildingPipeline_WhenValidConfiguration_Events()
    {
        // Start--1--*--2--4
        //           |
        //           *--3--5--*--6--*--8
        //                    |     |
        //                    *--7--*
        // Failed--*--9---*--11
        //         |      |
        //         *--10--*
        //
        // Cancelled--12--13
        
        var nodes = new List<PipelineNode>
        {
            Constants.Start,
            new Component() { Name = "1", Id = "1", SubscriptionIds = [Constants.Start.Id], },
            new Component() { Name = "2", Id = "2", SubscriptionIds = ["1"] },
            new Component() { Name = "3", Id = "3", SubscriptionIds = ["1"] },
            new Component() { Name = "4", Id = "4", SubscriptionIds = ["2"] },
            new Component() { Name = "5", Id = "5", SubscriptionIds = ["3"] },
            new Component() { Name = "6", Id = "6", SubscriptionIds = ["5"] },
            new Component() { Name = "7", Id = "7", SubscriptionIds = ["5"] },
            new Component() { Name = "8", Id = "8", SubscriptionIds = ["6", "7"] },
            Constants.Failed,
            new Component() { Name = "9", Id = "9", SubscriptionIds = [Constants.Failed.Id], },
            new Component() { Name = "10", Id = "10", SubscriptionIds = [Constants.Failed.Id], },
            new Component() { Name = "11", Id = "11", SubscriptionIds = ["9", "10"], },
            Constants.Cancelled,
            new Component() { Name = "12", Id = "12", SubscriptionIds = [Constants.Cancelled.Id], },
            new Component() { Name = "13", Id = "13", SubscriptionIds = ["12"], },
        };
        
        var result = PipelineBuilder.TryBuild(nodes, out var pipelines);
        
        Assert.That(result, Is.True);
        Assert.That(pipelines, Is.Not.Null);
        Assert.That(pipelines, Has.Count.EqualTo(3));
        
        var (eventNodeStart, pipelineStart) = pipelines.FirstOrDefault(x => x.Key.Id == Constants.Start.Id);
        
        Assert.That(eventNodeStart.Id, Is.EqualTo(Constants.Start.Id));
        Assert.That(pipelineStart, Is.Not.Null);
        Assert.That(pipelineStart, Has.Length.EqualTo(8));
        
        var (eventNodeFailed, pipelineFailed) = pipelines.FirstOrDefault(x => x.Key.Id == Constants.Failed.Id);
        
        Assert.That(eventNodeFailed.Id, Is.EqualTo(Constants.Failed.Id));
        Assert.That(pipelineFailed, Is.Not.Null);
        Assert.That(pipelineFailed, Has.Length.EqualTo(3));
        
        var (eventNodeCancelled, pipelineCancelled) = pipelines.FirstOrDefault(x => x.Key.Id == Constants.Cancelled.Id);
        
        Assert.That(eventNodeCancelled.Id, Is.EqualTo(Constants.Cancelled.Id));
        Assert.That(pipelineCancelled, Is.Not.Null);
        Assert.That(pipelineCancelled, Has.Length.EqualTo(2));
    }
    
    [Test]
    public void BuildingPipeline_WhenInValidConfiguration()
    {
        // Start--1--2--3--4--*
        //        |           |
        //        *-----------*  
        
        var nodes = new List<PipelineNode>
        {
            Constants.Start,
            new Component() { Name = "1", Id = "1", SubscriptionIds = [Constants.Start.Id], },
            new Component() { Name = "2", Id = "2", SubscriptionIds = ["1"] },
            new Component() { Name = "3", Id = "3", SubscriptionIds = ["2"] },
            new Component() { Name = "4", Id = "4", SubscriptionIds = ["3", "1"] },
        };
        
        var result = PipelineBuilder.TryBuild(nodes, out var pipelines);
        
        Assert.That(result, Is.False);
        Assert.That(pipelines, Is.Null);
    }
}


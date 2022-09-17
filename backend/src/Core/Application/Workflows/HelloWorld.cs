using Elsa.Activities.Console;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;

namespace FSH.WebApi.Application.Workflows;

public class HelloWorldWorkflow : IWorkflow
{
  public void Build(IWorkflowBuilder builder) => builder.WriteLine("Hello World!");
}

public class HelloWorldWorkflow2 : IWorkflow
{
  private readonly IClock _clock;

  public HelloWorldWorkflow2(IClock clock)
  {
    _clock = clock;
  }

  public void Build(IWorkflowBuilder builder) => builder
    .AsSingleton()
    .Timer(Duration.FromSeconds(30))
    .WriteLine(context => $"{context.WorkflowExecutionContext.WorkflowInstance.Id} triggered by timer at {_clock.GetCurrentInstant()}.")
    .Timer(Duration.FromSeconds(30))
    .WriteLine(context => $"{context.WorkflowExecutionContext.WorkflowInstance.Id} resumed by timer at {_clock.GetCurrentInstant()}.");
}
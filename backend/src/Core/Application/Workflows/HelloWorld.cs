using Elsa.Activities.Console;
using Elsa.Builders;

namespace FSH.WebApi.Application.Workflows;

public class HelloWorldWorkflow : IWorkflow
{
  public void Build(IWorkflowBuilder builder) => builder.WriteLine("Hello World!");
}
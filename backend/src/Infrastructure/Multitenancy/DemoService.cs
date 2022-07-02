using FSH.WebApi.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class DemoService : IHostedService
{
  private readonly IJobService _jobService;

  public DemoService(IJobService jobService)
  {
    _jobService = jobService;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _jobService.Enqueue(() => Console.WriteLine("This is a demo job is running at startup, it can be anything"));
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    Console.WriteLine("Stopping async ");
    return Task.CompletedTask;
  }
}
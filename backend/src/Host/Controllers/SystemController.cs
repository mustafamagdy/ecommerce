using Serilog;


namespace FSH.WebApi.Host.Controllers;
[AllowAnonymous]
public class SystemController : VersionNeutralApiController
{
  [HttpGet("ping")]
  public async Task<string> PingAsync()
  {
    Log.Information("Ping Request Received.");
    return "Ok";
  }
}
namespace FSH.WebApi.Application.Identity.Roles;

public class AbilityPerRoleDto
{
  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public List<AbilityDto> Abilities { get; set; }
}

public class AbilityDto
{
  public string[] Actions { get; set; }
  public string Resource { get; set; }
}
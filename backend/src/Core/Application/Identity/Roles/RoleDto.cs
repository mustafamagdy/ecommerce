namespace FSH.WebApi.Application.Identity.Roles;

public class RoleDto
{
  public string Id { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public List<AbilityDto> Abilities { get; set; }
}

public class AbilityDto
{
  public string[] Actions { get; set; }
  public string Resource { get; set; }
}
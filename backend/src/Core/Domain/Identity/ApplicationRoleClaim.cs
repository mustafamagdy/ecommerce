using Microsoft.AspNetCore.Identity;

namespace FSH.WebApi.Domain.Identity;

public sealed class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public string? CreatedBy { get; init; }
    public DateTime CreatedOn { get; init; }
}
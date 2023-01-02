using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
  public void Configure(EntityTypeBuilder<ApplicationUser> builder)
  {
    builder
      .ToTable("Users", SchemaNames.Identity)
      .IsMultiTenant();

    builder
      .Property(u => u.ObjectId)
      .HasMaxLength(256);
  }
}

public sealed class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
{
  public void Configure(EntityTypeBuilder<ApplicationRole> builder)
  {
    builder
      .ToTable("Roles", SchemaNames.Identity)
      .IsMultiTenant()
      .AdjustUniqueIndexes();
  }
}

public sealed class ApplicationRoleClaimConfig : IEntityTypeConfiguration<ApplicationRoleClaim>
{
  public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder) =>
    builder
      .ToTable("RoleClaims", SchemaNames.Identity)
      .IsMultiTenant();
}

public sealed class IdentityUserRoleConfig : IEntityTypeConfiguration<ApplicationUserRole>
{
  public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
  {
    builder
      .ToTable("UserRoles", SchemaNames.Identity)
      .IsMultiTenant();

    builder
      .HasOne(e => e.User)
      .WithMany(e => e.UserRoles)
      .HasForeignKey(e => e.UserId);

    builder
      .HasOne(e => e.Role)
      .WithMany(e => e.UserRoles)
      .HasForeignKey(e => e.RoleId);
  }
}

public sealed class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
  public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder) =>
    builder
      .ToTable("UserClaims", SchemaNames.Identity)
      .IsMultiTenant();
}

public sealed class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
  public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder) =>
    builder
      .ToTable("UserLogins", SchemaNames.Identity)
      .IsMultiTenant();
}

public sealed class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
{
  public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder) =>
    builder
      .ToTable("UserTokens", SchemaNames.Identity)
      .IsMultiTenant();
}
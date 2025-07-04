﻿using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.Auditing;
using FSH.WebApi.Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class AuditTrailConfig : IEntityTypeConfiguration<Trail>
{
  public void Configure(EntityTypeBuilder<Trail> builder) =>
    builder
      .ToTable("AuditTrails", SchemaNames.Auditing)
      .IsMultiTenant();
}
# Always do tenant migrations first

# Tenant related entities
dotnet ef migrations add "Tenant_InitialMigration" --project .././Migrators/Migrators.PostgreSQL/ --context TenantDbContext -o Migrations/Tenant
dotnet ef migrations remove --project .././Migrators/Migrators.PostgreSQL/ --context TenantDbContext


# Application related entities
dotnet ef migrations add "App_InitialMigration" --project .././Migrators/Migrators.PostgreSQL/ --context ApplicationDbContext -o Migrations/Application
dotnet ef migrations remove --project .././Migrators/Migrators.PostgreSQL/ --context ApplicationDbContext

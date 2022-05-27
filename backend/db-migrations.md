
# Application related entities
dotnet ef migrations add "_" --project .././Migrators/Migrators.MySQL/ --context ApplicationDbContext -o Migrations/Application
dotnet ef migrations remove --project .././Migrators/Migrators.MySQL/ --context ApplicationDbContext

# Tenant related entities
dotnet ef migrations add "AddTenantSubscriptions" --project .././Migrators/Migrators.MySQL/ --context TenantDbContext -o Migrations/Tenant
dotnet ef migrations remove --project .././Migrators/Migrators.MySQL/ --context TenantDbContext
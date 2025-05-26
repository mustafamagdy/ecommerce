using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence; // For Startup.cs where UseDatabase is defined
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Claims; // Added for Claim and ClaimsPrincipal
using FSH.WebApi.Application.Common.Interfaces; // Added for ICurrentUser and ISerializerService
using FSH.WebApi.Application.Multitenancy.Services;
using Microsoft.Extensions.Options;
using FSH.WebApi.Shared.Multitenancy; // Added for SubscriptionType

namespace FSH.WebApi.Migrators.MSSQL;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get environment variable or use default
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Build configuration
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../Host")) // Corrected path to Host directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var dbSettings = config.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        string? connectionString = dbSettings?.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback or error if no connection string is found.
            // For design time, a dummy string for the correct provider might be enough if schema generation is the goal.
            // However, it's better if it can read the actual one.
            // Let's use a typical local SQL Server connection string as a fallback for design-time.
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=FSH.WebApi.DesignTime;Trusted_Connection=True;MultipleActiveResultSets=true";
            Console.WriteLine($"Warning: ConnectionString not found in appsettings. Using default: {connectionString}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQL Server
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName);
            // Potentially add other SQL Server specific options if needed for migrations
        });

        

        // We need to pass null for dependencies that are normally resolved by DI if ApplicationDbContext constructor requires them.
        // ApplicationDbContext's constructor:
        // ITenantInfo currentTenant, ISubscriptionInfo subscriptionInfo, DbContextOptions options, ICurrentUser currentUser,
        // ISerializerService serializer, ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings,
        // IEventPublisher events, TenantDbContext tenantDb
        // This is complex to mock. A simpler constructor or conditional logic in ApplicationDbContext for design-time might be needed.

        // For now, let's assume a simpler scenario or that EF Core tools can handle this with a basic factory.
        // If ApplicationDbContext cannot be instantiated this way, the error will persist.
        // The error "Unable to create an object of type 'ApplicationDbContext'" suggests this is the core issue.

        // The FSH project's ApplicationDbContext likely needs several services.
        // A common pattern is to have a constructor in ApplicationDbContext that takes only DbContextOptions
        // for migrations, or to fully mock/provide minimal implementations for required services here.

        // Given the complexity, the ideal solution would be if Migrators.MSSQL project could
        // somehow leverage the Host's DI. However, when -s is Migrators.MSSQL, it doesn't run Host's Program.cs.

        // Let's try providing a basic configuration and see if EF Core tools are smart enough or if ApplicationDbContext
        // has a constructor that can work with just options.
        // The actual FSH.WebApi.Infrastructure.Persistence.Context.ApplicationDbContext requires many parameters.
        // This factory will likely fail to create the context if those parameters are not optional or service-provided.

        // The error message "Unable to create an object of type 'ApplicationDbContext'" is often solved by a factory
        // that correctly resolves or mocks dependencies.
        // This factory is a simplified one. If it fails, it means ApplicationDbContext needs more setup.
        // The provided solution structure in FSH typically relies on the Host project for DI.

        // A more robust factory for FSH would need to replicate parts of Host's DI setup for DbContext,
        // or ApplicationDbContext would need a design-time-specific constructor.

        Console.WriteLine($"DesignTimeDbContextFactory: Using environment '{environment}' and connection string for SQL Server (might be default).");
        // Returning a direct instantiation will likely fail due to missing services.
        // The correct way is to use the optionsBuilder to build the options and pass it.
        // However, ApplicationDbContext's constructor needs more than just DbContextOptions.
        // This is the limitation here.

        // The error "Unable to create an object of type 'ApplicationDbContext'" points to this.
        // The factory needs to return `new ApplicationDbContext(optionsBuilder.Options, ...other_dependencies...)`.
        // Let's try to get the connection string and provider right and see.
        // The actual instantiation is done by EF tools using this factory to get options, then it calls the constructor.

        // The error might be because it cannot find a parameterless constructor OR a constructor that it can satisfy.
        // By providing this factory, we tell it how to get DbContextOptions.
        // It will then try to find a constructor on ApplicationDbContext that takes these options.
        // ApplicationDbContext(ITenantInfo currentTenant, ..., DbContextOptions options, ...) is complex.

        // A common pattern for this is to have a specific constructor for design time in ApplicationDbContext
        // #if DEBUG
        // public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        // #endif
        // Or the factory has to mock all those services.

        // For this step, I'm providing the options. The tools will attempt to use them.
        Console.WriteLine($"DesignTimeDbContextFactory: Using environment '{environment}' and connection string for SQL Server.");
        
        return new ApplicationDbContext(
            null, /* ITenantInfo */
            new DesignTimeSubscriptionTypeResolver(), /* ISubscriptionTypeResolver */
            optionsBuilder.Options,
            new DesignTimeCurrentUser(), /* ICurrentUser */
            new DesignTimeSerializerService(), /* ISerializerService */
            null, /* ITenantConnectionStringBuilder */
            Microsoft.Extensions.Options.Options.Create(dbSettings ?? new DatabaseSettings()), /* IOptions<DatabaseSettings> */
            new DesignTimeTenantConnectionStringResolver() /* ITenantConnectionStringResolver */
        );
    }

    // Minimal mock for ICurrentUser for design time
    public class DesignTimeCurrentUser : ICurrentUser
    {
        public string? Name => "DesignTimeUser";
        public Guid GetUserId() => Guid.NewGuid();
        public string? GetUserEmail() => "design@time.user";
        public bool IsAuthenticated() => true;
        public bool IsInRole(string role) => false;
        public IEnumerable<Claim>? GetUserClaims() => null;
        public string? GetTenant() => "root"; // Default or mock tenant
        public void SetUser(ClaimsPrincipal user) { }
        public void SetUserJob(string userId, string tenant) { }
    }

    // Minimal mock for ISerializerService
    public class DesignTimeSerializerService : ISerializerService
    {
        public T Deserialize<T>(string text) => default(T);
        public T Deserialize<T>(string text, Newtonsoft.Json.JsonConverter converter) => default(T);
        public string Serialize<T>(T obj) => string.Empty;
        public string Serialize<T>(T obj, Type type) => string.Empty;
    }
    
    // Minimal mock for ISubscriptionTypeResolver
    public class DesignTimeSubscriptionTypeResolver : ISubscriptionTypeResolver
    {
        public SubscriptionType Resolve() => SubscriptionType.Standard;
    }
    
    // Minimal mock for ITenantConnectionStringResolver
    public class DesignTimeTenantConnectionStringResolver : ITenantConnectionStringResolver
    {
        public string Resolve(string tenantId, SubscriptionType subscriptionType) => string.Empty;
    }
}

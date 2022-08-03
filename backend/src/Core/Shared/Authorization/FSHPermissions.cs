using System.Collections.ObjectModel;

namespace FSH.WebApi.Shared.Authorization;

public static class FSHAction
{
  public const string View = nameof(View);
  public const string Search = nameof(Search);
  public const string Create = nameof(Create);
  public const string Update = nameof(Update);
  public const string Delete = nameof(Delete);
  public const string Activate = nameof(Activate);
  public const string Deactivate = nameof(Deactivate);
  public const string Cancel = nameof(Cancel);
  public const string Export = nameof(Export);
  public const string Generate = nameof(Generate);
  public const string ViewBasic = nameof(ViewBasic);
  public const string Open = nameof(Open);
  public const string Close = nameof(Close);
  public const string Transfer = nameof(Transfer);
  public const string Approve = nameof(Approve);
  public const string ResetPassword = nameof(ResetPassword);
  public const string ViewMy = nameof(ViewMy);
  public const string ViewAdvanced = nameof(ViewAdvanced);

  public const string Clean = nameof(Clean);
  // public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class FSHResource
{
  public const string Tenants = nameof(Tenants);
  public const string Subscriptions = nameof(Subscriptions);
  public const string Dashboard = nameof(Dashboard);
  public const string Hangfire = nameof(Hangfire);
  public const string Users = nameof(Users);
  public const string UserRoles = nameof(UserRoles);
  public const string Roles = nameof(Roles);
  public const string RoleClaims = nameof(RoleClaims);
  public const string Products = nameof(Products);
  public const string Brands = nameof(Brands);
  public const string ServiceCatalog = nameof(ServiceCatalog);
  public const string Services = nameof(Services);
  public const string Orders = nameof(Orders);
  public const string Customers = nameof(Customers);
  public const string Branches = nameof(Branches);
  public const string CashRegisters = nameof(CashRegisters);
}

public static class FSHPermissions
{
  private static readonly FSHPermission[] _all = new FSHPermission[]
  {
    new("View Dashboard", FSHAction.View, FSHResource.Dashboard),
    new("View Hangfire", FSHAction.View, FSHResource.Hangfire),

    // Users
    new("View Users", FSHAction.View, FSHResource.Users),
    new("Search Users", FSHAction.Search, FSHResource.Users),
    new("Create Users", FSHAction.Create, FSHResource.Users),
    new("Update Users", FSHAction.Update, FSHResource.Users),
    new("Delete Users", FSHAction.Delete, FSHResource.Users),
    new("Export Users", FSHAction.Export, FSHResource.Users),
    new("ResetPassword For User", FSHAction.ResetPassword, FSHResource.Users),

    // User Roles
    new("View UserRoles", FSHAction.View, FSHResource.UserRoles),
    new("Update UserRoles", FSHAction.Update, FSHResource.UserRoles),

    // Roles
    new("View Roles", FSHAction.View, FSHResource.Roles),
    new("Create Roles", FSHAction.Create, FSHResource.Roles),
    new("Update Roles", FSHAction.Update, FSHResource.Roles),
    new("Delete Roles", FSHAction.Delete, FSHResource.Roles),

    // Role Claims
    new("View RoleClaims", FSHAction.View, FSHResource.RoleClaims),
    new("Update RoleClaims", FSHAction.Update, FSHResource.RoleClaims),

    // Products
    new("View Products", FSHAction.View, FSHResource.Products, IsBasic: true),
    new("Search Products", FSHAction.Search, FSHResource.Products, IsBasic: true),
    new("Create Products", FSHAction.Create, FSHResource.Products),
    new("Update Products", FSHAction.Update, FSHResource.Products),
    new("Delete Products", FSHAction.Delete, FSHResource.Products),
    new("Export Products", FSHAction.Export, FSHResource.Products),

    // brands
    new("View Brands", FSHAction.View, FSHResource.Brands, IsBasic: true, IsDemo: true),
    new("Search Brands", FSHAction.Search, FSHResource.Brands, IsBasic: true),
    new("Create Brands", FSHAction.Create, FSHResource.Brands),
    new("Update Brands", FSHAction.Update, FSHResource.Brands),
    new("Delete Brands", FSHAction.Delete, FSHResource.Brands),
    new("Generate Brands", FSHAction.Generate, FSHResource.Brands),
    new("Clean Brands", FSHAction.Clean, FSHResource.Brands),

    // Tenants
    new("Search Tenants", FSHAction.Search, FSHResource.Tenants, IsRoot: true),
    new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true),
    new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true),
    new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true),
    new("View Tenant Subscription", FSHAction.View, FSHResource.Subscriptions, IsRoot: true),
    new("Create Tenant Subscription", FSHAction.Create, FSHResource.Subscriptions, IsRoot: true),
    new("Renew Tenant Subscription", FSHAction.Update, FSHResource.Subscriptions, IsRoot: true),
    new("View Basic Tenants Info", FSHAction.ViewBasic, FSHResource.Tenants),
    new("View My Tenant Info", FSHAction.ViewMy, FSHResource.Subscriptions),
    new("View My Tenant History & Payments", FSHAction.ViewAdvanced, FSHResource.Subscriptions),

    // Branches
    new("Search Branches", FSHAction.Search, FSHResource.Branches),
    new("View Branches", FSHAction.View, FSHResource.Branches),
    new("Create Branches", FSHAction.Create, FSHResource.Branches),
    new("Update Branches", FSHAction.Update, FSHResource.Branches),

    // CashRegisters
    new("Search Cash Registers", FSHAction.Search, FSHResource.CashRegisters),
    new("View Cash Registers", FSHAction.View, FSHResource.CashRegisters),
    new("Create Cash Registers", FSHAction.Create, FSHResource.CashRegisters),
    new("Update Cash Registers", FSHAction.Update, FSHResource.CashRegisters),

    // Service Catalogs
    new("View Service Catalog", FSHAction.View, FSHResource.ServiceCatalog),
    new("Search Service Catalog", FSHAction.Search, FSHResource.ServiceCatalog),
    new("Create Service Catalog", FSHAction.Create, FSHResource.ServiceCatalog),
    new("Update Service Catalog", FSHAction.Update, FSHResource.ServiceCatalog),
    new("Delete Service Catalog", FSHAction.Delete, FSHResource.ServiceCatalog),

    // Services
    new("View Services", FSHAction.View, FSHResource.Services),
    new("View Services", FSHAction.Search, FSHResource.Services),
    new("Create Service", FSHAction.Create, FSHResource.Services),
    new("Update Services", FSHAction.Update, FSHResource.Services),
    new("Delete Services", FSHAction.Delete, FSHResource.Services),

    // Orders & Order Items
    new("Search Orders", FSHAction.Search, FSHResource.Orders),
    new("View Orders", FSHAction.View, FSHResource.Orders),
    new("Create Order", FSHAction.Create, FSHResource.Orders),
    new("Update Order", FSHAction.Update, FSHResource.Orders),
    new("Delete Order", FSHAction.Delete, FSHResource.Orders),
    new("Cancel Order", FSHAction.Cancel, FSHResource.Orders),

    // Customers
    new("Search Customers", FSHAction.Search, FSHResource.Customers),
    new("View Customers", FSHAction.View, FSHResource.Customers),
    new("Create Order", FSHAction.Create, FSHResource.Customers),
    new("Update Customer", FSHAction.Update, FSHResource.Customers),
    new("Delete Customer", FSHAction.Delete, FSHResource.Customers),
  };

  public static IReadOnlyList<FSHPermission> All { get; } = new ReadOnlyCollection<FSHPermission>(_all);

  public static IReadOnlyList<FSHPermission> Root { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsRoot).ToArray());

  public static IReadOnlyList<FSHPermission> Admin { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => !p.IsRoot).ToArray());

  public static IReadOnlyList<FSHPermission> Basic { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsBasic).ToArray());

  public static IReadOnlyList<FSHPermission> Demo { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsDemo).ToArray());
}

public record FSHPermission(string Description, string Action, string Resource, bool IsBasic = false,
  bool IsRoot = false, bool IsDemo = false)
{
  public string Name => NameFor(Action, Resource);
  public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
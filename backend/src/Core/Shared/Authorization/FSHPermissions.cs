using System.Collections.ObjectModel;
using FSH.WebApi.Shared.Extensions;

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
  public const string RemoteLogin = nameof(RemoteLogin);
  public const string Pay = nameof(Pay);

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
  private static readonly FSHPermission[] _all =
  {
    new("View Dashboard", FSHAction.View, FSHResource.Dashboard, Roles: new[] { PredefinedRoles.Supervisor, PredefinedRoles.TenantAdmin }),
    new("View Hangfire", FSHAction.View, FSHResource.Hangfire, Roles: new[] { PredefinedRoles.SystemAdmin }),

    // Users
    new("View Users", FSHAction.View, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Search Users", FSHAction.Search, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Create Users", FSHAction.Create, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Update Users", FSHAction.Update, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Delete Users", FSHAction.Delete, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Export Users", FSHAction.Export, FSHResource.Users, Roles: PredefinedRoles.Admins),
    new("Reset Password For User", FSHAction.ResetPassword, FSHResource.Users, Roles: PredefinedRoles.Admins),

    // User Roles
    new("View UserRoles", FSHAction.View, FSHResource.UserRoles, Roles: PredefinedRoles.Admins),
    new("Update UserRoles", FSHAction.Update, FSHResource.UserRoles, Roles: PredefinedRoles.Admins),

    // Roles
    new("View Roles", FSHAction.View, FSHResource.Roles, Roles: PredefinedRoles.Admins),
    new("Create Roles", FSHAction.Create, FSHResource.Roles, Roles: PredefinedRoles.Admins),
    new("Update Roles", FSHAction.Update, FSHResource.Roles, Roles: PredefinedRoles.Admins),
    new("Delete Roles", FSHAction.Delete, FSHResource.Roles, Roles: PredefinedRoles.Admins),

    // Role Claims
    new("View RoleClaims", FSHAction.View, FSHResource.RoleClaims, Roles: PredefinedRoles.Admins),
    new("Update RoleClaims", FSHAction.Update, FSHResource.RoleClaims, Roles: PredefinedRoles.Admins),

    // Products
    new("View Products", FSHAction.View, FSHResource.Products, Roles: PredefinedRoles.All),
    new("Search Products", FSHAction.Search, FSHResource.Products, Roles: PredefinedRoles.All),
    new("Create Products", FSHAction.Create, FSHResource.Products, Roles: PredefinedRoles.Supervisors),
    new("Update Products", FSHAction.Update, FSHResource.Products, Roles: PredefinedRoles.Supervisors),
    new("Delete Products", FSHAction.Delete, FSHResource.Products, Roles: PredefinedRoles.Supervisors),
    new("Export Products", FSHAction.Export, FSHResource.Products, Roles: PredefinedRoles.Supervisors),

    // brands
    new("View Brands", FSHAction.View, FSHResource.Brands, Roles: PredefinedRoles.All),
    new("Search Brands", FSHAction.Search, FSHResource.Brands, Roles: PredefinedRoles.All),
    new("Create Brands", FSHAction.Create, FSHResource.Brands, Roles: PredefinedRoles.Supervisors),
    new("Update Brands", FSHAction.Update, FSHResource.Brands, Roles: PredefinedRoles.Supervisors),
    new("Delete Brands", FSHAction.Delete, FSHResource.Brands, Roles: PredefinedRoles.Supervisors),

    // Tenants
    new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("Search Tenants", FSHAction.Search, FSHResource.Tenants, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("View Tenant Subscription", FSHAction.View, FSHResource.Subscriptions, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("Create Tenant Subscription", FSHAction.Create, FSHResource.Subscriptions, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("Renew Tenant Subscription", FSHAction.Update, FSHResource.Subscriptions, IsRoot: true, Roles: PredefinedRoles.RootAdmins),
    new("View Basic Tenants Info", FSHAction.ViewBasic, FSHResource.Tenants, Roles: PredefinedRoles.All),
    new("View My Tenant Info", FSHAction.ViewMy, FSHResource.Subscriptions, Roles: PredefinedRoles.Supervisors),
    new("View My Tenant History & Payments", FSHAction.ViewAdvanced, FSHResource.Subscriptions, PredefinedRoles.Supervisors),
    new("Root admin can remote login to tenant db to provide support", FSHAction.RemoteLogin, FSHResource.Tenants, Roles: new[] { PredefinedRoles.RootAdmin }),
    new("Root admin can reset other tenant's user password to provide support", FSHAction.ResetPassword, FSHResource.Tenants, Roles: new[] { PredefinedRoles.RootAdmin }),

    // Branches
    new("Search Branches", FSHAction.Search, FSHResource.Branches, Roles: PredefinedRoles.All),
    new("View Branches", FSHAction.View, FSHResource.Branches, Roles: PredefinedRoles.All),
    new("Create Branches", FSHAction.Create, FSHResource.Branches, Roles: PredefinedRoles.Admins),
    new("Update Branches", FSHAction.Update, FSHResource.Branches, Roles: PredefinedRoles.Admins),
    new("Activate Branches", FSHAction.Activate, FSHResource.Branches, Roles: PredefinedRoles.Admins),
    new("Deactivate Branches", FSHAction.Deactivate, FSHResource.Branches, Roles: PredefinedRoles.Admins),

    // CashRegisters
    new("Search Cash Registers", FSHAction.Search, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("View Cash Registers", FSHAction.View, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("View Cash Registers Basic Info", FSHAction.ViewBasic, FSHResource.CashRegisters, Roles: PredefinedRoles.All),
    new("Create Cash Registers", FSHAction.Create, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Update Cash Registers", FSHAction.Update, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Open Cash Register", FSHAction.Open, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Close Cash Registers", FSHAction.Close, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Transfer amount between cash registers", FSHAction.Transfer, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Approve Cash Registers transfer", FSHAction.Approve, FSHResource.CashRegisters, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),

    // Service Catalogs
    new("View Service Catalog", FSHAction.View, FSHResource.ServiceCatalog, Roles: PredefinedRoles.All),
    new("Search Service Catalog", FSHAction.Search, FSHResource.ServiceCatalog, Roles: PredefinedRoles.All),
    new("Create Service Catalog", FSHAction.Create, FSHResource.ServiceCatalog, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Update Service Catalog", FSHAction.Update, FSHResource.ServiceCatalog, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Delete Service Catalog", FSHAction.Delete, FSHResource.ServiceCatalog, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),

    // Services
    new("View Services", FSHAction.View, FSHResource.Services, Roles: PredefinedRoles.All),
    new("View Services", FSHAction.Search, FSHResource.Services, Roles: PredefinedRoles.All),
    new("Create Service", FSHAction.Create, FSHResource.Services, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Update Services", FSHAction.Update, FSHResource.Services, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Delete Services", FSHAction.Delete, FSHResource.Services, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),

    // Orders & Order Items
    new("Search Orders", FSHAction.Search, FSHResource.Orders, Roles: PredefinedRoles.All),
    new("View Orders", FSHAction.View, FSHResource.Orders, Roles: PredefinedRoles.All),
    new("Create Order", FSHAction.Create, FSHResource.Orders, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Update Order", FSHAction.Update, FSHResource.Orders, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Delete Order", FSHAction.Delete, FSHResource.Orders, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Cancel Order", FSHAction.Cancel, FSHResource.Orders, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Pay for order", FSHAction.Pay, FSHResource.Orders, Roles: PredefinedRoles.All),

    // Customers
    new("Search Customers", FSHAction.Search, FSHResource.Customers, Roles: PredefinedRoles.All),
    new("View Customers", FSHAction.View, FSHResource.Customers, Roles: PredefinedRoles.All),
    new("Create Order", FSHAction.Create, FSHResource.Customers, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Update Customer", FSHAction.Update, FSHResource.Customers, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
    new("Delete Customer", FSHAction.Delete, FSHResource.Customers, Roles: PredefinedRoles.Supervisors.And(PredefinedRoles.Admins)),
  };

  public static IReadOnlyCollection<FSHPermission> All { get; } = new ReadOnlyCollection<FSHPermission>(_all);

  public static IReadOnlyCollection<FSHPermission> Root { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsRoot).ToArray());

  public static IReadOnlyCollection<FSHPermission> Admin { get; } =
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => !p.IsRoot).ToArray());

  public static IReadOnlyCollection<FSHPermission> ForPredefinedRole(string role) =>
    new ReadOnlyCollection<FSHPermission>(_all.Where(p => (bool)p.Roles?.Contains(role)).ToArray());
}

public record FSHPermission(string Description, string Action, string Resource, string[] Roles,
  bool IsRoot = false, bool IsDemo = false)
{
  public string Name => NameFor(Action, Resource);
  public static string NameFor(string action, string resource) => $"{resource}.{action}".ToLower();
}
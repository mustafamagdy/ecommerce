using System.Security.Cryptography.X509Certificates;

namespace FSH.WebApi.Shared.Authorization;

public static class PredefinedRoles
{
  public static string SystemAdmin = "system_admin";
  public static string RootAdmin = "root_admin";
  public static string TenantAdmin = "admin";
  public static string Cashier = "cashier";
  public static string Supervisor = "supervisor";

  public static string[] RootAdmins = { RootAdmin };
  public static string[] Admins = { RootAdmin, TenantAdmin };
  public static string[] All = { SystemAdmin, RootAdmin, TenantAdmin, Cashier, Supervisor };
  public static string[] Supervisors = { Supervisor };
}
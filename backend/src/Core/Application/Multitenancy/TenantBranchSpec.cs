using FSH.WebApi.Domain.Structure;

namespace FSH.WebApi.Application.Multitenancy;

public class TenantBranchSpec : Specification<Branch>
{
  public TenantBranchSpec(string tenantId) => Query.Where(a => a.TenantId == tenantId);
}
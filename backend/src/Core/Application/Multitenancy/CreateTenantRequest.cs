namespace FSH.WebApi.Application.Multitenancy;

public class CreateTenantRequest : IRequest<string>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? DatabaseName { get; set; }
    public string AdminEmail { get; set; } = default!;
    public string? Issuer { get; set; }

    public string? PhoneNumber { get; set; }
    public string? VatNo { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AdminName { get; set; }
    public string? AdminPhoneNumber { get; set; }
    public string? TechSupportUserId { get; set; }
}

public class CreateTenantRequestHandler : IRequestHandler<CreateTenantRequest, string>
{
    private readonly ITenantService _tenantService;

    public CreateTenantRequestHandler(ITenantService tenantService) => _tenantService = tenantService;

    public Task<string> Handle(CreateTenantRequest request, CancellationToken cancellationToken) =>
        _tenantService.CreateAsync(request, cancellationToken);
}
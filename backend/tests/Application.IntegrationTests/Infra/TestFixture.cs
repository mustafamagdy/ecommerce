using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Shared.Multitenancy;
using netDumbster.smtp;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Infra;

[Collection(nameof(TestConstants.WebHostTests))]
// public abstract class TestFixture : IClassFixture<HostFixture>
public abstract class TestFixture : IAsyncLifetime
{
  private readonly HostFixture _host;
  private readonly HttpClient _client;
  protected readonly Faker _faker = new();
  protected readonly ITestOutputHelper _output;
  protected static string RootAdminPassword = "123Pa$$word!";
  protected static string RootAdminEmail = "admin@root.com";

  protected TestFixture(HostFixture host, ITestOutputHelper output)
  {
    _host = host;
    _output = output;

    _client = _host.CreateClient();
    _output.WriteLine("New http client created");
    _host.MessageReceived += HostOnMessageReceived;
  }

  protected TaskCompletionSource<SmtpMessage>? MailReceivedTask;

  private void HostOnMessageReceived(object? sender, MessageReceivedArgs e)
  {
    MailReceivedTask?.SetResult(e.Message);
  }

  private Task<HttpResponseMessage> SendAsJsonAsync<TValue>(HttpMethod method, string requestUri, TValue? value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _client.DefaultRequestHeaders.Add(key, val);
    }

    var message = new HttpRequestMessage
    {
      Method = method,
      RequestUri = new Uri(requestUri),
    };

    if (value != null)
    {
      message.Content = JsonContent.Create(value);
    }

    return _client.SendAsync(message, cancellationToken);
  }

  protected Task<HttpResponseMessage> DeleteAsJsonAsync<TValue>(string requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    => SendAsJsonAsync(HttpMethod.Delete, requestUri, value, headers, cancellationToken);

  protected Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    => SendAsJsonAsync(HttpMethod.Post, requestUri, value, headers, cancellationToken);

  protected Task<HttpResponseMessage> PutAsJsonAsync<TValue>(string requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    => SendAsJsonAsync(HttpMethod.Put, requestUri, value, headers, cancellationToken);

  protected Task<HttpResponseMessage> GetAsync(string requestUri, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    => SendAsJsonAsync(HttpMethod.Get, requestUri, (object)null, headers, cancellationToken);

  protected Task<HttpResponseMessage> RootAdmin_PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
    => RootAdmin_PostAsJsonAsync(requestUri, (object)value, headers, cancellationToken);

  protected async Task<HttpResponseMessage> RootAdmin_PostAsJsonAsync(string? requestUri, object value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await PostAsJsonAsync(requestUri, value, headers, cancellationToken);
  }

  protected async Task<HttpResponseMessage> RootAdmin_PutAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await PutAsJsonAsync(requestUri, value, headers, cancellationToken);
  }

  protected async Task<HttpResponseMessage> RootAdmin_GetAsync(string? requestUri, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await GetAsync(requestUri, headers, cancellationToken);
  }

  protected async Task<(Dictionary<string, string> Headers, Guid BranchId)> CreateTenantAndLogin()
  {
    var tenantId = PrepareNewTenant(out string adminEmail, out var tenant, false);

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantResult = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    var tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId);
    tenantAdminLoginHeaders.Should().NotBeNullOrEmpty();

    _ = await GetAsync("/api/v1/my/", tenantAdminLoginHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantInfo = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantInfo.Should().NotBeNull();
    tenantInfo.Branches.Should().NotBeEmpty();
    var defaultBranch = tenantInfo.Branches.First();

    tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId, defaultBranch.Id);
    tenantAdminLoginHeaders.Should().NotBeNullOrEmpty();

    return (tenantAdminLoginHeaders, defaultBranch.Id);
  }

  protected async Task<List<BasicUserDataDto>> GetUserList(Dictionary<string, string> headers)
  {
    var _ = await GetAsync("/api/users/basic", headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    return await _.Content.ReadFromJsonAsync<List<BasicUserDataDto>>();
  }

  protected Task<HttpResponseMessage> TryLoginAs(string username, string password, string? tenant,
    Guid? branchId = null, SubscriptionType? subscriptionType = default, CancellationToken cancellationToken = default)
  {
    var tenantHeader = tenant != null ? new Dictionary<string, string> { { "tenant", tenant } } : new Dictionary<string, string> { { "tenant", "root" } };
    tenantHeader.Add(MultitenancyConstants.SubscriptionTypeHeaderName, subscriptionType ?? SubscriptionType.Standard);
    return PostAsJsonAsync("/api/tokens", new TokenRequest(username, password, branchId), tenantHeader, cancellationToken);
  }

  protected async Task<Dictionary<string, string>> LoginAs(string username, string password,
    Dictionary<string, string>? headers, string? tenant, Guid? branchId = null,
    SubscriptionType? subscriptionType = default,
    CancellationToken cancellationToken = default)
  {
    var response = await TryLoginAs(username, password, tenant, branchId, subscriptionType, cancellationToken);
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    _output.WriteLine("Token is " + tokenResult.Token);

    headers ??= new Dictionary<string, string>();

    headers.Add("Authorization", $"Bearer {tokenResult.Token}");
    headers.Add(MultitenancyConstants.SubscriptionTypeHeaderName, SubscriptionType.Standard);

    return headers;
  }

  private Task<Dictionary<string, string>> LoginAsRootAdmin(Dictionary<string, string> headers, CancellationToken cancellationToken)
  {
    return LoginAs(RootAdminEmail, RootAdminPassword, headers, "root", null, SubscriptionType.Standard, cancellationToken);
  }

  protected string PrepareNewTenant(out string adminEmail, out CreateTenantRequest tenant, bool hasDemo = true)
  {
    var tenantId = Guid.NewGuid().ToString();
    adminEmail = $"admin@{tenantId}.com";

    var technicalSupportEngId = _rootTenantUsers.First().Id.ToString();

    tenant = new CreateTenantRequest
    {
      Id = tenantId,
      ProdPackageId = _packages.First().Id,
      DemoPackageId = hasDemo ? _packages.First().Id : null,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      TechSupportUserId = technicalSupportEngId
    };
    return tenantId;
  }

  protected List<SubscriptionPackageDto>? _packages;
  protected List<UserDetailsDto>? _rootTenantUsers;

  public async Task InitializeAsync()
  {
    Randomizer.Seed = new Random(1234);

    var _ = await GetAsync("/api/tenants/packages", new Dictionary<string, string>());
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    _packages = await _.Content.ReadFromJsonAsync<List<SubscriptionPackageDto>>();

    var headers = new Dictionary<string, string>();
    var adminHeaders = await LoginAsRootAdmin(headers, CancellationToken.None);
    _ = await GetAsync("/api/users", adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    _rootTenantUsers = await _.Content.ReadFromJsonAsync<List<UserDetailsDto>>();
    _rootTenantUsers.Should().NotBeNullOrEmpty();
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  protected async Task openCashRegister(Guid cashRegisterId, Dictionary<string, string> adminHeaders)
  {
    var _ = await PostAsJsonAsync("/api/v1/cashRegister/open", new OpenCashRegisterRequest()
    {
      Id = cashRegisterId
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  protected async Task<Guid> createNewCashRegister(Guid branchId, List<BasicUserDataDto> users, Dictionary<string, string> adminHeaders)
  {
    HttpResponseMessage _;
    _ = await PostAsJsonAsync("/api/v1/cashRegister", new CreateCashRegisterRequest()
    {
      Name = Guid.NewGuid().ToString(),
      BranchId = branchId,
      ManagerId = users.First().Id,
      Color = "red"
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var cashRegisterId = await _.Content.ReadFromJsonAsync<Guid>();
    return cashRegisterId;
  }
}
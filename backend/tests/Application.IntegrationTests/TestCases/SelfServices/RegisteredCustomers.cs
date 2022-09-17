using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.SelfServices;

public class RegisteredCustomers : TestFixture
{
  public RegisteredCustomers(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
- as a registered user I can login, logout, reset password
- as a registered user I can see and update my profile
- as a registered user I can create orders with different payment methods
- as a registered user I can see my orders and follow up my order's status
- as a registered user I should receive notifications when my order status updated
   */
}
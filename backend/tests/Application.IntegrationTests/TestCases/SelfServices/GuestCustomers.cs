using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.SelfServices;

public class GuestCustomers : TestFixture
{
  public GuestCustomers(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
- as a guest I can browse the different service catalog
- as a guest I can choose different items from service catalog and create an online order with either cash on delivery, card, or apple pay
- as a guest I can register with my phone number + OTP, then fill in my profile
   */
}
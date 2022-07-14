using Xunit;

namespace Application.IntegrationTests.Infra;

[CollectionDefinition(nameof(TestConstants.WebHostTests))]
public class WebHostCollectionFixture : ICollectionFixture<HostFixture>
{
}
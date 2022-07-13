using Xunit;

namespace Application.IntegrationTests;

[CollectionDefinition(nameof(TestConstants.WebHostTests))]
public class WebHostCollectionFixture : ICollectionFixture<HostFixture>
{
}
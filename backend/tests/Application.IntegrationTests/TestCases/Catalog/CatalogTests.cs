using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Catalog;

public class CatalogTests : TestFixture
{
  public CatalogTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   * - brands
   * can_create_brand_when_submit_valid_data_and_has_permission
   * can_list_brand_when__has_permission
   * can_update_brand_when_submit_valid_data_and_has_permission
   * can_delete_brand_when_brand_not_used_and_has_permission
   * cannot_delete_brand_when_brand_is_used_and_has_permission
   * - products
   * can_create_product_when_submit_valid_data_and_has_permission
   * can_list_product_when__has_permission
   * can_update_product_when_submit_valid_data_and_has_permission
   * can_delete_product_when_product_not_used_and_has_permission
   * cannot_delete_product_when_product_is_used_and_has_permission
   * - services
   * can_create_service_when_submit_valid_data_and_has_permission
   * can_list_service_when__has_permission
   * can_update_service_when_submit_valid_data_and_has_permission
   * can_delete_service_when_service_not_used_and_has_permission
   * cannot_delete_service_when_service_is_used_and_has_permission
   * - service catalogs
   * can_create_service_catalog_when_submit_valid_data_and_has_permission
   * can_list_service_catalog_when__has_permission
   * can_update_service_catalog_when_submit_valid_data_and_has_permission
   * can_delete_service_catalog_when_service_catalog_not_used_and_has_permission
   * cannot_delete_service_catalog_when_service_catalog_is_used_and_has_permission
   */
}
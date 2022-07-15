using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Operations;

public class OperationsTests : TestFixture
{
  public OperationsTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   * - cash_register
   * ? can_create_only_one_cash_register_per_branch
   * get_default_cash_register_for_branch
   * cannot_do_any_operations_on_closed_cash_register
   * can_open_cash_register
   * can_close_cash_register
   * any_payment_should_have_record_in_cash_register
   * cash_register_active_operations_should_be_empty_when_open_cash_register
   * cash_register_active_operations_should_be_empty_when_close_cash_register
   * active_operations_should_moved_to_archived_operations_when_close_cash_register
   * can_transfer_between_cash_register
   * det_cash_register_had_to_accept_transfer_in_order_to_appear_in_its_balance
   * - customers
   * - orders
   * - payments
   */
}
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using Mapster;

namespace FSH.WebApi.Application.Mappings;

public class DtoCustomMapping
{
  public static void Configure()
  {
    TypeAdapterConfig<Order, OrderDto>
      .NewConfig()
      .Map(dest => dest.OrderDate, src => src.OrderDate.ToString("dd/MM/yyyy"))
      .Map(dest => dest.OrderTime, src => src.OrderDate.ToString("HH:mm:ss"))
      .Map(dest => dest.PhoneNumber, src => src.Customer.PhoneNumber)
      ;

    TypeAdapterConfig<Order, OrderSummaryDto>
      .NewConfig();

    TypeAdapterConfig<Order, OrderExportDto>
      .ForType()
      .Map(dest => dest.Base64QrCode, src => src.QrCodeBase64)
      .Map(dest => dest.PhoneNumber, src => src.Customer.PhoneNumber);

    TypeAdapterConfig<TenantProdSubscription, ProdTenantSubscriptionDto>
      .NewConfig()
      .Map(dest => dest.SubscriptionId, src => src.Id)
      .Map(dest => dest.History, src => src.History)
      ;

    TypeAdapterConfig<TenantProdSubscription, ProdTenantSubscriptionWithPaymentDto>
      .NewConfig()
      .Map(dest => dest.SubscriptionId, src => src.Id)
      .Map(dest => dest.History, src => src.History);

    TypeAdapterConfig<ActivePaymentOperation, CashRegisterOperationDto>
      .NewConfig()
      .Map(dest => dest.PaymentMethodName, src => src.PaymentMethod.Name)
      .Map(dest => dest.PaymentOperationType, src => src.OperationType.Name);

    TypeAdapterConfig<ArchivedPaymentOperation, CashRegisterOperationDto>
      .NewConfig()
      .Map(dest => dest.PaymentMethodName, src => src.PaymentMethod.Name)
      .Map(dest => dest.PaymentOperationType, src => src.OperationType.Name);

    TypeAdapterConfig<OrderPayment, OrderPaymentDto>
      .NewConfig()
      .Map(dest => dest.PaymentMethodName, src => src.PaymentMethod.Name)
      .Map(dest => dest.PaymentId, src => src.Id);

    TypeAdapterConfig<ApplicationUser, UserDetailsDto>
      .NewConfig()
      .Map(dest => dest.Roles, src => src.UserRoles.Select(a => a.Role.Name.ToLower()));

    TypeAdapterConfig<Order, BasicOrderDto>
      .NewConfig()
      .Map(dest => dest.Amount, src => src.NetAmount)
      .Map(dest => dest.Due, src => src.NetAmount - src.TotalPaid);

    TypeAdapterConfig<FSHTenantInfo, BasicTenantInfoDto>
      .NewConfig()
      .Map(dest => dest.TotalDue, src =>
        src.ProdSubscription.History.DefaultIfEmpty().Sum(a => (decimal?)a.Price) ?? 0
        - src.ProdSubscription.Payments.DefaultIfEmpty().Sum(a => (decimal?)a.Amount) ?? 0)
      .Map(dest => dest.TotalPaid, src =>
        src.ProdSubscription.Payments.DefaultIfEmpty().Sum(a => (decimal?)a.Amount) ?? 0);
  }
}
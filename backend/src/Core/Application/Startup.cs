using System.Reflection;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.Operation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Application;

public static class Startup
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    var assembly = Assembly.GetExecutingAssembly();
    DtoCustomMapping.Configure();
    return services
      .AddValidatorsFromAssembly(assembly)
      .AddMediatR(assembly);
  }
}

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

    TypeAdapterConfig<Order, OrderExportDto>
      .NewConfig()
      .Map(dest => dest.Base64QrCode, src => src.QrCodeBase64)
      .Map(dest => dest.PhoneNumber, src => src.Customer.PhoneNumber)
      ;
  }
}
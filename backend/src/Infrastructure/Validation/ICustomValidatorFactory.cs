using FluentValidation;

namespace FSH.WebApi.Infrastructure.Validation;

public interface ICustomValidatorFactory
{
  IValidator GetValidatorFor(Type type);
}
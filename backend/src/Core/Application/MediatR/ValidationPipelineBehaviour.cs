using IValidator = FluentValidation.IValidator;

namespace FSH.WebApi.Application.MediatR;

public class ValidationPipelineBehaviour<TRequest, TResponse> :
  IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>

{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  public ValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    if (!_validators.Any())
    {
      return await next();
    }

    var failures = _validators
      .Select(async validator => await validator.ValidateAsync(request, cancellationToken))
      .SelectMany(task => task.Result.Errors)
      .Where(failure => failure is not null)
      .Distinct()
      .ToArray();

    if (failures.Any())
    {
      throw new ValidationException(failures);
    }

    return await next();
  }
}
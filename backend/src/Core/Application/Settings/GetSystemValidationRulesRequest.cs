namespace FSH.WebApi.Application.Settings;

public class ValidationRuleDto
{

}

public class GetSystemValidationRulesRequest : IRequest<ValidationRuleDto>
{
}

public class GetSystemValidationRulesRequestHandler : RequestHandler<GetSystemValidationRulesRequest, ValidationRuleDto>
{
  protected override ValidationRuleDto Handle(GetSystemValidationRulesRequest request)
  {
    return null!;
  }
}
using MediatR.Wrappers;

namespace FSH.WebApi.Application.Settings;

public class ValidationRuleDto
{
}

public class GetSystemValidationRulesRequest : IRequest<ValidationRuleDto>
{
}

public class GetSystemValidationRulesRequestHandler : RequestHandlerWrapperImpl<GetSystemValidationRulesRequest,
        ValidationRuleDto>
{
}
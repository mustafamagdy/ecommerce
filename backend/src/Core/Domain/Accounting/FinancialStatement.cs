using FSH.WebApi.Domain.Common.Contracts;
using System;

namespace FSH.WebApi.Domain.Accounting;

public class FinancialStatement : AuditableEntity, IAggregateRoot
{
    public FinancialStatementType StatementType { get; private set; }
    public DateTime PeriodStartDate { get; private set; }
    public DateTime PeriodEndDate { get; private set; }
    public DateTime GeneratedDate { get; private set; }
    public string Content { get; private set; } = default!; // Likely JSON

    public FinancialStatement(FinancialStatementType statementType, DateTime periodStartDate, DateTime periodEndDate, DateTime generatedDate, string content)
    {
        StatementType = statementType;
        PeriodStartDate = periodStartDate;
        PeriodEndDate = periodEndDate;
        GeneratedDate = generatedDate;
        Content = content;
    }

    public FinancialStatement Update(FinancialStatementType? statementType, DateTime? periodStartDate, DateTime? periodEndDate, DateTime? generatedDate, string? content)
    {
        if (statementType.HasValue && StatementType != statementType.Value) StatementType = statementType.Value;
        if (periodStartDate.HasValue && PeriodStartDate != periodStartDate.Value) PeriodStartDate = periodStartDate.Value;
        if (periodEndDate.HasValue && PeriodEndDate != periodEndDate.Value) PeriodEndDate = periodEndDate.Value;
        if (generatedDate.HasValue && GeneratedDate != generatedDate.Value) GeneratedDate = generatedDate.Value;
        if (content is not null && Content?.Equals(content) is not true) Content = content;
        return this;
    }
}

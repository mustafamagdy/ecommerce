using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For Account
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.Accounts.Specifications;

public class AccountsByIdsSpec : Specification<Account>
{
    public AccountsByIdsSpec(IEnumerable<Guid> ids)
    {
        Query.Where(acc => ids.Contains(acc.Id));
    }
}

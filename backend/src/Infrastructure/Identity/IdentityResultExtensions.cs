using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Infrastructure.Identity;

internal static class IdentityResultExtensions
{
  public static ReadOnlyCollection<string> GetErrors(this IdentityResult result, IStringLocalizer T) =>
    result.Errors.Select(e => T[e.Description].ToString()).ToList().AsReadOnly();
}
namespace FSH.WebApi.Application.Operation.CurrentBranchs;

public interface ICurrentBranchResolver : ITransientService
{
  Guid Resolve(object context);
}
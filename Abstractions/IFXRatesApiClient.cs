using FXChangeWebAPI.Domain.Common;

namespace FXChangeWebAPI.Abstractions;

public interface IFXRatesApiClient
{
    Task<FResult<TEntity>> GetEntityAsync<TEntity>(string Url);
}

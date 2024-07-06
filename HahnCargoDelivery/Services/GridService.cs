using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Services;

public interface IGridService
{
    public Task<Grid> GetGrid();
}

public class GridService: IGridService
{
    private readonly IExternalApiService _externalApiService;
    private readonly HahnCargoSimApiConfig _hahnCargoSimApiConfig;

    public GridService(IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
    {
        if (hahnCargoSimApiConfig.Value == null)
        {
            throw new Exception("Configs can't be null");
        }
        _externalApiService = externalApiService;
        _hahnCargoSimApiConfig = hahnCargoSimApiConfig.Value;
    }

    public async Task<Grid> GetGrid()
    {
        var response = await _externalApiService.GetAsync<Grid>(_hahnCargoSimApiConfig.Uri + "Grid/Get");
        if (response == null)
        {
            throw new Exception("Login response can't be null!");
        }

        return response;
    }
}
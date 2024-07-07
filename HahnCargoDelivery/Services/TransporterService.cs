using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Services;

public interface ITransporterService
{
    public Task<int> Buy(int positionNodeId);
    public Task<CargoTransporter> Get(int transporterId);
    public Task Move(int transporterId, int targetNodeId);
}
public class TransporterService(IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig): ITransporterService
{
    public async Task<int> Buy(int positionNodeId)
    {
        var transporterId = await externalApiService.PostAsync<int>(
            hahnCargoSimApiConfig.Value.Uri + $"CargoTransporter/Buy?positionNodeId={positionNodeId}", null);
        return transporterId;
    }

    public async Task<CargoTransporter> Get(int transporterId)
    {
        var cargoTransporter = await externalApiService.GetAsync<CargoTransporter>(
            hahnCargoSimApiConfig.Value.Uri + $"CargoTransporter/Get?transporterId={transporterId}");
        if (cargoTransporter == null)
        {
            throw new Exception("Cargo transporter can't be null");
        }

        return cargoTransporter;
    }

    public async Task Move(int transporterId, int targetNodeId)
    {
        await externalApiService.PutAsync(
            hahnCargoSimApiConfig.Value.Uri + $"CargoTransporter/Move?transporterId={transporterId}&targetNodeId={targetNodeId}", null);
    }
}
using HahnCargoDelivery.Models;
using HahnCargoDelivery.Services;
using Microsoft.AspNetCore.Mvc;

namespace HahnCargoDelivery.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SimController(SimulationService simulationService)
{
    
    [HttpGet]
    public ActionResult<SimulationState> GetSimulationState()
    {
        return simulationService.GetSimulationState();
    }
}
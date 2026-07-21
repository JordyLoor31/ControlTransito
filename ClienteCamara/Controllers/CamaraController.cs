using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ClienteCamara.Controllers;

[ApiController]
[Route("api/camara")]
public class CamaraController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public CamaraController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    [HttpPost("simular")]
    public async Task<IActionResult> Simular()
    {
        var client = _factory.CreateClient();

        var infraccion = new
        {
            placa = $"ABC{Random.Shared.Next(1000,9999)}",
            velocidad = Random.Shared.Next(70,120),
            limiteVelocidad = 50,
            fechaDeteccion = DateTime.UtcNow
        };

        await client.PostAsJsonAsync(
            "https://localhost:7036/api/infracciones",
            infraccion);

        return Ok(infraccion);
    }
}
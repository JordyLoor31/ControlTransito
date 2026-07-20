using ApiMultas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMultas.Controllers;

[ApiController]
[Route("api/multas")]
public class MultasController : ControllerBase
{
    private readonly AppDbContext _context;

    public MultasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var multas = await _context.Multas
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync();

        return Ok(multas);
    }

    [HttpGet("{placa}")]
    public async Task<IActionResult> ObtenerPorPlaca(
        string placa)
    {
        var multas = await _context.Multas
            .Where(x => x.Placa == placa)
            .ToListAsync();

        return Ok(multas);
    }
}
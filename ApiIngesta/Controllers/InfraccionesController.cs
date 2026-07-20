using ApiIngesta.Data;
using ApiIngesta.Models;
using ApiIngesta.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using System.Text.Json;

namespace ApiIngesta.Controllers;

[ApiController]
[Route("api/infracciones")]
public class InfraccionesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ServiceBusProducer _producer;

    public InfraccionesController(
        AppDbContext context,
        ServiceBusProducer producer)
    {
        _context = context;
        _producer = producer;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        Infraccion infraccion)
    {
        // Generar Id automáticamente
        infraccion.Id = Guid.NewGuid();

        _context.Infracciones.Add(infraccion);

        await _context.SaveChangesAsync();

        var evento =
            new InfraccionDetectadaEvent(
                infraccion.Id,
                infraccion.Placa,
                infraccion.Velocidad,
                infraccion.LimiteVelocidad,
                infraccion.FechaDeteccion);

        var json =
            JsonSerializer.Serialize(evento);

        try
        {
            await _producer.SendAsync(json);

            Console.WriteLine(
                "MENSAJE ENVIADO A SERVICE BUS");
        }
        catch (Exception ex)
        {
            var pendiente = new MensajePendiente
            {
                Id = Guid.NewGuid(),
                Payload = json,
                FechaCreacion = DateTime.UtcNow,
                Procesado = false
            };

            _context.MensajesPendientes.Add(
                pendiente);

            await _context.SaveChangesAsync();

            Console.WriteLine(
                $"MENSAJE GUARDADO COMO PENDIENTE: {ex.Message}");
        }

        return Ok(new
        {
            mensaje = "Infracción creada correctamente",
            infraccion.Id
        });
    }

}
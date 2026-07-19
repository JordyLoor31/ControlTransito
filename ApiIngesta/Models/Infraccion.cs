namespace ApiIngesta.Models;

public class Infraccion
{
    public Guid Id { get; set; }

    public string Placa { get; set; } = string.Empty;

    public decimal Velocidad { get; set; }

    public decimal LimiteVelocidad { get; set; }

    public DateTime FechaDeteccion { get; set; }
}
namespace ApiIngesta.Models;

public class MensajePendiente
{
    public Guid Id { get; set; }

    public string Payload { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public bool Procesado { get; set; }
}
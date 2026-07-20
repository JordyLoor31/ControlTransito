namespace ApiMultas.Models;

public class Multa
{
    public Guid Id { get; set; }

    public string Placa { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateTime FechaEmision { get; set; }

    public bool Pagada { get; set; }
}
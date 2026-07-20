namespace Shared.Contracts;

public record InfraccionDetectadaEvent(
    Guid Id,
    string Placa,
    decimal Velocidad,
    decimal LimiteVelocidad,
    DateTime FechaDeteccion
);
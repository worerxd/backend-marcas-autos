namespace MarcasAutos.Api.Models;

public class MarcaAuto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string PaisOrigen { get; set; } = string.Empty;
}
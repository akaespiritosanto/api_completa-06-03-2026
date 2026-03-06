namespace criacao_api4.Models;

public class BandWithCds : Band
{
    public List<Cd> cds { get; set; } = new();
}

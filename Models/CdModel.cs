namespace criacao_api4.Models;

public class Cd
{
    public int cdId { get; set; }
    public string name { get; set; } = string.Empty;
    public int bandId { get; set; }
    public int rating { get; set; }
}

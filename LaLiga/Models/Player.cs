namespace LaLiga.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
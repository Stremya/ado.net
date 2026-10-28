using Microsoft.EntityFrameworkCore;

namespace LaLiga.Models;

[Keyless]
public class TeamStanding
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int TotalScored { get; set; }
    public int TotalConceded { get; set; }
    public int TotalPoints { get; set; }
}
using Microsoft.EntityFrameworkCore;

namespace LaLiga.Models;

[Keyless]
public class PlayerStat
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int TotalGoals { get; set; }
}
namespace LaLiga.Models;

public class Match
{
    public int Id { get; set; }
    public DateTime MatchDate { get; set; }
    public int HomeTeamGoals { get; set; }
    public int AwayTeamGoals { get; set; }

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
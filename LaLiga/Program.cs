using Microsoft.EntityFrameworkCore;
using LaLiga.Data;
using LaLiga.Models;

using var context = new LaLigaContext();

// ============================================
// ЗАДАНИЕ 1: Генерация случайных матчей
// ============================================
Console.WriteLine("=== ЗАДАНИЕ 1: Генерация матчей ===");
for (int i = 0; i < 10; i++)
{
    await context.Database.ExecuteSqlRawAsync("EXEC GenerateRandomMatches");
}
Console.WriteLine("Сгенерировано 10 матчей!");

// ============================================
// ЗАДАНИЕ 2: Бомбардиры
// ============================================
Console.WriteLine("\n=== ЗАДАНИЕ 2: Бомбардиры ===");

int targetTeamId = 1; // Реал Мадрид

// Топ-3 бомбардиров конкретной команды
var top3TeamScorers = await context.PlayerStats
    .Where(ps => ps.TeamId == targetTeamId)
    .OrderByDescending(ps => ps.TotalGoals)
    .Take(3)
    .ToListAsync();

Console.WriteLine($"Топ-3 бомбардиров команды (TeamId={targetTeamId}):");
foreach (var s in top3TeamScorers)
    Console.WriteLine($"  {s.PlayerName} - {s.TotalGoals} голов");

// Лучший бомбардир конкретной команды
var bestTeamScorer = await context.PlayerStats
    .Where(ps => ps.TeamId == targetTeamId)
    .OrderByDescending(ps => ps.TotalGoals)
    .FirstOrDefaultAsync();

Console.WriteLine($"Лучший бомбардир команды: {bestTeamScorer?.PlayerName} - {bestTeamScorer?.TotalGoals}");

// Топ-3 бомбардиров всего чемпионата
var top3Scorers = await context.PlayerStats
    .OrderByDescending(ps => ps.TotalGoals)
    .Take(3)
    .ToListAsync();

Console.WriteLine("Топ-3 бомбардиров чемпионата:");
foreach (var s in top3Scorers)
    Console.WriteLine($"  {s.PlayerName} ({s.TeamName}) - {s.TotalGoals}");

// Лучший бомбардир чемпионата
var bestScorer = await context.PlayerStats
    .OrderByDescending(ps => ps.TotalGoals)
    .FirstOrDefaultAsync();

Console.WriteLine($"Лучший бомбардир чемпионата: {bestScorer?.PlayerName} - {bestScorer?.TotalGoals}");

// ============================================
// ЗАДАНИЕ 3: Голы команд
// ============================================
Console.WriteLine("\n=== ЗАДАНИЕ 3: Голы команд ===");

// Топ-3 команд, забивших больше всего
var top3Scored = await context.TeamStandings
    .OrderByDescending(ts => ts.TotalScored)
    .Take(3)
    .ToListAsync();

Console.WriteLine("Топ-3 команд по забитым голам:");
foreach (var t in top3Scored)
    Console.WriteLine($"  {t.TeamName} - {t.TotalScored}");

// Команда, забившая больше всего
var bestAttack = await context.TeamStandings
    .OrderByDescending(ts => ts.TotalScored)
    .FirstOrDefaultAsync();

Console.WriteLine($"Лучшая атака: {bestAttack?.TeamName} - {bestAttack?.TotalScored}");

// Топ-3 команд, пропустивших меньше всего
var top3Defense = await context.TeamStandings
    .OrderBy(ts => ts.TotalConceded)
    .Take(3)
    .ToListAsync();

Console.WriteLine("Топ-3 команд по пропущенным (меньше всего):");
foreach (var t in top3Defense)
    Console.WriteLine($"  {t.TeamName} - {t.TotalConceded}");

// Команда, пропустившая меньше всего
var bestDefense = await context.TeamStandings
    .OrderBy(ts => ts.TotalConceded)
    .FirstOrDefaultAsync();

Console.WriteLine($"Лучшая защита: {bestDefense?.TeamName} - {bestDefense?.TotalConceded}");

// ============================================
// ЗАДАНИЕ 4: Очки команд
// ============================================
Console.WriteLine("\n=== ЗАДАНИЕ 4: Очки ===");

// Топ-3 команд по очкам
var top3Points = await context.TeamStandings
    .OrderByDescending(ts => ts.TotalPoints)
    .Take(3)
    .ToListAsync();

Console.WriteLine("Топ-3 команд по очкам:");
foreach (var t in top3Points)
    Console.WriteLine($"  {t.TeamName} - {t.TotalPoints} очков");

// Команда с наибольшим количеством очков
var bestTeam = await context.TeamStandings
    .OrderByDescending(ts => ts.TotalPoints)
    .FirstOrDefaultAsync();

Console.WriteLine($"Лидер: {bestTeam?.TeamName} - {bestTeam?.TotalPoints}");

// Топ-3 команд с наименьшим количеством очков
var worst3Teams = await context.TeamStandings
    .OrderBy(ts => ts.TotalPoints)
    .Take(3)
    .ToListAsync();

Console.WriteLine("Топ-3 команд с наименьшими очками:");
foreach (var t in worst3Teams)
    Console.WriteLine($"  {t.TeamName} - {t.TotalPoints}");

// Команда с наименьшим количеством очков
var worstTeam = await context.TeamStandings
    .OrderBy(ts => ts.TotalPoints)
    .FirstOrDefaultAsync();

Console.WriteLine($"Аутсайдер: {worstTeam?.TeamName} - {worstTeam?.TotalPoints}");

Console.WriteLine("\nГотово! Нажми любую клавишу для выхода.");
Console.ReadKey();
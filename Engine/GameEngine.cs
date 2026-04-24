namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameEngine
{
    private Graph _graph = new();
    private ItemDatabase _itemDb = new();
    private Random _rng;

    public GameEngine()
    {
        _rng = new Random();
    }

    public Graph GetGraph() => _graph;

    public void LoadGraph(string nodesCsv, string mapCsv)
    {
        LoadNodes(nodesCsv);
        LoadEdges(mapCsv);
    }

    private void LoadNodes(string csv)
    {
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 3) continue;

            var name = parts[0].Trim().Trim('"');
            if (double.TryParse(parts[1], out var x) && double.TryParse(parts[2], out var y))
                _graph.AddNode(name, x, y);
        }
    }

    private void LoadEdges(string csv)
    {
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 3) continue;

            var from = parts[0].Trim().Trim('"');
            var to = parts[1].Trim().Trim('"');
            if (double.TryParse(parts[2], out var weight))
            {
                _graph.AddEdge(from, to, weight);
                _graph.AddEdge(to, from, weight);
            }
        }
    }

    public void LoadItems(string csv)
    {
        _itemDb.LoadFromCSV(csv);
    }

    public SimulationResult RunSimulation(int runs)
    {
        const int STEPS = 5;
        var rarityCounter = new Dictionary<string, long>();
        var weaponCounter = new Dictionary<string, long>();
        long totalLootRolls = 0;

        var nodesList = _graph.Nodes.Keys.ToList();
        if (nodesList.Count == 0)
            return new SimulationResult(runs, STEPS, 0, new(), new());

        var weaponTypeMap = new Dictionary<string, string>
        {
            { "Medium", "AR" },
            { "Shells", "Shotgun" },
            { "Light", "SMG" },
            { "Heavy", "Sniper" },
            { "Rockets", "Sniper" }
        };

        // Initialize counters for all rarities and weapon types
        foreach (var weapon in _itemDb.Items)
        {
            if (!rarityCounter.ContainsKey(weapon.Rarity))
                rarityCounter[weapon.Rarity] = 0;
            var weaponType = weaponTypeMap.GetValueOrDefault(weapon.AmmoType, weapon.AmmoType);
            if (!weaponCounter.ContainsKey(weaponType))
                weaponCounter[weaponType] = 0;
        }

        for (int run = 0; run < runs; run++)
        {
            // Pick random starting location
            string currentLoc = nodesList[_rng.Next(nodesList.Count)];
            var visitedChests = new HashSet<string>();

            // Run STEPS iterations
            for (int step = 0; step < STEPS; step++)
            {
                // Check if current location has a chest that hasn't been looted this run
                if (!visitedChests.Contains(currentLoc))
                {
                    // Loot the chest
                    var weapon = _itemDb.LootPool.RollLoot(_rng);
                    rarityCounter[weapon.Rarity]++;
                    var weaponType = weaponTypeMap.GetValueOrDefault(weapon.AmmoType, weapon.AmmoType);
                    weaponCounter[weaponType]++;
                    totalLootRolls++;
                    visitedChests.Add(currentLoc);
                }

                // Move to a random neighbor
                var neighbors = _graph.GetNeighbors(currentLoc);
                if (neighbors.Count > 0)
                    currentLoc = neighbors[_rng.Next(neighbors.Count)];
            }
        }

        var rarityDist = ConvertDistribution(rarityCounter, totalLootRolls);
        var weaponDist = ConvertDistribution(weaponCounter, totalLootRolls);

        return new SimulationResult(runs, STEPS, totalLootRolls, rarityDist, weaponDist);
    }

    public TraversalOutput RunPathfinding(string start, string end, string algorithm)
    {
        List<string> path;

        if (algorithm == "astar")
            path = _graph.AStar(start, end);
        else
            path = _graph.ShortestPath(start, end);

        var steps = new List<(string From, string To)>();
        for (int i = 0; i < path.Count - 1; i++)
            steps.Add((path[i], path[i + 1]));

        return new TraversalOutput(path, steps);
    }

    private Dictionary<string, (double Percentage, long Count)> ConvertDistribution(
        Dictionary<string, long> counter, long total)
    {
        var dist = new Dictionary<string, (double, long)>();
        foreach (var kvp in counter)
        {
            var pct = total > 0 ? (kvp.Value / (double)total) * 100 : 0;
            dist[kvp.Key] = (pct, kvp.Value);
        }
        return dist;
    }
}

public record TraversalOutput(List<string> Path, List<(string From, string To)> Steps);

public record SimulationResult(
    int Runs,
    int StepsPerRun,
    long TotalLootRolls,
    Dictionary<string, (double Percentage, long Count)> RarityDistribution,
    Dictionary<string, (double Percentage, long Count)> WeaponDistribution);

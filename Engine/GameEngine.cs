namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameEngine
{
    private Graph _graph = new();
    private ItemDatabase _itemDb = new();
    private LootPool _lootPool = new();
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
        _lootPool.Initialize(_itemDb);
    }

    public SimulationResult RunSimulation(int runs)
    {
        var rarityCounter = new Dictionary<string, long>();
        var weaponCounter = new Dictionary<string, long>();
        long totalLootRolls = 0;
        int stepsPerRun = 0;

        foreach (var rarity in _itemDb.GetRarities())
            rarityCounter[rarity] = 0;

        foreach (var wType in _itemDb.GetWeaponTypes())
            weaponCounter[wType] = 0;

        for (int i = 0; i < runs; i++)
        {
            var (steps, rolls) = RunSingleGame();
            stepsPerRun = steps;
            totalLootRolls += rolls;

            // Simple approximation: distribute loot proportionally
            for (int j = 0; j < rolls; j++)
            {
                var item = _lootPool.RollLoot(_rng);
                rarityCounter.TryGetValue(item.Rarity, out var rc);
                rarityCounter[item.Rarity] = rc + 1;

                weaponCounter.TryGetValue(item.WeaponType, out var wc);
                weaponCounter[item.WeaponType] = wc + 1;
            }
        }

        var rarityDist = ConvertDistribution(rarityCounter, totalLootRolls);
        var weaponDist = ConvertDistribution(weaponCounter, totalLootRolls);

        return new SimulationResult(runs, stepsPerRun, totalLootRolls, rarityDist, weaponDist);
    }

    private (int Steps, long Rolls) RunSingleGame()
    {
        // Simulate a single game: pick a random start and end, traverse the path
        var nodes = _graph.Nodes;
        if (nodes.Count < 2)
            return (0, 0);

        var nodesList = nodes.Keys.ToList();
        var start = nodesList[_rng.Next(nodesList.Count)];
        var end = nodesList[_rng.Next(nodesList.Count)];

        while (end == start)
            end = nodesList[_rng.Next(nodesList.Count)];

        var path = _graph.ShortestPath(start, end);
        long rolls = (path.Count - 1) * 10; // Simplified: 10 loot drops per step

        return (path.Count - 1, rolls);
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

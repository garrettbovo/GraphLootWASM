namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;

public class GameEngine
{
    private Graph _graph = new();
    private ItemDatabase _itemDb = new();
    private Random _rng = new();

    public Graph GetGraph() => _graph;

    public void LoadGraph(string nodesCsv, string mapCsv)
    {
        LoadNodes(nodesCsv);
        LoadEdges(mapCsv);
    }

    private void LoadNodes(string csv)
    {
        foreach (var line in csv.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 3) continue;

            var name = parts[0].Trim().Trim('"');
            if (double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var y))
                _graph.AddNode(name, x, y);
        }
    }

    private void LoadEdges(string csv)
    {
        foreach (var line in csv.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 3) continue;

            var from = parts[0].Trim().Trim('"');
            var to   = parts[1].Trim().Trim('"');
            if (double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var weight))
            {
                _graph.AddEdge(from, to, weight);
                _graph.AddEdge(to, from, weight);
            }
        }
    }

    public void LoadItems(string csv) => _itemDb.LoadFromCSV(csv);

    public SimulationResult RunSimulation(int runs)
    {
        var rarityCounter = new Dictionary<string, long>();
        var weaponCounter = new Dictionary<string, long>();
        long totalLootRolls = 0;
        int stepsPerRun = 0;

        foreach (var r in _itemDb.GetRarities())   rarityCounter[r] = 0;
        foreach (var c in _itemDb.GetWeaponCategories()) weaponCounter[c] = 0;

        for (int i = 0; i < runs; i++)
        {
            var (steps, rolls) = RunSingleGame();
            stepsPerRun = steps;
            totalLootRolls += rolls;

            for (int j = 0; j < rolls; j++)
            {
                var item = _itemDb.RollWeapon(_rng);
                if (item == null) continue;

                if (rarityCounter.ContainsKey(item.Rarity))
                    rarityCounter[item.Rarity]++;

                var cat = ItemDatabase.GetWeaponCategory(item.WeaponType);
                if (cat != null && weaponCounter.ContainsKey(cat))
                    weaponCounter[cat]++;
            }
        }

        return new SimulationResult(
            runs, stepsPerRun, totalLootRolls,
            ConvertDistribution(rarityCounter, totalLootRolls),
            ConvertDistribution(weaponCounter, totalLootRolls));
    }

    private (int Steps, long Rolls) RunSingleGame()
    {
        var nodes = _graph.Nodes;
        if (nodes.Count < 2) return (0, 0);

        var keys  = nodes.Keys.ToList();
        var start = keys[_rng.Next(keys.Count)];
        var end   = keys[_rng.Next(keys.Count)];
        while (end == start)
            end = keys[_rng.Next(keys.Count)];

        var path = _graph.ShortestPath(start, end);
        var steps = path.Count - 1;
        return (steps, steps); // 1 weapon roll per step
    }

    public TraversalOutput RunPathfinding(string start, string end, string algorithm)
    {
        var path = algorithm == "astar"
            ? _graph.AStar(start, end)
            : _graph.ShortestPath(start, end);

        var steps = new List<(string From, string To)>();
        for (int i = 0; i < path.Count - 1; i++)
            steps.Add((path[i], path[i + 1]));

        return new TraversalOutput(path, steps);
    }

    private static Dictionary<string, (double Percentage, long Count)> ConvertDistribution(
        Dictionary<string, long> counter, long total)
    {
        var dist = new Dictionary<string, (double, long)>();
        var counted = counter.Values.Sum();
        foreach (var kvp in counter)
        {
            var pct = counted > 0 ? kvp.Value / (double)counted * 100 : 0;
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

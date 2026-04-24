namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;

public class ItemDatabase
{
    private List<Weapon> _items = new();
    private LootPool _lootPool = new();

    public List<Weapon> Items => _items;
    public LootPool LootPool => _lootPool;

    public void LoadFromCSV(string csv)
    {
        _items.Clear();
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 4) continue;

            var name = parts[0].Trim().Trim('"');
            var rarity = parts[1].Trim().Trim('"');
            var ammoType = parts[2].Trim().Trim('"');
            if (double.TryParse(parts[3], out var damage))
            {
                var weapon = new Weapon(name, rarity, ammoType, damage);
                _items.Add(weapon);
            }
        }
        _lootPool.Initialize(_items);
    }
}

public class LootPool
{
    private List<(Weapon Weapon, double Weight)> _weightedLootTable = new();
    private double _totalWeight = 0;

    private static readonly Dictionary<string, double> RarityWeights = new()
    {
        { "Common", 40 },
        { "Uncommon", 30 },
        { "Rare", 15 },
        { "Epic", 8 },
        { "Legendary", 4 }
    };

    private static readonly Dictionary<string, double> AmmoWeights = new()
    {
        { "Light", 1.1 },
        { "Medium", 1.0 },
        { "Heavy", 0.5 },
        { "Shells", 0.9 },
        { "Rockets", 0.3 }
    };

    private const double Weapons = 100.0;

    public void Initialize(List<Weapon> items)
    {
        _weightedLootTable.Clear();
        _totalWeight = 0;

        foreach (var weapon in items)
        {
            double rarityWeight = RarityWeights.GetValueOrDefault(weapon.Rarity, 1);
            double ammoWeight = AmmoWeights.GetValueOrDefault(weapon.AmmoType, 1.0);
            double categoryWeight = Weapons * ammoWeight;
            double finalWeight = rarityWeight * categoryWeight;

            _weightedLootTable.Add((weapon, finalWeight));
            _totalWeight += finalWeight;
        }
    }

    public Weapon RollLoot(Random rng)
    {
        if (_weightedLootTable.Count == 0)
            return new Weapon("Empty", "Common", "Medium", 0);

        double roll;
        do
        {
            roll = rng.NextDouble() * _totalWeight;
        } while (roll == 0.0);

        double cumulative = 0;
        foreach (var (weapon, weight) in _weightedLootTable)
        {
            cumulative += weight;
            if (roll <= cumulative)
                return weapon;
        }

        return _weightedLootTable[_weightedLootTable.Count - 1].Weapon;
    }
}

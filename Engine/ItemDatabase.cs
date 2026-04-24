namespace GraphLootWASM.Engine;

using System.Collections.Generic;
using System.Linq;

public class ItemDatabase
{
    private List<Weapon> _items = new();
    private Dictionary<string, int> _rarityCount = new();
    private Dictionary<string, int> _weaponTypeCount = new();

    public List<Weapon> Items => _items;

    public void LoadFromCSV(string csv)
    {
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 4) continue;

            var name = parts[0].Trim().Trim('"');
            var rarity = parts[1].Trim().Trim('"');
            var weaponType = parts[2].Trim().Trim('"');
            if (double.TryParse(parts[3], out var damage))
            {
                var weapon = new Weapon(name, rarity, weaponType, damage);
                _items.Add(weapon);

                _rarityCount.TryGetValue(rarity, out var rc);
                _rarityCount[rarity] = rc + 1;

                _weaponTypeCount.TryGetValue(weaponType, out var wc);
                _weaponTypeCount[weaponType] = wc + 1;
            }
        }
    }

    public IEnumerable<string> GetRarities() => _rarityCount.Keys;
    public IEnumerable<string> GetWeaponTypes() => _weaponTypeCount.Keys;
}

public class LootPool
{
    private List<Weapon> _lootTable = new();
    private Dictionary<string, double> _rarityWeights = new();
    private double _totalRarityWeight = 0;

    public void Initialize(ItemDatabase db)
    {
        _lootTable = db.Items;
        BuildRarityWeights();
    }

    private void BuildRarityWeights()
    {
        _rarityWeights.Clear();
        var rarityMap = new Dictionary<string, int>();

        foreach (var item in _lootTable)
        {
            rarityMap.TryGetValue(item.Rarity, out var count);
            rarityMap[item.Rarity] = count + 1;
        }

        foreach (var kvp in rarityMap)
        {
            // Weight by frequency: items with more entries in the table are more common
            _rarityWeights[kvp.Key] = kvp.Value;
            _totalRarityWeight += kvp.Value;
        }
    }

    public Weapon RollLoot(Random rng)
    {
        if (_lootTable.Count == 0)
            return new Weapon("Empty", "Common", "AR", 0);

        var idx = rng.Next(_lootTable.Count);
        return _lootTable[idx];
    }

    public Dictionary<string, (double Percentage, long Count)> GetRarityDistribution(long totalRolls)
    {
        var dist = new Dictionary<string, (double, long)>();
        foreach (var rarity in _rarityWeights.Keys)
        {
            var weight = _rarityWeights[rarity];
            var pct = (weight / _totalRarityWeight) * 100;
            var count = (long)(totalRolls * weight / _totalRarityWeight);
            dist[rarity] = (pct, count);
        }
        return dist;
    }

    public Dictionary<string, (double Percentage, long Count)> GetWeaponDistribution(long totalRolls)
    {
        var weaponMap = new Dictionary<string, int>();
        foreach (var item in _lootTable)
        {
            weaponMap.TryGetValue(item.WeaponType, out var count);
            weaponMap[item.WeaponType] = count + 1;
        }

        var dist = new Dictionary<string, (double, long)>();
        long total = weaponMap.Values.Sum();
        foreach (var kvp in weaponMap)
        {
            var pct = (kvp.Value / (double)total) * 100;
            var count = (long)(totalRolls * kvp.Value / (double)total);
            dist[kvp.Key] = (pct, count);
        }
        return dist;
    }
}

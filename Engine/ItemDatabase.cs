namespace GraphLootWASM.Engine;

using System.Collections.Generic;

public class ItemDatabase
{
    private readonly List<(Weapon Item, double Weight)> _weaponPool = new();
    private double _totalWeaponWeight = 0;

    private static double GetRarityWeight(string rarity) => rarity switch
    {
        "Common"    => 40,
        "Uncommon"  => 30,
        "Rare"      => 15,
        "Epic"      =>  8,
        "Legendary" =>  4,
        "Mythic"    =>  1,
        "Exotic"    =>  1,
        _ => 0
    };

    // Base weapon weight * ammo multiplier (matches C++ reference)
    private static double GetAmmoWeight(string ammoType) => ammoType switch
    {
        "Light"   => 110,
        "Medium"  => 100,
        "Shells"  =>  90,
        "Heavy"   =>  50,
        "Rockets" =>  30,
        _ => 0
    };

    // Maps ammo type to the 4 display categories; null = excluded from chart
    public static string? GetWeaponCategory(string ammoType) => ammoType switch
    {
        "Medium" => "AR",
        "Shells" => "Shotgun",
        "Light"  => "SMG",
        "Heavy"  => "Heavy",
        _ => null
    };

    public void LoadFromCSV(string csv)
    {
        _weaponPool.Clear();
        _totalWeaponWeight = 0;

        foreach (var line in csv.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;

            var parts = trimmed.Split(',');
            if (parts.Length < 6) continue;

            // CSV format: ID,Name,Type,Rarity,Description,AmmoType,Damage,...
            var type = parts[2].Trim();
            if (type != "Weapon") continue;

            var name     = parts[1].Trim();
            var rarity   = parts[3].Trim();
            var ammoType = parts[5].Trim();

            double damage = 0;
            if (parts.Length > 6) double.TryParse(parts[6].Trim(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out damage);

            var weight = GetRarityWeight(rarity) * GetAmmoWeight(ammoType);
            if (weight <= 0) continue;

            _weaponPool.Add((new Weapon(name, rarity, ammoType, damage), weight));
            _totalWeaponWeight += weight;
        }
    }

    public Weapon? RollWeapon(Random rng)
    {
        if (_weaponPool.Count == 0) return null;

        var roll = rng.NextDouble() * _totalWeaponWeight;
        double accumulated = 0;
        foreach (var (item, weight) in _weaponPool)
        {
            accumulated += weight;
            if (roll <= accumulated) return item;
        }
        return _weaponPool[^1].Item;
    }

    public IEnumerable<string> GetRarities() =>
        new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };

    public IEnumerable<string> GetWeaponCategories() =>
        new[] { "AR", "Shotgun", "SMG", "Heavy" };
}

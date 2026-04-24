namespace GraphLootWASM.Engine;

public record Edge(string To, double Weight);

public record NodeData(string Name, double X, double Y);

public record PathResult(List<string> Path, double Cost, int NodesVisited);

public abstract class Item
{
    public string Name { get; set; }
    public string Rarity { get; set; }

    protected Item(string name, string rarity)
    {
        Name = name;
        Rarity = rarity;
    }

    public abstract string GetItemType();
}

public class Weapon : Item
{
    public string AmmoType { get; set; }
    public double Damage { get; set; }

    public Weapon(string name, string rarity, string ammoType, double damage)
        : base(name, rarity)
    {
        AmmoType = ammoType;
        Damage = damage;
    }

    public override string GetItemType() => "Weapon";
}

public record LootDrop(string ItemName, string Rarity, string ItemType);

public record SimulationStats(
    int Runs,
    int StepsPerRun,
    long TotalLootRolls,
    Dictionary<string, (double Percentage, long Count)> RarityDistribution,
    Dictionary<string, (double Percentage, long Count)> WeaponDistribution);

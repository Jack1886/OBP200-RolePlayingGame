namespace OBP200_RolePlayingGame;

public class Enemy : Character
{
    public string Type { get; private set; }
    public int XPReward { get; private set; }
    public int GoldReward { get; private set; }

    public Enemy(string type, string name, int health, int attack, int defense, int xpReward, int goldReward)
        : base(name, health, health, attack, defense)
    {
        Type = type;
        XPReward = xpReward;
        GoldReward = goldReward;
    }
}
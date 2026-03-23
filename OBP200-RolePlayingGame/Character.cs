namespace OBP200_RolePlayingGame;

public abstract class Character
{
 public string Name { get; protected set; }
 public int Health { get; protected set; }
 public int MaxHealth { get; protected set; }
 public int Attack { get; protected set; }
 public int Defense { get; protected set; }


protected Character(string name, int health, int maxhealth, int attack, int defense)
{
 Name = name;
 Health = health;
 MaxHealth = maxhealth;
 Attack = attack;
 Defense = defense;
}
//Allmäna metoder 
public void TakeDamage(int damage)
{
 Health = Math.Max(0,Health - damage);
}

public void Heal(int amount)
{
 Health = Math.Min(MaxHealth, Health + amount);
}

public bool IsDead()
{
 return Health <= 0;
}
}
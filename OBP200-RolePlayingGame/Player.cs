using System;
using System.Collections.Generic;

namespace OBP200_RolePlayingGame;

public class Player : Character
{
    public string ClassName { get; private set; }
    public int Gold { get; private set; }
    public int XP { get; private set; }
    public int Level  { get; private set; }
    public int Potions { get; private set; }
    public List<string> Inventory { get; private set; }

    public Player(string name, string cls, int health, int maxhealth, int attack, int defense, int gold, int potions)
        : base(name, health, maxhealth, attack, defense)
    {
        ClassName = cls;
        Gold = gold;
        Potions = potions;
        XP = 0;
        Level = 1;
        Inventory = new List<string> { "Wooden Sword", "Cloth Armor" };
    }
    //Player-Specifika metoder
    public void AddGold (int amount) => Gold += amount;
    public void SpendGold (int amount) => Gold = Math.Max (0, Gold - amount);
    public void AddXP(int amount) => XP += amount;
    public void UsePotion()
    {
        if (Potions > 0)
        {
            Potions--;
            Heal(12);
        }
    }

    public void AddItem (string item) => Inventory.Add(item);
    public void AddPotions(int amount) => Potions += amount;
    public void IncreaseAttack(int amount) => Attack += amount;
    public void IncreaseDefense(int amount) => Defense += amount;
    public void LevelUp(int addedHealth, int addedAttack, int addedDefense)
    {
        Level++;
        MaxHealth += addedHealth;
        Attack += addedAttack;
        Defense += addedDefense;
        Health = MaxHealth;
    }
}
using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    // ======= Globalt tillstånd  =======

    // Spelarens "databas": alla värden som strängar
    // index: 0 Name, 1 Class, 2 HP, 3 MaxHP, 4 ATK, 5 DEF, 6 GOLD, 7 XP, 8 LEVEL, 9 POTIONS, 10 INVENTORY (semicolon-sep)
    static Player player;
    // Rum: [type, label]
    // types: battle, treasure, shop, rest, boss
    static List<string[]> Rooms = new List<string[]>();

    // Fiendemallar: [type, name, HP, ATK, DEF, XPReward, GoldReward]
    static List<string[]> EnemyTemplates = new List<string[]>();

    // Status för kartan
    static int CurrentRoomIndex = 0;

    // Random
    static Random Rng = new Random();

    // ======= Main =======

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    // ======= Meny & Init =======

    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var k = (Console.ReadLine() ?? "").Trim();

        string cls = "Warrior";
        int health = 0, maxhealth = 0, attack = 0, defense = 0;
        int potions = 0, gold = 0;
        
        switch (k)
        {
            case "1": // Warrior: tankig
                cls = "Warrior";
                maxhealth = 40; health = 40; attack = 7; defense = 5; potions = 2; gold = 15;
                break;
            case "2": // Mage: hög damage, låg def
                cls = "Mage";
                maxhealth = 28; health = 28; attack = 10; defense = 2; potions = 2; gold = 15;
                break;
            case "3": // Rogue: krit-chans
                cls = "Rogue";
                maxhealth = 32; health = 32; attack = 8; defense = 3; potions = 3; gold = 20;
                break;
            default:
                cls = "Warrior";
                maxhealth = 40; health = 40; attack = 7; defense = 5; potions = 2; gold = 15;
                break;
        }
        
        //Player objekt
        player = new Player(name, cls, health,maxhealth, attack, defense, potions, gold);
        

        // Initiera karta (linjärt äventyr)
        Rooms.Clear();
        Rooms.Add(new[] { "battle", "Skogsstig" });
        Rooms.Add(new[] { "treasure", "Gammal kista" });
        Rooms.Add(new[] { "shop", "Vandrande köpman" });
        Rooms.Add(new[] { "battle", "Grottans mynning" });
        Rooms.Add(new[] { "rest", "Lägereld" });
        Rooms.Add(new[] { "battle", "Grottans djup" });
        Rooms.Add(new[] { "boss", "Urdraken" });

        CurrentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {name} the {cls}!");
        ShowStatus();
    }

    static void RunGameLoop()
    {
        while (true)
        {
            var room = Rooms[CurrentRoomIndex];
            Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room[1]} ({room[0]}) ---");

            bool continueAdventure = EnterRoom(room[0]);
            
            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }
            
            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            CurrentRoomIndex++;
            
            if (CurrentRoomIndex >= Rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    // ======= Rumshantering =======

    static bool EnterRoom(string type)
    {
        switch ((type ?? "battle").Trim())
        {
            case "battle":
                return DoBattle(isBoss: false);
            case "boss":
                return DoBattle(isBoss: true);
            case "treasure":
                return DoTreasure();
            case "shop":
                return DoShop();
            case "rest":
                return DoRest();
            default:
                Console.WriteLine("Du vandrar vidare...");
                return true;
        }
    }

    // ======= Strid =======

    static bool DoBattle(bool isBoss)
    {
        var enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy.Name} dyker upp! (Health {enemy.Health}, Attack {enemy.Attack}, Defense {enemy.Defense})");
        
        while (!enemy.IsDead() && !player.IsDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy.Name} Health={enemy.Health}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemy.Defense);
                enemy.TakeDamage (damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemy.Defense, isBoss);
                enemy.TakeDamage (special);
                Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (!enemy.IsDead())
            {
                // Fiendens tur
                int enemyDamage = CalculateEnemyDamage(enemy.Attack);
                ApplyDamageToPlayer(enemyDamage);
                Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
            }
            
        }

        if (player.IsDead())
        {
            return false; // avsluta äventyr
        }

        // Vinstrapporter, XP, guld, loot
        int xpReward = enemy.XPReward;
        int goldReward = enemy.GoldReward;

        AddPlayerXp(enemy.XPReward);
        AddPlayerGold(enemy.GoldReward);

        Console.WriteLine($"Seger! +{xpReward} XP, +{goldReward} guld.");
        MaybeDropLoot(enemy.Name);

        return true;
    }

    static Enemy GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new Enemy("boss", "Urdraken", 55, 9, 4, 30, 50);
        }
        else
        {
            // Slumpa bland templates
            var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
            
            // Slmumpmässig justering av stats
            int health = ParseInt(template[2], 10) + Rng.Next(-1, 3);
            int attack = ParseInt(template[3], 3) + Rng.Next(0, 2);
            int defense = ParseInt(template[4], 0) + Rng.Next(0, 2);
            int xp = ParseInt(template[5], 4) + Rng.Next(0, 3);
            int gold = ParseInt(template[6], 2) + Rng.Next(0, 3);
           
            return new Enemy ( template[0], template[1], health, attack, defense, xp, gold );
        }
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new[] { "beast", "Vildsvin", "18", "4", "1", "6", "4" });
        EnemyTemplates.Add(new[] { "undead", "Skelett", "20", "5", "2", "7", "5" });
        EnemyTemplates.Add(new[] { "bandit", "Bandit", "16", "6", "1", "8", "6" });
        EnemyTemplates.Add(new[] { "slime", "Geléslem", "14", "3", "0", "5", "3" });
    }

    static int CalculatePlayerDamage(int enemyDefense)
    {
        int attack = player.Attack;
        string cls = player.ClassName ?? "Warrior";

        // Beräkna grundskada
        int baseDmg = Math.Max(1, attack - (enemyDefense / 2));
        int roll = Rng.Next(0, 3); // liten variation

        switch (cls.Trim())
        {
            case "Warrior":
                baseDmg += 1; // warrior buff
                break;
            case "Mage":
                baseDmg += 2; // mage buff
                break;
            case "Rogue":
                baseDmg += (Rng.NextDouble() < 0.2) ? 4 : 0; // rogue crit-chans
                break;
            default:
                baseDmg += 0;
                break;
        }

        return Math.Max(1, baseDmg + roll);
    }

    static int UseClassSpecial(int enemyDefense, bool vsBoss)
    {
        string cls = player.ClassName ?? "Warrior";
        int specialDmg = 0;

        // Hantering av specialförmågor
        if (cls == "Warrior")
        {
            // Heavy Strike: hög skada men självskada
            Console.WriteLine("Warrior använder Heavy Strike!");
            int attack = player.Attack;
            specialDmg = Math.Max(2, attack + 3 - enemyDefense);
            player.TakeDamage(2); // självskada
        }
        else if (cls == "Mage")
        {
            // Fireball: stor skada, kostar guld
            int gold = player.Gold;
            
            if (gold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
               player.SpendGold(3);
               int attack = player.Attack;
                specialDmg = Math.Max(3, attack + 5 - (enemyDefense / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDmg = 0;
            }
        }
        else if (cls == "Rogue")
        {
            // Backstab: chans att ignorera försvar, hög risk/hög belöning
            if (Rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                int attack = player.Attack;
                specialDmg = Math.Max(4, attack + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDmg = 1;
            }
        }
        else
        {
            specialDmg = 0;
        }

        // Dämpa skada mot bossen
        if (vsBoss)
        {
            specialDmg = (int)Math.Round(specialDmg * 0.8);
        }

        return Math.Max(0, specialDmg);
    }

    static int CalculateEnemyDamage(int enemyAttack)
    {
        int def = player.Defense;
        int roll = Rng.Next(0, 3);

        int damage = Math.Max(1, enemyAttack - (def / 2)) + roll;

        // Liten chans till "glancing blow" (minskad skada)
        if (Rng.NextDouble() < 0.1) damage = Math.Max(1, damage - 2);

        return damage;
    }

    static void ApplyDamageToPlayer(int damage)
    {
        player.TakeDamage(damage);
    }

    static void UsePotion()
    {
        if (player.Potions <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }
        int oldHealth = player.Health;

        // Helning av spelaren
       player.Heal(12);
       player.UsePotion();
        Console.WriteLine($"Du dricker en dryck och återfår {player.Health - oldHealth} Hälsa.");
    }

    static bool TryRunAway()
    {
        // Flyktschans baserad på karaktärsklass
        string cls = player.ClassName ?? "Warrior";
        double chance = 0.25;
        if (cls == "Rogue") chance = 0.5;
        if (cls == "Mage") chance = 0.35;
        return Rng.NextDouble() < chance;
    }

    static bool IsPlayerDead()
    {
        return player.Health <= 0;
    }

    static void AddPlayerXp(int amount)
    {
        player.AddXP(amount);
        MaybeLevelUp();
    }

    static void AddPlayerGold(int amount)
    {
        player.AddGold(amount);
    }

    static void MaybeLevelUp()
    {
        // Nivåtrösklar
        int xp = player.XP;
        int lvl = player.Level;
        int nextThreshold = lvl == 1 ? 10 : (lvl == 2 ? 25 : (lvl == 3 ? 45 : lvl * 20));

        if (xp >= nextThreshold)
        {
            // Uppgradering baserad på karaktärsklass
            string cls = player.ClassName ?? "Warrior";
            int addedHealth = 0, addedAttack = 0, addedDefense = 0;

            switch (cls)
            {
                case "Warrior":
                    addedHealth = 6; addedAttack = 2; addedDefense = 2;
                    break;
                case "Mage":
                    addedHealth = 4; addedAttack = 4; addedDefense = 1;
                    break;
                case "Rogue":
                    addedHealth = 5; addedAttack = 3; addedDefense = 1;
                    break;
                default:
                    addedHealth = 4; addedAttack = 3; addedDefense = 1;
                    break;
            }
            player.LevelUp(addedHealth, addedAttack, addedDefense);
             // full heal vid level up
            Console.WriteLine($"Du når nivå {lvl + 1}! Värden ökade och HP återställd.");
        }
    }

    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = "Minor Gem";
            if (enemyName.Contains("Urdraken")) item = "Dragon Scale";

            player.Inventory.Add(item);

            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Rumshändelser =======

    static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[Rng.Next(items.Length)];

            player.Inventory.Add(found);
            Console.WriteLine($"Du plockar upp: {found}");
        }
        return true;
    }

    static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {player.Gold} | Drycker: {player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                TryBuy(10, () => player.AddPotions (1), "Du köper en dryck.");
            }
            else if (val == "2")
            {
                TryBuy(25, () => player.IncreaseAttack(2) , "Du köper ett bättre vapen.");
            }
            else if (val == "3")
            {
                TryBuy(25, () => player.IncreaseDefense(2), "Du köper bättre rustning.");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }
        return true;
    }

    static void TryBuy(int cost, Action apply, string successMsg)
    {
        if (player.Gold >= cost)
        {
            player.AddGold(-cost);
            apply();
            Console.WriteLine(successMsg);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    static void SellMinorGems()
    {
        if (player.Inventory.Count == 0)
        {
            Console.WriteLine("Du har inga föremål att sälja.");
            return;
        }
        
        int count = player.Inventory.Count(x => x == "Minor Gem");
       
        if (count == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        player.Inventory.RemoveAll(x => x == "Minor Gem");
        
        player.AddGold (count * 5);
       
        Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
    }

    static bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        player.Heal(player.MaxHealth);
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    // ======= Status =======

    static void ShowStatus()
    {
        Console.WriteLine($"[{player.Name} | {player.ClassName}]  Health {player.Health}/{player.MaxHealth}  Attack {player.Attack}  Defense {player.Defense}  Level {player.Level}  XP {player.XP}  Guld {player.Gold}  Drycker {player.Potions}");
       
       
        if (player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: " + string.Join(", ", player.Inventory));
        }
    }
    
    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        try
        {
            int value = Convert.ToInt32(s);
            return value;
        }
        catch (Exception e)
        {
            return fallback;
        }
    }
}


namespace WarRegions.Core.Controllers
{
    public class BattleCalculator
    {
        private Random _random;
        
        public BattleCalculator()
        {
            _random = new Random();
            Console.WriteLine("[BATTLE] BattleCalculator initialized");
        }
        
        public BattleResult CalculateBattle(Army attacker, Army defender, Region battleRegion)
        {
            if (attacker.GetAliveUnitCount() == 0 || defender.GetAliveUnitCount() == 0)
            {
                Console.WriteLine("[BATTLE] Cannot calculate battle - one army has no alive units");
                return new BattleResult { BattleWasFought = false };
            }
            
            Console.WriteLine($"[BATTLE] Calculating battle: {attacker.ArmyName} vs {defender.ArmyName} at {battleRegion.RegionName}");
            
            var result = new BattleResult
            {
                Attacker = attacker,
                Defender = defender,
                BattleRegion = battleRegion,
                BattleWasFought = true
            };
            
            // Calculate base combat values
            int attackerPower = CalculateArmyCombatPower(attacker, false, battleRegion);
            int defenderPower = CalculateArmyCombatPower(defender, true, battleRegion);
            
            // Apply random factor (10% variance)
            double attackerRandomFactor = 0.9 + (_random.NextDouble() * 0.2);
            double defenderRandomFactor = 0.9 + (_random.NextDouble() * 0.2);
            
            attackerPower = (int)(attackerPower * attackerRandomFactor);
            defenderPower = (int)(defenderPower * defenderRandomFactor);
            
            // Apply development config multiplier
            if (DevConfig.DebugMode)
            {
                attackerPower = (int)(attackerPower * DevConfig.BattleDamageMultiplier);
            }
            
            // Determine winner
            result.AttackerWon = attackerPower > defenderPower;
            result.VictoryMargin = Math.Abs(attackerPower - defenderPower);
            
            // Calculate casualties
            CalculateCasualties(result, attackerPower, defenderPower);
            
            // Calculate rewards
            CalculateRewards(result);
            
            LogBattleResult(result, attackerPower, defenderPower);
            
            return result;
        }
        
        private int CalculateArmyCombatPower(Army army, bool isDefender, Region region)
        {
            int totalPower = 0;
            
            foreach (var unit in army.Units.Where(u => u.IsAlive))
            {
                int unitPower = CalculateUnitCombatPower(unit, isDefender, region);
                totalPower += unitPower;
            }
            
            // Apply army-wide modifiers
            totalPower = ApplyArmyModifiers(totalPower, army, isDefender, region);
            
            return totalPower;
        }
        
        private int CalculateUnitCombatPower(UnitCard unit, bool isDefender, Region region)
        {
            int basePower = isDefender ? unit.Stats.Defense : unit.Stats.Attack;
            
            // Apply terrain bonuses
            int terrainBonus = region?.GetCombatBonus() ?? 0;
            basePower += terrainBonus;
            
            // Apply morale modifier
            double moraleMultiplier = 0.8 + (unit.Owner?.Morale ?? 1.0) * 0.2;
            basePower = (int)(basePower * moraleMultiplier);
            
            // Apply level bonus
            double levelMultiplier = 1.0 + (unit.Level - 1) * 0.1;
            basePower = (int)(basePower * levelMultiplier);
            
            // Apply rarity bonus
            double rarityMultiplier = unit.Rarity.GetStatBonusMultiplier();
            basePower = (int)(basePower * rarityMultiplier);
            
            return Math.Max(1, basePower);
        }
        
        private int ApplyArmyModifiers(int basePower, Army army, bool isDefender, Region region)
        {
            double modifier = 1.0;
            
            // Supply modifier
            modifier *= army.Supply;
            
            // Defender advantage
            if (isDefender)
            {
                modifier *= 1.1; // 10% defender advantage
            }
            
            // Region ownership bonus
            if (region?.Owner == army.Owner)
            {
                modifier *= 1.15; // 15% home territory bonus
            }
            
            // Army size bonus/penalty
            int aliveUnits = army.GetAliveUnitCount();
            if (aliveUnits <= 2)
            {
                modifier *= 0.7; // Small army penalty
            }
            else if (aliveUnits >= 8)
            {
                modifier *= 1.2; // Large army bonus
            }
            
            return (int)(basePower * modifier);
        }
        
        private void CalculateCasualties(BattleResult result, int attackerPower, int defenderPower)
        {
            int totalPower = attackerPower + defenderPower;
            
            if (totalPower == 0) return;
            
            // Calculate casualty percentages based on power ratio
            double attackerCasualtyRate = (double)defenderPower / totalPower;
            double defenderCasualtyRate = (double)attackerPower / totalPower;
            
            // Winning side takes fewer casualties
            if (result.AttackerWon)
            {
                attackerCasualtyRate *= 0.6; // Winner takes 40% fewer casualties
                defenderCasualtyRate *= 1.4; // Loser takes 40% more casualties
            }
            else
            {
                attackerCasualtyRate *= 1.4;
                defenderCasualtyRate *= 0.6;
            }
            
            // Convert to actual unit casualties
            result.AttackerCasualties = CalculateUnitCasualties(result.Attacker, attackerCasualtyRate);
            result.DefenderCasualties = CalculateUnitCasualties(result.Defender, defenderCasualtyRate);
            
            // Ensure at least some casualties in meaningful battles
            if (totalPower > 50)
            {
                result.AttackerCasualties = Math.Max(1, result.AttackerCasualties);
                result.DefenderCasualties = Math.Max(1, result.DefenderCasualties);
            }
        }
        
        private int CalculateUnitCasualties(Army army, double casualtyRate)
        {
            int totalHealth = army.Units.Where(u => u.IsAlive).Sum(u => u.Stats.MaxHealth);
            int casualties = (int)(totalHealth * casualtyRate);
            
            // Distribute casualties among units
            return DistributeDamageToUnits(army, casualties);
        }
        
        private int DistributeDamageToUnits(Army army, int totalDamage)
        {
            int damageDealt = 0;
            var aliveUnits = army.Units.Where(u => u.IsAlive).ToList();
            
            // Simple damage distribution
            int damagePerUnit = Math.Max(1, totalDamage / Math.Max(1, aliveUnits.Count));
            
            foreach (var unit in aliveUnits)
            {
                if (damageDealt >= totalDamage) break;
                
                int unitDamage = Math.Min(unit.CurrentHealth, damagePerUnit);
                unit.TakeDamage(unitDamage);
                damageDealt += unitDamage;
            }
            
            return damageDealt;
        }
        
        private void CalculateRewards(BattleResult result)
        {
            // Base reward based on enemy power
            int enemyPower = result.AttackerWon ? 
                CalculateArmyCombatPower(result.Defender, true, result.BattleRegion) :
                CalculateArmyCombatPower(result.Attacker, false, result.BattleRegion);
            
            int baseReward = enemyPower / 4; // 25% of enemy power as reward
            
            // Victory bonus
            if (result.AttackerWon)
            {
                baseReward = (int)(baseReward * 1.5);
            }
            
            // Apply development multiplier
            if (DevConfig.DebugMode && DevConfig.InfiniteResources)
            {
                baseReward *= 2;
            }
            
            result.SilverReward = baseReward;
            result.GoldReward = baseReward / 10; // 10% of silver reward as gold
            
            // Experience reward
            result.ExperienceGained = enemyPower / 10;
        }
        
        private void LogBattleResult(BattleResult result, int attackerPower, int defenderPower)
        {
            // ✅ التصحيح: استخدام string interpolation عادي بدل multiline strings المعقد
            Console.WriteLine("[BATTLE] Battle Result:");
            Console.WriteLine($"Location: {result.BattleRegion.RegionName}");
            Console.WriteLine($"Combatants: {result.Attacker.ArmyName} ({attackerPower}) vs {result.Defender.ArmyName} ({defenderPower})");
            Console.WriteLine($"Winner: {(result.AttackerWon ? result.Attacker.ArmyName : result.Defender.ArmyName)}");
            Console.WriteLine($"Casualties: {result.AttackerCasualties} (Attacker) / {result.DefenderCasualties} (Defender)");
            Console.WriteLine($"Rewards: {result.SilverReward} silver, {result.GoldReward} gold");
        }
        
        public BattleResult SimulateBattle(Army attacker, Army defender, Region battleRegion, int simulations = 1000)
        {
            Console.WriteLine($"[BATTLE] Simulating {simulations} battles...");
            
            int attackerWins = 0;
            int totalAttackerCasualties = 0;
            int totalDefenderCasualties = 0;
            
            for (int i = 0; i < simulations; i++)
            {
                var result = CalculateBattle(attacker, defender, battleRegion);
                
                if (result.AttackerWon) attackerWins++;
                totalAttackerCasualties += result.AttackerCasualties;
                totalDefenderCasualties += result.DefenderCasualties;
            }
            
            double winRate = (double)attackerWins / simulations;
            
            // ✅ التصحيح: استخدام Console.WriteLine عادي
            Console.WriteLine($"[BATTLE] Simulation Results ({simulations} runs):");
            Console.WriteLine($"Attacker Win Rate: {winRate:P1}");
            Console.WriteLine($"Average Attacker Casualties: {totalAttackerCasualties / simulations}");
            Console.WriteLine($"Average Defender Casualties: {totalDefenderCasualties / simulations}");
            Console.WriteLine($"Recommended: {(winRate > 0.7 ? "ATTACK" : winRate < 0.3 ? "RETREAT" : "RISKY")}");
            
            return new BattleResult
            {
                AttackerWon = winRate > 0.5,
                AttackerCasualties = totalAttackerCasualties / simulations,
                DefenderCasualties = totalDefenderCasualties / simulations
            };
        }
    }
    
    public class BattleResult
    {
        public Army Attacker { get; set; }
        public Army Defender { get; set; }
        public Region BattleRegion { get; set; }
        
        public bool AttackerWon { get; set; }
        public int VictoryMargin { get; set; }
        
        public int AttackerCasualties { get; set; }
        public int DefenderCasualties { get; set; }
        
        public int SilverReward { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceGained { get; set; }
        
        public bool BattleWasFought { get; set; }
        
        public BattleResult()
        {
            BattleWasFought = false;
        }
    }
}

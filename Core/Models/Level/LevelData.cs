
namespace WarRegions.Core.Models.Level
{
    public partial class LevelData  // جعلها partial لسهولة التعديل
    {
        public string LevelName { get; set; } = "Battle Field";
        public int MapWidth { get; set; } = 10;
        public int MapHeight { get; set; } = 8;
        public string AIDifficulty { get; set; } = "normal";
        public string AIBehavior { get; set; } = "aggressive";
        public class VictoryCondition
        {
            public string Type { get; set; } = "eliminate_all";
            public string Target { get; set; } = "enemy_units";
            public int RequiredCount { get; set; } = 0;
            public int TimeLimit { get; set; } = 0;
            // ✅ إضافة الخصائص المطلوبة
            public int MapWidth { get; set; } = 10;
            public int MapHeight { get; set; } = 8;
            public string AIDifficulty { get; set; } = "normal";
            public string AIBehavior { get; set; } = "aggressive";
        
            // ✅ إضافة طرق مساعدة
            public bool IsPositionValid(int x, int y)
            {
                return x >= 0 && x < MapWidth && y >= 0 && y < MapHeight;
            }
        
            public int GetTotalRegions()
            {
                return MapWidth * MapHeight;
            }

            public VictoryCondition() { }
            
            public VictoryCondition(string type, string target = "enemy_units", int requiredCount = 0)
            {
                Type = type;
                Target = target;
                RequiredCount = requiredCount;
            }
            
            public bool IsConditionMet(GameState gameState)
            {
                if (gameState?.CurrentPlayer == null) return false;
                
                switch (Type)
                {
                    case "eliminate_all":
                        return CheckEliminateAll(gameState);
                        
                    case "capture_regions":
                        return CheckCaptureRegions(gameState);
                        
                    case "survive_turns":
                        return CheckSurviveTurns(gameState);
                        
                    case "destroy_specific":
                        return CheckDestroySpecific(gameState);
                        
                    default:
                        return false;
                }
            }
            
            private bool CheckEliminateAll(GameState gameState)
            {
                // التصحيح: استخدام اللاعب البشري (المفترض أنه الأول)
                var humanPlayer = gameState.Players.FirstOrDefault();
                if (humanPlayer == null) return false;
                
                var enemyArmies = gameState.Armies.Where(a => a.Owner != humanPlayer && a.Owner != null);
                return !enemyArmies.Any(a => a.GetAliveUnitCount() > 0);
            }
            
            private bool CheckCaptureRegions(GameState gameState)
            {
                var humanPlayer = gameState.Players.FirstOrDefault();
                if (humanPlayer == null) return false;
                
                var playerRegions = gameState.Regions.Count(r => r.Owner == humanPlayer);
                return playerRegions >= RequiredCount;
            }
            
            private bool CheckSurviveTurns(GameState gameState)
            {
                return gameState.TurnNumber >= TimeLimit;
            }
            
            private bool CheckDestroySpecific(GameState gameState)
            {
                // أثناء التطوير، نعيد true لتجنب تعطيل اللعبة
                return DevConfig.DebugMode;
            }
            
            public string GetDescription()
            {
                return Type switch
                {
                    "eliminate_all" => "Destroy all enemy units",
                    "capture_regions" => $"Capture at least {RequiredCount} regions",
                    "survive_turns" => $"Survive for {TimeLimit} turns",
                    "destroy_specific" => "Destroy the enemy commander",
                    _ => "Unknown victory condition"
                };
            }
        }
    }
}

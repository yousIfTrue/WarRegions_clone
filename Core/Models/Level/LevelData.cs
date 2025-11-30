
namespace WarRegions.Core.Models.Level
{
    public partial class LevelData  // جعلها partial لسهولة التعديل
    {
        public string LevelName { get; set; } = "Battle Field";
        public int MapWidth { get; set; } = 10;
        public int MapHeight { get; set; } = 8;
        public string AIDifficulty { get; set; } = "normal";
        public string AIBehavior { get; set; } = "aggressive";
        // أضف في LevelData.cs:
        public int SilverReward { get; set; } = 500;
        public int GoldReward { get; set; } = 50;
        public int TurnsLimit { get; set; } = 30;
        // constructor يأخذ 4 معاملات (كما يتوقع LevelManager)
        // أضف هذه الخصائص لتتوافق مع LevelManager
        public bool IsUnlocked { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public string LevelId { get; set; } = "level_1";
        public string Description { get; set; } = "Default level description";
        public int RecommendedLevel { get; set; } = 1;
        public List<string> RequiredLevels { get; set; } = new List<string>();
        public List<string> RequiredAchievements { get; set; } = new List<string>();
            // ✅ إضافة طرق مساعدة
        public bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x < MapWidth && y >= 0 && y < MapHeight;
        }
        
        public int GetTotalRegions()
        {
            return MapWidth * MapHeight;
        }
            
        public LevelData(string levelName, string AIBehavior, int width, int height)
        {
            this.LevelName = levelName;
            this.AIBehavior = AIBehavior; 
            this.MapWidth = width;
            this.MapHeight = height;
        
            // قيم افتراضية للخصائص الأخرى
            this.AIDifficulty = "medium";
            this.SilverReward = 100;
            this.GoldReward = 10;
            this.TurnsLimit = 20;

        }
        public LevelData() 
        {
            
        }
        
           public List<Vector2> PlayerSpawnPoints { get; set; } = new();
    public List<Vector2> EnemySpawnPoints { get; set; } = new();
    
    public void AddPlayerSpawnPoint(Vector2 point) => PlayerSpawnPoints.Add(point);
    public void AddEnemySpawnPoint(Vector2 point) => EnemySpawnPoints.Add(point);
    
    public bool IsCompleted(PlayerProgress progress) 
    {
        // التفعيل
            if (progress == null) 
        return false;

    // إذا كان المستوى الأول، يكون مفتوح دائماً
    if (LevelId == "level_1" || LevelName == "First Battle")
        return true;

    // إذا كان المستوى مذكور في قائمة المستويات المفتوحة
    if (progress.UnlockedLevels?.Contains(LevelId) == true)
        return true;

    // فتح المستوى إذا اكتمل المستوى السابق
    var previousLevel = GetPreviousLevel();
    if (previousLevel != null && progress.CompletedLevels?.Contains(previousLevel.LevelId) == true)
        return true;

    // في وضع التطوير، جميع المستويات مفتوحة
    return DevConfig.DebugMode;
    }
    
public bool MeetsRequirements(PlayerProgress progress)
{
    if (progress == null) 
        return false;

    // 1. تحقق من مستوى اللاعب
    if (progress.Level < RecommendedLevel)
        return false;

    // 2. تحقق إذا اكتملت المستويات المطلوبة
    if (RequiredLevels?.Any() == true)
    {
        foreach (var requiredLevel in RequiredLevels)
        {
            if (!progress.CompletedLevels?.Contains(requiredLevel) == true)
                return false;
        }
    }

    // 3. تحقق من الإنجازات المطلوبة
    if (RequiredAchievements?.Any() == true)
    {
        foreach (var achievement in RequiredAchievements)
        {
            if (!progress.Achievements?.Contains(achievement) == true)
                return false;
        }
    }

    return true;
}

public Reward CalculateRewards() 
{
    var reward = new Reward
    {
        Silver = SilverReward,
        Gold = GoldReward,
        Experience = CalculateExperienceReward()
    };

    // مكافآت إضافية بناءً على الأداء
    if (HasBonusConditions())
    {
        reward.Silver += (int)(SilverReward * 0.3f); // +30%
        reward.Gold += (int)(GoldReward * 0.2f);     // +20%
    }

    return reward;
}

// دوال مساعدة
private int CalculateExperienceReward()
{
    int baseXP = 100;
    int difficultyMultiplier = AIDifficulty?.ToLower() switch
    {
        "easy" => 1,
        "normal" => 2,
        "hard" => 3,
        _ => 1
    };
    return baseXP * difficultyMultiplier * RecommendedLevel;
}

private bool HasBonusConditions()
{
    // شروط المكافآت الإضافية (يمكن تطويرها لاحقاً)
    return false; // مؤقتاً
}
        
        public class VictoryCondition
        {
            public string Type { get; set; } = "eliminate_all";
            public string Target { get; set; } = "enemy_units";
            public int RequiredCount { get; set; } = 0;
            public int TimeLimit { get; set; } = 0;
            
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

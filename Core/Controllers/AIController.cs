
namespace WarRegions.Core.Controllers
{
    public class AIController
    {
        private GameState _gameState;
        private BattleCalculator _battleCalculator;
        private TerrainManager _terrainManager;
        private Random _random;

        public AIController(GameState gameState, BattleCalculator battleCalculator, TerrainManager terrainManager)
        {
            _gameState = gameState;
            _battleCalculator = battleCalculator;
            _terrainManager = terrainManager;
            _random = new Random();
        }
        // ✅ أضف هذه الدوال المطلوبة:
        public void Initialize(GameState gameState, LevelData levelData)
        {
            _gameState = gameState;
            _levelData = levelData;
            _difficulty = levelData?.AIDifficulty ?? "normal";
            
            Console.WriteLine($"[AI] Initialized with difficulty: {_difficulty}");
        }
        private void MakeRandomMove(Army army)
        {
            if (army.CurrentRegion == null) return;

            // حركة عشوائية في نطاق الحركة
            var possibleMoves = GetPossibleMoves(army);
            if (possibleMoves.Any())
            {
                var randomMove = possibleMoves[_random.Next(possibleMoves.Count)];
                army.MoveTo(randomMove, _terrainManager);
                Console.WriteLine($"[AI] {army.ArmyName} moving randomly to ({randomMove.X}, {randomMove.Y})");
            }
        }
        public void MakeAIMove(Player aiPlayer)
        {
            // منطق AI سيتم إضافته هنا لاحقاً
            if (aiPlayer == null) return;

            var aiArmies = GetAIArmies(aiPlayer);
            
            foreach (var army in aiArmies)
            {
                if (army.IsDefeated) continue;
                
                // 1. ابحث عن هدف
                var target = FindAttackTarget(army, aiPlayer);
                
                if (target != null && army.CanMoveTo(target.Position, _terrainManager))
                {
                    // 2. تحرك نحو الهدف
                    army.MoveTo(target.Position, _terrainManager);
                    Console.WriteLine($"[AI] {army.ArmyName} moving to attack {target.RegionName}");
                }
                else
                {
                    // 3. حركة عشوائية إذا لم يوجد هدف
                    MakeRandomMove(army);
                }
            }
        }
        public void ProcessTurn(GameState gameState)
        {
            _gameState = gameState;
            var aiPlayer = GetAIPlayer();
            
            if (aiPlayer == null)
            {
                Console.WriteLine("[AI] No AI player found");
                return;
            }

            Console.WriteLine($"[AI] Processing turn for {aiPlayer.PlayerName}");
            
            // استدعاء الدالة الحالية مع التحسين
            MakeAIMove(aiPlayer);
        }

        private Army FindStrongestArmy(Player player)
        {
            var playerArmies = GetAIArmies(player);
            return playerArmies.OrderByDescending(a => a.GetStrength()).FirstOrDefault();
        }

        private Region FindWeakestEnemyRegion(Player aiPlayer)
        {
            return null; // مؤقت
        }
        // ✅ أضف دوال مساعدة جديدة:
        private Player GetAIPlayer()
        {
            return _gameState?.Players?.FirstOrDefault(p => p != _gameState.CurrentPlayer);
        }

        private List<Army> GetAIArmies(Player aiPlayer)
        {
            return _gameState?.Armies?.Where(a => a.Owner == aiPlayer && !a.IsDefeated).ToList() 
                   ?? new List<Army>();
        }

        private Region FindAttackTarget(Army army, Player aiPlayer)
        {
            var enemyRegions = _gameState?.Regions?.Where(r => r.Owner != aiPlayer).ToList();
            
            if (enemyRegions == null || !enemyRegions.Any()) 
                return null;

            // ابحث عن أقرب منطقة معادية
            return enemyRegions
                .OrderBy(r => CalculateDistance(army.CurrentRegion.Position, r.Position))
                .FirstOrDefault();
        }

        private List<Vector2> GetPossibleMoves(Army army)
        {
            var moves = new List<Vector2>();
            var currentPos = army.CurrentRegion.Position;
            var range = army.GetCurrentMovementRange();

            for (int x = (int)currentPos.X - range; x <= currentPos.X + range; x++)
            {
                for (int y = (int)currentPos.Y - range; y <= currentPos.Y + range; y++)
                {
                    var target = new Vector2(x, y);
                    if (army.CanMoveTo(target, _terrainManager))
                    {
                        moves.Add(target);
                    }
                }
            }

            return moves;
        }

        private float CalculateDistance(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); // Manhattan distance
        }
    }
}

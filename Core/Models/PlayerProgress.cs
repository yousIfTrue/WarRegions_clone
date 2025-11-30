// Core/Models/Progress/PlayerProgress.cs

namespace WarRegions.Core.Models
{
    public class PlayerProgress
    {
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public List<string> UnlockedLevels { get; set; } = new List<string>();
        public List<string> CompletedLevels { get; set; } = new List<string>();
        public List<string> Achievements { get; set; } = new List<string>();
        
        public PlayerProgress()
        {
            UnlockedLevels.Add("level_1");
        }
    }
}
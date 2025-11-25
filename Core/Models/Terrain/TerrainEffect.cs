
namespace WarRegions.Core.Models.Terrain
{
    public class TerrainEffect
    {
        public string EffectName { get; set; }
        public int MovementModifier { get; set; }
        public int DefenseModifier { get; set; }
        public int AttackModifier { get; set; }
        
        public TerrainEffect()
        {
            EffectName = "Default";
            MovementModifier = 0;
            DefenseModifier = 0;
            AttackModifier = 0;
        }
    }
}




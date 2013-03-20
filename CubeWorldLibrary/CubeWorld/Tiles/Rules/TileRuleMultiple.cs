using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleMultiple : TileRule
    {
        public TileRule[] otherRules;

        public TileRuleMultiple()
        {
        }

        public TileRuleMultiple(TileRule[] otherRules, TileRuleCondition condition)
            : base(condition)
        {
            this.otherRules = otherRules;
        }

        public override void Execute(TileManager tileManager, Tile tile, TilePosition pos)
        {
            foreach (TileRule rule in otherRules)
                if (rule.CheckConditions(tileManager, tile, pos))
                    rule.Execute(tileManager, tile, pos);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref otherRules, "otherRules");
        }
    }
}
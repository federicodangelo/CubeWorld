using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleConditionNot : TileRuleCondition
    {
        public TileRuleCondition condition;

        public TileRuleConditionNot()
        {
        }

        public TileRuleConditionNot(TileRuleCondition condition)
        {
            this.condition = condition;
        }

        public override bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            tileManager.world.stats.checkedConditions++;

            return !condition.Validate(tileManager, tile, pos);
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref condition, "condition");
        }
    }
}
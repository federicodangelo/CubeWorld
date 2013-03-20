using CubeWorld.Tiles;
using CubeWorld.Serialization;

namespace CubeWorld.Tiles.Rules
{
    public class TileRuleConditionOr : TileRuleCondition
    {
        public TileRuleCondition[] conditions;

        public TileRuleConditionOr()
        {
        }

        public TileRuleConditionOr(TileRuleCondition[] conditions)
        {
            this.conditions = conditions;
        }

        public override bool Validate(TileManager tileManager, Tile tile, TilePosition pos)
        {
            tileManager.world.stats.checkedConditions++;

            foreach (TileRuleCondition condition in conditions)
                if (condition.Validate(tileManager, tile, pos))
                    return true;

            return false;
        }

        public override void Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.Serialize(ref conditions, "conditions");
        }
    }
}